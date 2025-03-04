using System;
using System.Collections.Generic;
using AIProject.Models;

namespace AIProject.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos (DTO) que representa una tarea en el sistema.
    /// Se utiliza para transferir datos entre capas y exponer la información de tareas
    /// sin revelar los detalles de implementación internos.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa el patrón DTO para encapsular los datos de una tarea
    /// y proporcionar métodos de conversión entre el DTO y la entidad de dominio.
    /// </remarks>
    /// <example>
    /// Ejemplo de creación de un nuevo TaskDTO:
    /// <code>
    /// var taskDto = new TaskDTO
    /// {
    ///     Description = "Completar documentación",
    ///     Status = TaskStatus.Pending,
    ///     Priority = Priority.High,
    ///     DueDate = DateTime.Now.AddDays(5),
    ///     Categories = new List<string> { "Documentación", "Desarrollo" }
    /// };
    /// </code>
    /// </example>
    public class TaskDTO
    {
        /// <summary>
        /// Identificador único de la tarea.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Descripción detallada de la tarea.
        /// </summary>
        /// <remarks>
        /// No debe ser null o estar vacía. Se recomienda una descripción clara y concisa.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Estado actual de la tarea.
        /// </summary>
        /// <remarks>
        /// Puede ser Pending, InProgress, Completed o Cancelled.
        /// </remarks>
        public AIProject.Models.TaskStatus Status { get; set; }

        /// <summary>
        /// Nivel de prioridad de la tarea.
        /// </summary>
        /// <remarks>
        /// Puede ser Low, Medium o High.
        /// </remarks>
        public Priority Priority { get; set; }

        /// <summary>
        /// Fecha y hora de creación de la tarea.
        /// </summary>
        /// <remarks>
        /// Se establece automáticamente al crear la tarea.
        /// </remarks>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Fecha y hora de la última modificación de la tarea.
        /// </summary>
        /// <remarks>
        /// Se actualiza automáticamente cada vez que se modifica la tarea.
        /// </remarks>
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Fecha límite para completar la tarea.
        /// </summary>
        /// <remarks>
        /// Es nullable para tareas sin fecha límite específica.
        /// Debe ser una fecha futura cuando se establece.
        /// </remarks>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Lista de categorías asociadas a la tarea.
        /// </summary>
        /// <remarks>
        /// Se inicializa como una lista vacía por defecto.
        /// Las categorías ayudan a organizar y filtrar tareas.
        /// </remarks>
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// Convierte una entidad TaskItem en un objeto TaskDTO.
        /// </summary>
        /// <param name="entity">Entidad TaskItem a convertir</param>
        /// <returns>Un nuevo objeto TaskDTO con los datos de la entidad</returns>
        /// <exception cref="ArgumentNullException">Si entity es null</exception>
        /// <example>
        /// <code>
        /// TaskItem taskItem = // obtener de alguna fuente
        /// TaskDTO dto = TaskDTO.FromEntity(taskItem);
        /// </code>
        /// </example>
        public static TaskDTO FromEntity(TaskItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new TaskDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                Status = entity.Status,
                Priority = entity.Priority,
                CreationDate = entity.CreationDate,
                LastModifiedDate = entity.LastModifiedDate,
                DueDate = entity.DueDate,
                Categories = new List<string>(entity.Categories)
            };
        }

        /// <summary>
        /// Convierte este objeto TaskDTO en una entidad TaskItem.
        /// </summary>
        /// <returns>Una nueva entidad TaskItem con los datos del DTO</returns>
        /// <exception cref="InvalidOperationException">Si faltan datos requeridos como Description</exception>
        /// <remarks>
        /// Este método realiza una conversión profunda, copiando todas las propiedades
        /// incluyendo la lista de categorías.
        /// </remarks>
        /// <example>
        /// <code>
        /// TaskDTO dto = // obtener de alguna fuente
        /// TaskItem entity = dto.ToEntity();
        /// </code>
        /// </example>
        public TaskItem ToEntity()
        {
            if (string.IsNullOrWhiteSpace(Description))
                throw new InvalidOperationException("La descripción es requerida para crear una entidad TaskItem");

            var task = new TaskItem(Id, Description, Status)
            {
                Priority = Priority,
                DueDate = DueDate
            };
            task.Categories.AddRange(Categories ?? new List<string>());
            return task;
        }
    }
}
