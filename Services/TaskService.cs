using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Models;

namespace AIProject.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de tareas que proporciona operaciones CRUD y funcionalidades adicionales.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa la interfaz ITaskService y proporciona:
    /// - Operaciones CRUD completas para tareas
    /// - Gestión de transacciones para garantizar la integridad de los datos
    /// - Caché para mejorar el rendimiento
    /// - Registro de seguridad para auditoría
    /// - Validación de datos para garantizar la integridad
    /// </remarks>
    /// <example>
    /// Ejemplo de uso básico:
    /// <code>
    /// var taskService = new TaskService(unitOfWork, validator, cacheService, securityLogger);
    /// var newTask = new TaskDTO { Description = "Nueva tarea", Status = TaskStatus.Pending };
    /// var createdTask = await taskService.CreateTaskAsync(newTask);
    /// </code>
    /// </example>
    public class TaskService : ITaskService
    {
        #region Implementación de métodos síncronos

        /// <summary>
        /// Crea una nueva tarea en el sistema.
        /// </summary>
        /// <param name="taskDto">DTO con la información de la tarea a crear</param>
        /// <returns>DTO de la tarea creada, incluyendo el ID asignado</returns>
        /// <remarks>
        /// Este método es un wrapper síncrono que internamente llama al método asíncrono.
        /// </remarks>
        public TaskDTO CreateTask(TaskDTO taskDto)
        {
            return CreateTaskAsync(taskDto).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Obtiene todas las tareas existentes en el sistema.
        /// </summary>
        /// <returns>Colección de DTOs de todas las tareas</returns>
        /// <remarks>
        /// Este método es un wrapper síncrono que internamente llama al método asíncrono.
        /// </remarks>
        public IEnumerable<TaskDTO> GetAllTasks()
        {
            return GetAllTasksAsync().GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Actualiza el estado de una tarea.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="status">Nuevo estado de la tarea</param>
        /// <remarks>
        /// Este método es un wrapper síncrono que internamente llama al método asíncrono.
        /// </remarks>
        public void UpdateTaskStatus(int taskId, AIProject.Models.TaskStatus status)
        {
            UpdateTaskStatusAsync(taskId, status).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Elimina una tarea por su ID.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar</param>
        /// <remarks>
        /// Este método es un wrapper síncrono que internamente llama al método asíncrono.
        /// </remarks>
        public void DeleteTask(int id)
        {
            DeleteTaskAsync(id).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Añade una categoría a una tarea existente.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="category">Categoría a añadir</param>
        /// <remarks>
        /// Este método es un wrapper síncrono que internamente llama al método asíncrono.
        /// </remarks>
        public void AddCategoryToTask(int taskId, string category)
        {
            AddCategoryToTaskAsync(taskId, category).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Actualiza la prioridad de una tarea.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="priority">Nueva prioridad de la tarea</param>
        /// <remarks>
        /// Este método es un wrapper síncrono que internamente llama al método asíncrono.
        /// </remarks>
        public void UpdateTaskPriority(int taskId, AIProject.Models.Priority priority)
        {
            UpdateTaskPriorityAsync(taskId, priority).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Busca tareas que contengan el término especificado en su título o descripción.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con la búsqueda</returns>
        /// <remarks>
        /// Este método es un wrapper síncrono que internamente llama al método asíncrono.
        /// </remarks>
        public IEnumerable<TaskDTO> SearchTasks(string searchTerm)
        {
            return SearchTasksAsync(searchTerm).GetAwaiter().GetResult();
        }

        #endregion
        
        #region Campos privados y constructor

        /// <summary>
        /// Unidad de trabajo para gestionar transacciones y acceso a repositorios.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Validador de tareas para garantizar la integridad de los datos.
        /// </summary>
        private readonly ITaskValidator _taskValidator;

        /// <summary>
        /// Servicio de caché para mejorar el rendimiento.
        /// </summary>
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Registrador de seguridad para auditoría y seguimiento.
        /// </summary>
        private readonly ISecurityLogger _securityLogger;

        /// <summary>
        /// Tiempo de expiración predeterminado para elementos en caché.
        /// </summary>
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Constructor que inicializa una nueva instancia del servicio de tareas.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para gestionar transacciones</param>
        /// <param name="taskValidator">Validador de tareas</param>
        /// <param name="cacheService">Servicio de caché</param>
        /// <param name="securityLogger">Registrador de seguridad</param>
        /// <exception cref="ArgumentNullException">Si alguno de los parámetros es null</exception>
        public TaskService(
            IUnitOfWork unitOfWork,
            ITaskValidator taskValidator,
            ICacheService cacheService,
            ISecurityLogger securityLogger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _taskValidator = taskValidator ?? throw new ArgumentNullException(nameof(taskValidator));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _securityLogger = securityLogger ?? throw new ArgumentNullException(nameof(securityLogger));
        }

        #endregion

        #region Métodos privados de utilidad

        /// <summary>
        /// Genera una clave de caché para una tarea específica.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <returns>Clave de caché única para la tarea</returns>
        private string GetCacheKey(int taskId) => $"task_{taskId}";

        /// <summary>
        /// Genera una clave de caché para una lista de tareas filtrada.
        /// </summary>
        /// <param name="type">Tipo de filtro (ej: "status", "priority")</param>
        /// <param name="value">Valor del filtro</param>
        /// <returns>Clave de caché única para la lista filtrada</returns>
        private string GetListCacheKey(string type, string value) => $"tasks_{type}_{value}";

        /// <summary>
        /// Ejecuta una acción dentro de una transacción con manejo de errores.
        /// </summary>
        /// <param name="action">Acción a ejecutar</param>
        /// <param name="operationName">Nombre de la operación para registro</param>
        /// <returns>Tarea asíncrona</returns>
        /// <exception cref="Exception">Cualquier excepción que ocurra durante la ejecución</exception>
        /// <remarks>
        /// Este método:
        /// - Inicia una transacción
        /// - Ejecuta la acción proporcionada
        /// - Guarda los cambios y confirma la transacción si todo es exitoso
        /// - Revierte la transacción y registra el error en caso de excepción
        /// </remarks>
        private async Task ExecuteInTransactionAsync(Func<Task> action, string operationName)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                await action();
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                await _securityLogger.LogOperationAsync(
                    operationName,
                    $"Error: {ex.Message}",
                    "system"
                );
                throw;
            }
        }

        #endregion

        #region Implementación de métodos asíncronos

        /// <summary>
        /// Crea una nueva tarea en el sistema de forma asíncrona.
        /// </summary>
        /// <param name="taskDto">DTO con la información de la tarea a crear</param>
        /// <returns>DTO de la tarea creada, incluyendo el ID asignado</returns>
        /// <exception cref="ArgumentNullException">Si taskDto es null</exception>
        /// <exception cref="ValidationException">Si los datos de la tarea no son válidos</exception>
        /// <remarks>
        /// Este método:
        /// - Valida los datos de la tarea
        /// - Crea la tarea en una transacción
        /// - Registra la operación para auditoría
        /// - Almacena la tarea en caché para acceso rápido
        /// - Mide y registra el rendimiento de la operación
        /// </remarks>
        public async Task<TaskDTO> CreateTaskAsync(TaskDTO taskDto)
        {
            if (taskDto == null)
                throw new ArgumentNullException(nameof(taskDto));

            await _taskValidator.ValidateTaskAsync(taskDto);

            var startTime = DateTime.UtcNow;
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.Add(taskDto);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskDto.Id.ToString(),
                    "Created new task",
                    "system"
                );
            }, "CreateTask");

            var duration = DateTime.UtcNow - startTime;
            await _securityLogger.LogPerformanceMetricAsync(
                "CreateTask",
                duration,
                $"Task ID: {taskDto.Id}"
            );

            await _cacheService.SetAsync(GetCacheKey(taskDto.Id), taskDto, _cacheExpiration);
            return taskDto;
        }

        /// <summary>
        /// Obtiene una tarea específica por su ID de forma asíncrona.
        /// </summary>
        /// <param name="id">ID de la tarea a obtener</param>
        /// <returns>DTO de la tarea encontrada o null si no existe</returns>
        /// <remarks>
        /// Este método:
        /// - Primero intenta obtener la tarea desde la caché
        /// - Si no está en caché, la obtiene del repositorio
        /// - Si la encuentra en el repositorio, la almacena en caché para futuras consultas
        /// </remarks>
        public async Task<TaskDTO> GetTaskAsync(int id)
        {
            var cacheKey = GetCacheKey(id);
            var cachedTask = await _cacheService.GetAsync<TaskDTO>(cacheKey);
            
            if (cachedTask != null)
                return cachedTask;

            var task = _unitOfWork.TaskRepository.GetById(id);
            if (task != null)
            {
                await _cacheService.SetAsync(cacheKey, task, _cacheExpiration);
            }
            return task;
        }

        /// <summary>
        /// Obtiene todas las tareas existentes en el sistema de forma asíncrona.
        /// </summary>
        /// <returns>Colección de DTOs de todas las tareas</returns>
        /// <remarks>
        /// Este método:
        /// - Primero intenta obtener la lista completa desde la caché
        /// - Si no está en caché, la obtiene del repositorio
        /// - Almacena el resultado en caché para futuras consultas
        /// </remarks>
        public async Task<IEnumerable<TaskDTO>> GetAllTasksAsync()
        {
            const string cacheKey = "all_tasks";
            var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = _unitOfWork.TaskRepository.GetAll();
            await _cacheService.SetAsync(cacheKey, tasks, _cacheExpiration);
            return tasks;
        }

        /// <summary>
        /// Obtiene las tareas filtradas por su estado de forma asíncrona.
        /// </summary>
        /// <param name="status">Estado de las tareas a buscar</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con el estado</returns>
        /// <remarks>
        /// Este método:
        /// - Primero intenta obtener la lista filtrada desde la caché
        /// - Si no está en caché, la obtiene del repositorio
        /// - Almacena el resultado en caché para futuras consultas
        /// </remarks>
        public async Task<IEnumerable<TaskDTO>> GetTasksByStatusAsync(AIProject.Models.TaskStatus status)
        {
            var cacheKey = GetListCacheKey("status", status.ToString());
            var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = _unitOfWork.TaskRepository.GetByStatus(status);
            await _cacheService.SetAsync(cacheKey, tasks, _cacheExpiration);
            return tasks;
        }

        /// <summary>
        /// Obtiene las tareas filtradas por su prioridad de forma asíncrona.
        /// </summary>
        /// <param name="priority">Prioridad de las tareas a buscar</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con la prioridad</returns>
        /// <remarks>
        /// Este método:
        /// - Primero intenta obtener la lista filtrada desde la caché
        /// - Si no está en caché, la obtiene del repositorio
        /// - Almacena el resultado en caché para futuras consultas
        /// </remarks>
        public async Task<IEnumerable<TaskDTO>> GetTasksByPriorityAsync(AIProject.Models.Priority priority)
        {
            var cacheKey = GetListCacheKey("priority", priority.ToString());
            var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = _unitOfWork.TaskRepository.GetByPriority(priority);
            await _cacheService.SetAsync(cacheKey, tasks, _cacheExpiration);
            return tasks;
        }

        /// <summary>
        /// Busca tareas que contengan el término especificado en su título o descripción de forma asíncrona.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con la búsqueda</returns>
        /// <exception cref="ArgumentException">Si searchTerm es null o vacío</exception>
        /// <remarks>
        /// Este método:
        /// - Valida que el término de búsqueda no esté vacío
        /// - Primero intenta obtener los resultados desde la caché
        /// - Si no están en caché, realiza la búsqueda en el repositorio
        /// - Almacena los resultados en caché por un tiempo más corto (5 minutos)
        /// </remarks>
        public async Task<IEnumerable<TaskDTO>> SearchTasksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

            var cacheKey = GetListCacheKey("search", searchTerm);
            var cachedResults = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedResults != null)
                return cachedResults;

            var results = _unitOfWork.TaskRepository.Search(searchTerm);
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5)); // Shorter cache for search results
            return results;
        }

        /// <summary>
        /// Actualiza una tarea existente de forma asíncrona.
        /// </summary>
        /// <param name="taskDto">DTO con la información actualizada de la tarea</param>
        /// <exception cref="ArgumentNullException">Si taskDto es null</exception>
        /// <exception cref="ValidationException">Si los datos actualizados no son válidos</exception>
        /// <remarks>
        /// Este método:
        /// - Valida los datos actualizados de la tarea
        /// - Actualiza la tarea en una transacción
        /// - Registra la operación para auditoría
        /// - Invalida la caché para esta tarea
        /// - Mide y registra el rendimiento de la operación
        /// </remarks>
        public async Task UpdateTaskAsync(TaskDTO taskDto)
        {
            if (taskDto == null)
                throw new ArgumentNullException(nameof(taskDto));

            await _taskValidator.ValidateTaskAsync(taskDto);

            var startTime = DateTime.UtcNow;
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.Update(taskDto);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskDto.Id.ToString(),
                    "Updated task",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskDto.Id));
            }, "UpdateTask");

            var duration = DateTime.UtcNow - startTime;
            await _securityLogger.LogPerformanceMetricAsync(
                "UpdateTask",
                duration,
                $"Task ID: {taskDto.Id}"
            );
        }

        /// <summary>
        /// Elimina una tarea por su ID de forma asíncrona.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar</param>
        /// <remarks>
        /// Este método:
        /// - Elimina la tarea en una transacción
        /// - Registra la operación para auditoría
        /// - Invalida la caché para esta tarea
        /// - Mide y registra el rendimiento de la operación
        /// </remarks>
        public async Task DeleteTaskAsync(int id)
        {
            var startTime = DateTime.UtcNow;
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.Delete(id);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    id.ToString(),
                    "Deleted task",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(id));
            }, "DeleteTask");

            var duration = DateTime.UtcNow - startTime;
            await _securityLogger.LogPerformanceMetricAsync(
                "DeleteTask",
                duration,
                $"Task ID: {id}"
            );
        }

        /// <summary>
        /// Añade una categoría a una tarea existente de forma asíncrona.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="category">Categoría a añadir</param>
        /// <exception cref="ArgumentException">Si la categoría es null o vacía</exception>
        /// <remarks>
        /// Este método:
        /// - Valida que la categoría no esté vacía
        /// - Añade la categoría a la tarea en una transacción
        /// - Registra la operación para auditoría
        /// - Invalida la caché para esta tarea
        /// </remarks>
        public async Task AddCategoryToTaskAsync(int taskId, string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty", nameof(category));

            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.AddCategory(taskId, category);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskId.ToString(),
                    $"Added category: {category}",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskId));
            }, "AddCategory");
        }

        /// <summary>
        /// Actualiza el estado de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="status">Nuevo estado de la tarea</param>
        /// <remarks>
        /// Este método:
        /// - Actualiza el estado de la tarea en una transacción
        /// - Registra la operación para auditoría
        /// - Invalida la caché para esta tarea
        /// </remarks>
        public async Task UpdateTaskStatusAsync(int taskId, AIProject.Models.TaskStatus status)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.UpdateStatus(taskId, status);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskId.ToString(),
                    $"Updated status to: {status}",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskId));
            }, "UpdateStatus");
        }

        /// <summary>
        /// Actualiza la prioridad de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="priority">Nueva prioridad de la tarea</param>
        /// <remarks>
        /// Este método:
        /// - Actualiza la prioridad de la tarea en una transacción
        /// - Registra la operación para auditoría
        /// - Invalida la caché para esta tarea
        /// </remarks>
        public async Task UpdateTaskPriorityAsync(int taskId, AIProject.Models.Priority priority)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.UpdatePriority(taskId, priority);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskId.ToString(),
                    $"Updated priority to: {priority}",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskId));
            }, "UpdatePriority");
        }
        
        #endregion
    }
}
