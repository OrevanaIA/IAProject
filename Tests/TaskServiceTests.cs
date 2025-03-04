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
    [TestClass]
    public class TaskServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ITaskRepository> _mockTaskRepository;
        private Mock<ITaskValidator> _mockTaskValidator;
        private Mock<ICacheService> _mockCacheService;
        private Mock<ISecurityLogger> _mockSecurityLogger;
        private TaskService _taskService;

        [TestInitialize]
        public void Initialize()
        {
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
        }

        #region Creación de Tareas

        [TestMethod]
        public async Task CreateTaskAsync_ValidDTO_CreatesTask()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Nueva Tarea",
                Status = AIProject.Models.TaskStatus.Pending,
                Priority = Priority.Media
            };

            // Act
            var result = await _taskService.CreateTaskAsync(taskDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(taskDto.Id, result.Id);
            Assert.AreEqual(taskDto.Description, result.Description);
            _mockTaskRepository.Verify(r => r.Add(taskDto), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Once);
            _mockSecurityLogger.Verify(l => l.LogDataChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<TaskDTO>(),
                It.IsAny<TimeSpan>()
            ), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateTaskAsync_NullDTO_ThrowsException()
        {
            // Act
            await _taskService.CreateTaskAsync(null);
        }

        [TestMethod]
        public async Task CreateTaskAsync_ValidationFails_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Short", // Too short description
                Status = AIProject.Models.TaskStatus.Pending
            };

            _mockTaskValidator.Setup(v => v.ValidateTaskAsync(taskDto))
                .ThrowsAsync(new ValidationException("Description must be at least 10 characters"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _taskService.CreateTaskAsync(taskDto);
            });

            _mockTaskRepository.Verify(r => r.Add(It.IsAny<TaskDTO>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Never);
        }

        [TestMethod]
        public async Task CreateTaskAsync_RepositoryError_RollsBackTransaction()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Nueva Tarea",
                Status = AIProject.Models.TaskStatus.Pending
            };

            _mockTaskRepository.Setup(r => r.Add(taskDto))
                .Throws(new InvalidOperationException("Repository error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await _taskService.CreateTaskAsync(taskDto);
            });

            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Never);
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        #endregion

        #region Listado de Tareas

        [TestMethod]
        public async Task GetTaskAsync_ExistingId_ReturnsTask()
        {
            // Arrange
            int taskId = 1;
            var expectedTask = new TaskDTO
            {
                Id = taskId,
                Description = "Tarea Existente",
                Status = AIProject.Models.TaskStatus.InProgress
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
            _mockCacheService.Verify(c => c.GetAsync<TaskDTO>(It.IsAny<string>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<TaskDTO>(),
                It.IsAny<TimeSpan>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task GetTaskAsync_CachedTask_ReturnsCachedTask()
        {
            // Arrange
            int taskId = 1;
            var cachedTask = new TaskDTO
            {
                Id = taskId,
                Description = "Tarea en Caché",
                Status = AIProject.Models.TaskStatus.Completed
            };

            _mockCacheService.Setup(c => c.GetAsync<TaskDTO>(It.IsAny<string>()))
                .ReturnsAsync(cachedTask);

            // Act
            var result = await _taskService.GetTaskAsync(taskId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(cachedTask.Id, result.Id);
            Assert.AreEqual(cachedTask.Description, result.Description);
            Assert.AreEqual(cachedTask.Status, result.Status);
            _mockTaskRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
            _mockCacheService.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<TaskDTO>(),
                It.IsAny<TimeSpan>()
            ), Times.Never);
        }

        [TestMethod]
        public async Task GetTaskAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            int taskId = 999;
            _mockCacheService.Setup(c => c.GetAsync<TaskDTO>(It.IsAny<string>()))
                .ReturnsAsync((TaskDTO)null);
            _mockTaskRepository.Setup(r => r.GetById(taskId))
                .Returns((TaskDTO)null);

            // Act
            var result = await _taskService.GetTaskAsync(taskId);

            // Assert
            Assert.IsNull(result);
            _mockCacheService.Verify(c => c.GetAsync<TaskDTO>(It.IsAny<string>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<TaskDTO>(),
                It.IsAny<TimeSpan>()
            ), Times.Never);
        }

        [TestMethod]
        public async Task GetAllTasksAsync_ReturnsTasks()
        {
            // Arrange
            var tasks = new List<TaskDTO>
            {
                new TaskDTO { Id = 1, Description = "Tarea 1", Status = AIProject.Models.TaskStatus.Pending },
                new TaskDTO { Id = 2, Description = "Tarea 2", Status = AIProject.Models.TaskStatus.InProgress }
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
            _mockCacheService.Verify(c => c.GetAsync<IEnumerable<TaskDTO>>(It.IsAny<string>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<TaskDTO>>(),
                It.IsAny<TimeSpan>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task SearchTasksAsync_ValidTerm_ReturnsTasks()
        {
            // Arrange
            string searchTerm = "Implementar";
            var tasks = new List<TaskDTO>
            {
                new TaskDTO { Id = 1, Description = "Implementar funcionalidad", Status = AIProject.Models.TaskStatus.Pending },
                new TaskDTO { Id = 3, Description = "Implementar tests", Status = AIProject.Models.TaskStatus.Completed }
            };

            _mockCacheService.Setup(c => c.GetAsync<IEnumerable<TaskDTO>>(It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<TaskDTO>)null);
            _mockTaskRepository.Setup(r => r.Search(searchTerm))
                .Returns(tasks);

            // Act
            var result = await _taskService.SearchTasksAsync(searchTerm);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(tasks.Count, result.Count());
            _mockTaskRepository.Verify(r => r.Search(searchTerm), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SearchTasksAsync_EmptyTerm_ThrowsException()
        {
            // Act
            await _taskService.SearchTasksAsync("");
        }

        #endregion

        #region Actualización de Tareas

        [TestMethod]
        public async Task UpdateTaskAsync_ValidDTO_UpdatesTask()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Tarea Actualizada",
                Status = AIProject.Models.TaskStatus.InProgress,
                Priority = Priority.Alta
            };

            // Act
            await _taskService.UpdateTaskAsync(taskDto);

            // Assert
            _mockTaskValidator.Verify(v => v.ValidateTaskAsync(taskDto), Times.Once);
            _mockTaskRepository.Verify(r => r.Update(taskDto), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Once);
            _mockSecurityLogger.Verify(l => l.LogDataChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateTaskAsync_NullDTO_ThrowsException()
        {
            // Act
            await _taskService.UpdateTaskAsync(null);
        }

        [TestMethod]
        public async Task UpdateTaskStatusAsync_ExistingId_UpdatesStatus()
        {
            // Arrange
            int taskId = 1;
            AIProject.Models.TaskStatus newStatus = AIProject.Models.TaskStatus.Completed;

            // Act
            await _taskService.UpdateTaskStatusAsync(taskId, newStatus);

            // Assert
            _mockTaskRepository.Verify(r => r.UpdateStatus(taskId, newStatus), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Once);
            _mockSecurityLogger.Verify(l => l.LogDataChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTaskPriorityAsync_ExistingId_UpdatesPriority()
        {
            // Arrange
            int taskId = 1;
            Priority newPriority = Priority.Alta;

            // Act
            await _taskService.UpdateTaskPriorityAsync(taskId, newPriority);

            // Assert
            _mockTaskRepository.Verify(r => r.UpdatePriority(taskId, newPriority), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Once);
            _mockSecurityLogger.Verify(l => l.LogDataChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task AddCategoryToTaskAsync_ExistingId_AddsCategory()
        {
            // Arrange
            int taskId = 1;
            string category = "Importante";

            // Act
            await _taskService.AddCategoryToTaskAsync(taskId, category);

            // Assert
            _mockTaskRepository.Verify(r => r.AddCategory(taskId, category), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Once);
            _mockSecurityLogger.Verify(l => l.LogDataChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddCategoryToTaskAsync_EmptyCategory_ThrowsException()
        {
            // Act
            await _taskService.AddCategoryToTaskAsync(1, "");
        }

        #endregion

        #region Eliminación de Tareas

        [TestMethod]
        public async Task DeleteTaskAsync_ExistingId_DeletesTask()
        {
            // Arrange
            int taskId = 1;

            // Act
            await _taskService.DeleteTaskAsync(taskId);

            // Assert
            _mockTaskRepository.Verify(r => r.Delete(taskId), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransaction(), Times.Once);
            _mockSecurityLogger.Verify(l => l.LogDataChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Seguridad y Manejo de Errores

        [TestMethod]
        public async Task ExecuteInTransaction_Error_LogsError()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Tarea con Error",
                Status = AIProject.Models.TaskStatus.Pending
            };

            _mockTaskRepository.Setup(r => r.Add(taskDto))
                .Throws(new Exception("Test error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _taskService.CreateTaskAsync(taskDto);
            });

            _mockUnitOfWork.Verify(u => u.RollbackTransaction(), Times.Once);
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task TaskOperations_LogPerformanceMetrics()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Tarea para Métricas",
                Status = AIProject.Models.TaskStatus.Pending
            };

            // Act
            await _taskService.CreateTaskAsync(taskDto);

            // Assert
            _mockSecurityLogger.Verify(l => l.LogPerformanceMetricAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        #endregion
    }
}
