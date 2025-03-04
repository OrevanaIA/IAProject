using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Models;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que define los servicios principales para la gestión de tareas en el sistema.
    /// Proporciona operaciones CRUD y funcionalidades adicionales para manipular tareas de forma asíncrona.
    /// </summary>
    /// <remarks>
    /// Esta interfaz implementa el patrón Service Layer, actuando como intermediario
    /// entre la capa de presentación y la capa de acceso a datos.
    /// Todas las operaciones son asíncronas para mejorar el rendimiento y la escalabilidad.
    /// </remarks>
    /// <example>
    /// Ejemplo de uso básico:
    /// <code>
    /// ITaskService taskService = new TaskService();
    /// var newTask = new TaskDTO { Title = "Nueva tarea", Description = "Descripción" };
    /// var createdTask = await taskService.CreateTaskAsync(newTask);
    /// </code>
    /// </example>
    public interface ITaskService
    {
        // Synchronous methods used by Program.cs
        
        /// <summary>
        /// Crea una nueva tarea en el sistema.
        /// </summary>
        /// <param name="taskDto">DTO con la información de la tarea a crear</param>
        /// <returns>DTO de la tarea creada, incluyendo el ID asignado</returns>
        TaskDTO CreateTask(TaskDTO taskDto);
        
        /// <summary>
        /// Obtiene todas las tareas existentes en el sistema.
        /// </summary>
        /// <returns>Colección de DTOs de todas las tareas</returns>
        IEnumerable<TaskDTO> GetAllTasks();
        
        /// <summary>
        /// Actualiza el estado de una tarea.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="status">Nuevo estado de la tarea</param>
        void UpdateTaskStatus(int taskId, AIProject.Models.TaskStatus status);
        
        /// <summary>
        /// Elimina una tarea por su ID.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar</param>
        void DeleteTask(int id);
        
        /// <summary>
        /// Añade una categoría a una tarea existente.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="category">Categoría a añadir</param>
        void AddCategoryToTask(int taskId, string category);
        
        /// <summary>
        /// Actualiza la prioridad de una tarea.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="priority">Nueva prioridad de la tarea</param>
        void UpdateTaskPriority(int taskId, AIProject.Models.Priority priority);
        
        /// <summary>
        /// Busca tareas que contengan el término especificado en su título o descripción.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con la búsqueda</returns>
        IEnumerable<TaskDTO> SearchTasks(string searchTerm);
        
        // Asynchronous methods
        /// <summary>
        /// Crea una nueva tarea en el sistema de forma asíncrona.
        /// </summary>
        /// <param name="taskDto">DTO con la información de la tarea a crear</param>
        /// <returns>DTO de la tarea creada, incluyendo el ID asignado</returns>
        /// <exception cref="ArgumentNullException">Si taskDto es null</exception>
        /// <exception cref="ValidationException">Si los datos de la tarea no son válidos</exception>
        /// <example>
        /// <code>
        /// var newTask = new TaskDTO 
        /// { 
        ///     Title = "Completar informe",
        ///     Description = "Realizar informe mensual de ventas",
        ///     DueDate = DateTime.Now.AddDays(7)
        /// };
        /// var createdTask = await taskService.CreateTaskAsync(newTask);
        /// </code>
        /// </example>
        Task<TaskDTO> CreateTaskAsync(TaskDTO taskDto);

        /// <summary>
        /// Obtiene una tarea específica por su ID de forma asíncrona.
        /// </summary>
        /// <param name="id">ID de la tarea a obtener</param>
        /// <returns>DTO de la tarea encontrada</returns>
        /// <exception cref="ArgumentException">Si el ID es menor o igual a 0</exception>
        /// <exception cref="NotFoundException">Si no se encuentra la tarea con el ID especificado</exception>
        Task<TaskDTO> GetTaskAsync(int id);

        /// <summary>
        /// Obtiene todas las tareas existentes en el sistema de forma asíncrona.
        /// </summary>
        /// <returns>Colección de DTOs de todas las tareas</returns>
        Task<IEnumerable<TaskDTO>> GetAllTasksAsync();

        /// <summary>
        /// Obtiene las tareas filtradas por su estado de forma asíncrona.
        /// </summary>
        /// <param name="status">Estado de las tareas a buscar</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con el estado</returns>
        Task<IEnumerable<TaskDTO>> GetTasksByStatusAsync(AIProject.Models.TaskStatus status);

        /// <summary>
        /// Obtiene las tareas filtradas por su prioridad de forma asíncrona.
        /// </summary>
        /// <param name="priority">Prioridad de las tareas a buscar</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con la prioridad</returns>
        Task<IEnumerable<TaskDTO>> GetTasksByPriorityAsync(AIProject.Models.Priority priority);

        /// <summary>
        /// Busca tareas que contengan el término especificado en su título o descripción de forma asíncrona.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Colección de DTOs de las tareas que coinciden con la búsqueda</returns>
        /// <exception cref="ArgumentNullException">Si searchTerm es null</exception>
        Task<IEnumerable<TaskDTO>> SearchTasksAsync(string searchTerm);

        /// <summary>
        /// Actualiza una tarea existente de forma asíncrona.
        /// </summary>
        /// <param name="taskDto">DTO con la información actualizada de la tarea</param>
        /// <exception cref="ArgumentNullException">Si taskDto es null</exception>
        /// <exception cref="NotFoundException">Si no se encuentra la tarea a actualizar</exception>
        /// <exception cref="ValidationException">Si los datos actualizados no son válidos</exception>
        Task UpdateTaskAsync(TaskDTO taskDto);

        /// <summary>
        /// Elimina una tarea por su ID de forma asíncrona.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar</param>
        /// <exception cref="ArgumentException">Si el ID es menor o igual a 0</exception>
        /// <exception cref="NotFoundException">Si no se encuentra la tarea a eliminar</exception>
        Task DeleteTaskAsync(int id);

        /// <summary>
        /// Añade una categoría a una tarea existente de forma asíncrona.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="category">Categoría a añadir</param>
        /// <exception cref="ArgumentException">Si el ID es menor o igual a 0 o la categoría es null o vacía</exception>
        /// <exception cref="NotFoundException">Si no se encuentra la tarea</exception>
        Task AddCategoryToTaskAsync(int taskId, string category);

        /// <summary>
        /// Actualiza el estado de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="status">Nuevo estado de la tarea</param>
        /// <exception cref="ArgumentException">Si el ID es menor o igual a 0</exception>
        /// <exception cref="NotFoundException">Si no se encuentra la tarea</exception>
        Task UpdateTaskStatusAsync(int taskId, AIProject.Models.TaskStatus status);

        /// <summary>
        /// Actualiza la prioridad de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="priority">Nueva prioridad de la tarea</param>
        /// <exception cref="ArgumentException">Si el ID es menor o igual a 0</exception>
        /// <exception cref="NotFoundException">Si no se encuentra la tarea</exception>
        Task UpdateTaskPriorityAsync(int taskId, AIProject.Models.Priority priority);
    }
}
