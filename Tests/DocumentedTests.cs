using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Services;
using AIProject.Models;

namespace AIProject.Tests
{
    /// <summary>
    /// Implementación de los casos de prueba documentados para el proyecto Sprint02Tasks
    /// </summary>
    [TestClass]
    public class DocumentedTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ITaskRepository> _mockTaskRepository;
        private Mock<ITaskValidator> _mockTaskValidator;
        private Mock<ICacheService> _mockCacheService;
        private Mock<ISecurityLogger> _mockSecurityLogger;
        private TaskService _taskService;
        private TaskRepository _repository;

        [TestInitialize]
        public void Initialize()
        {
            // Setup for service tests
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTaskValidator = new Mock<ITaskValidator>();
            _mockCacheService = new Mock<ICacheService>();
            _mockSecurityLogger = new Mock<ISecurityLogger>();

            _mockUnitOfWork.Setup(u => u.TaskRepository).Returns(_mockTaskRepository.Object);

            _taskService = new TaskService(
                _mockUnitOfWork.Object,
                _mockTaskValidator.Object,
                _mockCacheService.Object,
                _mockSecurityLogger.Object
            );

            // Setup for repository tests
            _repository = new TaskRepository();
        }

        #region 3. Creación de Tareas

        /// <summary>
        /// CU_001: Verificar que una tarea se crea correctamente con datos válidos
        /// </summary>
        [TestMethod]
        public async Task CU_001_CrearTarea_DatosValidos_CreaTareaConExito()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Nueva Tarea",
                Status = AIProject.Models.TaskStatus.Pending
            };

            // Act
            var result = await _taskService.CreateTaskAsync(taskDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(taskDto.Id, result.Id);
            Assert.AreEqual(taskDto.Description, result.Description);
            Assert.AreEqual(taskDto.Status, result.Status);
            _mockTaskRepository.Verify(r => r.Add(taskDto), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// CU_002: Verificar que se lanza una excepción al intentar crear una tarea con descripción inválida
        /// </summary>
        [TestMethod]
        public async Task CU_002_CrearTarea_DescripcionInvalida_LanzaExcepcion()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Corta", // Descripción demasiado corta
                Status = AIProject.Models.TaskStatus.Pending
            };

            _mockTaskValidator.Setup(v => v.ValidateTaskAsync(taskDto))
                .ThrowsAsync(new ValidationException("La descripción debe tener al menos 10 caracteres"));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _taskService.CreateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("descripción"));
            _mockTaskRepository.Verify(r => r.Add(It.IsAny<TaskDTO>()), Times.Never);
        }

        /// <summary>
        /// CU_003: Verificar que se lanza una excepción al intentar crear una tarea con estado inválido
        /// </summary>
        [TestMethod]
        public async Task CU_003_CrearTarea_EstadoInvalido_LanzaExcepcion()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Descripción válida de la tarea",
                Status = (AIProject.Models.TaskStatus)999 // Estado inválido
            };

            _mockTaskValidator.Setup(v => v.ValidateTaskAsync(taskDto))
                .ThrowsAsync(new ValidationException("El estado de la tarea no es válido"));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _taskService.CreateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("estado"));
            _mockTaskRepository.Verify(r => r.Add(It.IsAny<TaskDTO>()), Times.Never);
        }

        /// <summary>
        /// CU_019: Verificar que se lanza una excepción al intentar crear una tarea con título demasiado largo
        /// </summary>
        /// <remarks>
        /// Este test utiliza reflexión para obtener dinámicamente el valor máximo permitido para la longitud
        /// del título desde la clase TaskValidator, lo que hace que el test sea más adaptable a cambios futuros.
        /// </remarks>
        [TestMethod]
        public async Task CrearTarea_TituloLargo_LanzaExcepcion()
        {
            // Arrange
            // Obtener dinámicamente el valor máximo de longitud de descripción desde TaskValidator
            var taskValidatorType = typeof(TaskValidator);
            var maxLengthField = taskValidatorType.GetField("MaxDescriptionLength", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            int maxLength = 100; // Valor por defecto en caso de que no se pueda obtener por reflexión
            if (maxLengthField != null)
            {
                maxLength = (int)maxLengthField.GetValue(null);
            }
            
            // Crear una descripción que excede el límite máximo
            string longDescription = new string('a', maxLength + 1);
            
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = longDescription,
                Status = AIProject.Models.TaskStatus.Pending
            };

            _mockTaskValidator.Setup(v => v.ValidateTaskAsync(It.IsAny<TaskDTO>()))
                .Callback<TaskDTO>(task => 
                {
                    if (task.Description.Length > maxLength)
                    {
                        throw new ValidationException($"Description must be between 10 and {maxLength} characters");
                    }
                })
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _taskService.CreateTaskAsync(taskDto);
            });

            // Verificar que el mensaje de error contiene la longitud máxima dinámica
            Assert.IsTrue(exception.Message.Contains(maxLength.ToString()));
            _mockTaskRepository.Verify(r => r.Add(It.IsAny<TaskDTO>()), Times.Never);
        }

        #endregion

        #region 4. Listado de Tareas

        /// <summary>
        /// CU_004: Verificar que se obtiene una tarea existente por su ID
        /// </summary>
        [TestMethod]
        public async Task CU_004_ObtenerTarea_IdExistente_DevuelveTarea()
        {
            // Arrange
            int taskId = 1;
            var expectedTask = new TaskDTO
            {
                Id = taskId,
                Description = "Tarea existente",
                Status = AIProject.Models.TaskStatus.Pending
            };

            _mockCacheService.Setup(c => c.GetAsync<TaskDTO>(It.IsAny<string>()))
                .ReturnsAsync((TaskDTO)null);
            _mockTaskRepository.Setup(r => r.GetById(taskId))
                .Returns(expectedTask);

            // Act
            var result = await _taskService.GetTaskAsync(taskId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedTask.Id, result.Id);
            Assert.AreEqual(expectedTask.Description, result.Description);
            Assert.AreEqual(expectedTask.Status, result.Status);
        }

        /// <summary>
        /// CU_005: Verificar que se devuelve null al buscar una tarea con ID inexistente
        /// </summary>
        [TestMethod]
        public async Task CU_005_ObtenerTarea_IdInexistente_DevuelveNull()
        {
            // Arrange
            int taskId = 999; // ID inexistente
            _mockCacheService.Setup(c => c.GetAsync<TaskDTO>(It.IsAny<string>()))
                .ReturnsAsync((TaskDTO)null);
            _mockTaskRepository.Setup(r => r.GetById(taskId))
                .Returns((TaskDTO)null);

            // Act
            var result = await _taskService.GetTaskAsync(taskId);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// CU_006: Verificar que se obtienen todas las tareas correctamente
        /// </summary>
        [TestMethod]
        public async Task CU_006_ObtenerTodasLasTareas_DevuelveListaCompleta()
        {
            // Arrange
            var tasks = new List<TaskDTO>
            {
                new TaskDTO { Id = 1, Description = "Tarea 1", Status = AIProject.Models.TaskStatus.Pending },
                new TaskDTO { Id = 2, Description = "Tarea 2", Status = AIProject.Models.TaskStatus.InProgress },
                new TaskDTO { Id = 3, Description = "Tarea 3", Status = AIProject.Models.TaskStatus.Completed }
            };

            _mockCacheService.Setup(c => c.GetAsync<IEnumerable<TaskDTO>>(It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<TaskDTO>)null);
            _mockTaskRepository.Setup(r => r.GetAll())
                .Returns(tasks);

            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(tasks.Count, result.Count());
            CollectionAssert.AreEqual(tasks.Select(t => t.Id).ToList(), result.Select(t => t.Id).ToList());
        }

        /// <summary>
        /// CU_007: Verificar que se filtran las tareas correctamente por estado
        /// </summary>
        [TestMethod]
        public void CU_007_FiltrarTareasPorEstado_DevuelveTareasFiltradas()
        {
            // Arrange
            _repository.AddTask("Tarea pendiente", AIProject.Models.TaskStatus.Pending);
            _repository.AddTask("Tarea en progreso", AIProject.Models.TaskStatus.InProgress);
            _repository.AddTask("Tarea completada", AIProject.Models.TaskStatus.Completed);
            _repository.AddTask("Otra tarea pendiente", AIProject.Models.TaskStatus.Pending);

            // Act
            var pendingTasks = _repository.ListTasks(statusFilter: AIProject.Models.TaskStatus.Pending);
            var inProgressTasks = _repository.ListTasks(statusFilter: AIProject.Models.TaskStatus.InProgress);
            var completedTasks = _repository.ListTasks(statusFilter: AIProject.Models.TaskStatus.Completed);

            // Assert
            Assert.AreEqual(2, pendingTasks.Count);
            Assert.AreEqual(1, inProgressTasks.Count);
            Assert.AreEqual(1, completedTasks.Count);
            Assert.IsTrue(pendingTasks.All(t => t.Status == AIProject.Models.TaskStatus.Pending));
            Assert.IsTrue(inProgressTasks.All(t => t.Status == AIProject.Models.TaskStatus.InProgress));
            Assert.IsTrue(completedTasks.All(t => t.Status == AIProject.Models.TaskStatus.Completed));
        }

        /// <summary>
        /// CU_008: Verificar que la búsqueda de tareas por término funciona correctamente
        /// </summary>
        [TestMethod]
        public async Task CU_008_BuscarTareas_TerminoValido_DevuelveTareasCoincidentes()
        {
            // Arrange
            string searchTerm = "importante";
            var matchingTasks = new List<TaskDTO>
            {
                new TaskDTO { Id = 1, Description = "Tarea importante 1", Status = AIProject.Models.TaskStatus.Pending },
                new TaskDTO { Id = 3, Description = "Otra tarea importante", Status = AIProject.Models.TaskStatus.Completed }
            };

            _mockCacheService.Setup(c => c.GetAsync<IEnumerable<TaskDTO>>(It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<TaskDTO>)null);
            _mockTaskRepository.Setup(r => r.Search(searchTerm))
                .Returns(matchingTasks);

            // Act
            var result = await _taskService.SearchTasksAsync(searchTerm);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(matchingTasks.Count, result.Count());
            CollectionAssert.AreEqual(matchingTasks.Select(t => t.Id).ToList(), result.Select(t => t.Id).ToList());
        }

        #endregion

        #region 5. Actualización de Tareas

        /// <summary>
        /// CU_009: Verificar que se actualiza correctamente una tarea existente
        /// </summary>
        [TestMethod]
        public async Task CU_009_ActualizarTarea_TareaExistente_ActualizaCorrectamente()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Tarea actualizada",
                Status = AIProject.Models.TaskStatus.InProgress,
                Priority = Priority.Alta
            };

            // Act
            await _taskService.UpdateTaskAsync(taskDto);

            // Assert
            _mockTaskValidator.Verify(v => v.ValidateTaskAsync(taskDto), Times.Once);
            _mockTaskRepository.Verify(r => r.Update(taskDto), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// CU_010: Verificar que se actualiza correctamente el estado de una tarea
        /// </summary>
        [TestMethod]
        public async Task CU_010_ActualizarEstadoTarea_ActualizaCorrectamente()
        {
            // Arrange
            int taskId = 1;
            AIProject.Models.TaskStatus newStatus = AIProject.Models.TaskStatus.Completed;

            // Act
            await _taskService.UpdateTaskStatusAsync(taskId, newStatus);

            // Assert
            _mockTaskRepository.Verify(r => r.UpdateStatus(taskId, newStatus), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// CU_011: Verificar que se actualiza correctamente la prioridad de una tarea
        /// </summary>
        [TestMethod]
        public async Task CU_011_ActualizarPrioridadTarea_ActualizaCorrectamente()
        {
            // Arrange
            int taskId = 1;
            Priority newPriority = Priority.Alta;

            // Act
            await _taskService.UpdateTaskPriorityAsync(taskId, newPriority);

            // Assert
            _mockTaskRepository.Verify(r => r.UpdatePriority(taskId, newPriority), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// CU_012: Verificar que se añade correctamente una categoría a una tarea
        /// </summary>
        [TestMethod]
        public async Task CU_012_AgregarCategoriaTarea_AgregaCorrectamente()
        {
            // Arrange
            int taskId = 1;
            string category = "Importante";

            // Act
            await _taskService.AddCategoryToTaskAsync(taskId, category);

            // Assert
            _mockTaskRepository.Verify(r => r.AddCategory(taskId, category), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region 6. Eliminación de Tareas

        /// <summary>
        /// CU_013: Verificar que se elimina correctamente una tarea existente
        /// </summary>
        [TestMethod]
        public async Task CU_013_EliminarTarea_TareaExistente_EliminaCorrectamente()
        {
            // Arrange
            int taskId = 1;

            // Act
            await _taskService.DeleteTaskAsync(taskId);

            // Assert
            _mockTaskRepository.Verify(r => r.Delete(taskId), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// CU_014: Verificar que no se produce error al intentar eliminar una tarea inexistente
        /// </summary>
        [TestMethod]
        public void CU_014_EliminarTarea_TareaInexistente_NoProduceError()
        {
            // Arrange
            int nonExistingTaskId = 999;

            // Act
            bool result = _repository.DeleteTask(nonExistingTaskId, false);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region 7. Seguridad y Manejo de Errores

        /// <summary>
        /// CU_015: Verificar que se sanitiza correctamente la entrada de texto
        /// </summary>
        [TestMethod]
        public void CU_015_SanitizarEntrada_EntradaConScriptMalicioso_EliminaContenidoPeligroso()
        {
            // Arrange
            string input = "Tarea con <script>alert('XSS')</script> código malicioso";
            string expected = "Tarea con  código malicioso";

            // Act
            string result = AIProject.Security.InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// CU_016: Verificar que se registran correctamente los cambios en los datos
        /// </summary>
        [TestMethod]
        public async Task CU_016_LoggingSeguridad_CambiosDatos_RegistraCorrectamente()
        {
            // Arrange
            var logger = new ConsoleSecurityLogger();
            string entityType = "Task";
            string entityId = "1";
            string changeDetails = "Updated task status to Completed";
            string user = "testuser";

            // Act & Assert
            // Since we can't easily test console output, we just ensure no exception is thrown
            await logger.LogDataChangeAsync(entityType, entityId, changeDetails, user);
        }

        /// <summary>
        /// CU_017: Verificar que se registran correctamente las violaciones de seguridad
        /// </summary>
        [TestMethod]
        public async Task CU_017_LoggingSeguridad_ViolacionSeguridad_RegistraCorrectamente()
        {
            // Arrange
            var logger = new ConsoleSecurityLogger();
            string resource = "TaskAPI";
            string ipAddress = "192.168.1.1";
            string additionalInfo = "Unauthorized access attempt";

            // Act & Assert
            // Since we can't easily test console output, we just ensure no exception is thrown
            await logger.LogSecurityViolationAsync(resource, ipAddress, additionalInfo);
        }

        /// <summary>
        /// CU_018: Verificar que se manejan correctamente las excepciones durante las operaciones
        /// </summary>
        [TestMethod]
        public async Task CU_018_ManejoErrores_ExcepcionEnOperacion_RollbackTransaccion()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Tarea con error",
                Status = AIProject.Models.TaskStatus.Pending
            };

            _mockTaskRepository.Setup(r => r.Add(taskDto))
                .Throws(new Exception("Error simulado"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _taskService.CreateTaskAsync(taskDto);
            });

            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Never);
        }

        #endregion
    }
}
