using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Models;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que define las operaciones de persistencia para las tareas.
    /// Implementa el patrón Repository para abstraer y encapsular el acceso a datos.
    /// </summary>
    /// <remarks>
    /// Esta interfaz proporciona una capa de abstracción sobre el almacenamiento de datos,
    /// permitiendo cambiar la implementación subyacente sin afectar al resto del sistema.
    /// Soporta operaciones CRUD básicas, consultas especializadas y optimizaciones de rendimiento.
    /// </remarks>
    /// <example>
    /// Ejemplo de uso básico:
    /// <code>
    /// ITaskRepository repository = new TaskRepository();
    /// 
    /// // Crear una nueva tarea
    /// var newTask = new TaskDTO { Description = "Nueva tarea" };
    /// repository.Add(newTask);
    /// 
    /// // Obtener tareas con paginación
    /// var params = new PaginationParams(pageNumber: 1, pageSize: 10);
    /// var tasks = await repository.GetAllPagedAsync(params);
    /// </code>
    /// </example>
    public interface ITaskRepository
    {
        /// <summary>
        /// Obtiene una tarea por su identificador único de forma sincrónica.
        /// </summary>
        /// <param name="id">ID de la tarea a obtener</param>
        /// <returns>DTO de la tarea encontrada</returns>
        TaskDTO GetById(int id);

        /// <summary>
        /// Obtiene una tarea por su identificador único, utilizando caché.
        /// </summary>
        /// <param name="id">ID de la tarea a obtener</param>
        /// <returns>DTO de la tarea encontrada</returns>
        /// <exception cref="ArgumentException">Si el ID es menor o igual a 0</exception>
        /// <exception cref="NotFoundException">Si no se encuentra la tarea con el ID especificado</exception>
        /// <remarks>
        /// Implementa caché para mejorar el rendimiento en tareas frecuentemente accedidas.
        /// </remarks>
        Task<TaskDTO> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las tareas de forma sincrónica.
        /// </summary>
        /// <returns>Colección de DTOs de tareas</returns>
        IEnumerable<TaskDTO> GetAll();

        /// <summary>
        /// Obtiene todas las tareas de forma paginada.
        /// </summary>
        /// <param name="paginationParams">Parámetros de paginación</param>
        /// <returns>Colección paginada de DTOs de tareas</returns>
        /// <remarks>
        /// Implementa paginación para optimizar el rendimiento con grandes conjuntos de datos.
        /// Las tareas se devuelven ordenadas por fecha de creación descendente.
        /// </remarks>
        Task<IEnumerable<TaskDTO>> GetAllPagedAsync(PaginationParams paginationParams);

        /// <summary>
        /// Obtiene las tareas por estado de forma sincrónica.
        /// </summary>
        /// <param name="status">Estado de las tareas a buscar</param>
        /// <returns>Colección de DTOs de tareas</returns>
        IEnumerable<TaskDTO> GetByStatus(AIProject.Models.TaskStatus status);

        /// <summary>
        /// Obtiene las tareas por estado de forma paginada.
        /// </summary>
        /// <param name="status">Estado de las tareas a buscar</param>
        /// <param name="paginationParams">Parámetros de paginación</param>
        /// <returns>Colección paginada de DTOs de tareas</returns>
        Task<IEnumerable<TaskDTO>> GetByStatusPagedAsync(AIProject.Models.TaskStatus status, PaginationParams paginationParams);

        /// <summary>
        /// Realiza una búsqueda de tareas de forma sincrónica.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Colección de DTOs de tareas que coinciden con la búsqueda</returns>
        IEnumerable<TaskDTO> Search(string searchTerm);

        /// <summary>
        /// Realiza una búsqueda optimizada de tareas.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <param name="paginationParams">Parámetros de paginación</param>
        /// <returns>Colección paginada de DTOs de tareas que coinciden con la búsqueda</returns>
        /// <remarks>
        /// Implementa búsqueda optimizada utilizando índices y caché.
        /// </remarks>
        Task<IEnumerable<TaskDTO>> SearchOptimizedAsync(string searchTerm, PaginationParams paginationParams);

        /// <summary>
        /// Añade una nueva tarea al repositorio de forma sincrónica.
        /// </summary>
        /// <param name="task">DTO de la tarea a añadir</param>
        void Add(TaskDTO task);

        /// <summary>
        /// Añade una nueva tarea al repositorio de forma asíncrona.
        /// </summary>
        /// <param name="task">DTO de la tarea a añadir</param>
        /// <returns>ID de la tarea creada</returns>
        Task<int> AddAsync(TaskDTO task);

        /// <summary>
        /// Actualiza una tarea existente de forma sincrónica.
        /// </summary>
        /// <param name="task">DTO con los datos actualizados</param>
        void Update(TaskDTO task);

        /// <summary>
        /// Actualiza una tarea existente de forma asíncrona.
        /// </summary>
        /// <param name="task">DTO con los datos actualizados</param>
        Task UpdateAsync(TaskDTO task);

        /// <summary>
        /// Elimina una tarea de forma sincrónica.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar</param>
        void Delete(int id);

        /// <summary>
        /// Elimina una tarea y actualiza la caché.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Obtiene las tareas por prioridad de forma sincrónica.
        /// </summary>
        /// <param name="priority">Prioridad de las tareas</param>
        /// <returns>Colección de DTOs de tareas</returns>
        IEnumerable<TaskDTO> GetByPriority(Priority priority);

        /// <summary>
        /// Obtiene las tareas por prioridad de forma paginada.
        /// </summary>
        /// <param name="priority">Prioridad de las tareas</param>
        /// <param name="paginationParams">Parámetros de paginación</param>
        /// <returns>Colección paginada de DTOs de tareas</returns>
        Task<IEnumerable<TaskDTO>> GetByPriorityPagedAsync(Priority priority, PaginationParams paginationParams);

        /// <summary>
        /// Añade una categoría a una tarea de forma sincrónica.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="category">Categoría a añadir</param>
        void AddCategory(int taskId, string category);

        /// <summary>
        /// Añade una categoría a una tarea y actualiza la caché.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="category">Categoría a añadir</param>
        Task AddCategoryAsync(int taskId, string category);

        /// <summary>
        /// Actualiza el estado de una tarea de forma sincrónica.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="status">Nuevo estado</param>
        void UpdateStatus(int taskId, AIProject.Models.TaskStatus status);

        /// <summary>
        /// Actualiza el estado de una tarea y la caché.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="status">Nuevo estado</param>
        Task UpdateStatusAsync(int taskId, AIProject.Models.TaskStatus status);

        /// <summary>
        /// Actualiza la prioridad de una tarea de forma sincrónica.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="priority">Nueva prioridad</param>
        void UpdatePriority(int taskId, Priority priority);

        /// <summary>
        /// Actualiza la prioridad de una tarea y la caché.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <param name="priority">Nueva prioridad</param>
        Task UpdatePriorityAsync(int taskId, Priority priority);

        /// <summary>
        /// Obtiene el total de tareas que coinciden con los criterios de búsqueda.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda opcional</param>
        /// <returns>Número total de tareas</returns>
        /// <remarks>
        /// Utilizado para cálculos de paginación y estadísticas.
        /// </remarks>
        Task<int> GetTotalCountAsync(string? searchTerm = null);

        /// <summary>
        /// Invalida la caché para una tarea específica.
        /// </summary>
        /// <param name="taskId">ID de la tarea</param>
        /// <remarks>
        /// Se utiliza cuando se realizan cambios que requieren actualización de caché.
        /// </remarks>
        Task InvalidateCacheAsync(int taskId);

        /// <summary>
        /// Guarda los cambios realizados en el repositorio.
        /// </summary>
        void SaveChanges();
    }
}
