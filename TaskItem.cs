using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace AIProject.Models
{
/// <summary>
/// Enumeración que define los niveles de prioridad disponibles para las tareas.
/// </summary>
/// <remarks>
/// La prioridad ayuda a organizar y priorizar las tareas según su importancia.
/// </remarks>
public enum Priority
{
    /// <summary>Prioridad alta para tareas urgentes o críticas</summary>
    Alta,
    /// <summary>Prioridad media para tareas importantes pero no urgentes</summary>
    Media,
    /// <summary>Prioridad baja para tareas que pueden esperar</summary>
    Baja
}

/// <summary>
/// Enumeración que define los posibles estados de una tarea.
/// </summary>
/// <remarks>
/// El estado representa el ciclo de vida de una tarea desde su creación hasta su finalización.
/// </remarks>
public enum TaskStatus
{
    /// <summary>Tarea pendiente de iniciar</summary>
    Pending,
    /// <summary>Tarea en proceso de realización</summary>
    InProgress,
    /// <summary>Tarea completada exitosamente</summary>
    Completed,
    /// <summary>Tarea cancelada o abandonada</summary>
    Cancelled
}

/// <summary>
/// Clase que representa una tarea en el sistema.
/// Implementa la entidad principal del dominio con todas sus propiedades y comportamientos.
/// </summary>
/// <remarks>
/// Esta clase incluye:
/// - Validaciones mediante Data Annotations
/// - Valores por defecto para propiedades esenciales
/// - Gestión automática de fechas de creación y modificación
/// - Soporte para categorización mediante etiquetas
/// </remarks>
/// <example>
/// Ejemplo de creación de una nueva tarea:
/// <code>
/// var task = new TaskItem(1, "Implementar feature X", TaskStatus.Pending)
/// {
///     Priority = Priority.Alta,
///     DueDate = DateTime.Now.AddDays(7),
///     Categories = new List<string> { "Desarrollo", "Frontend" }
/// };
/// </code>
/// </example>
public class TaskItem
{
    
    /// <summary>
    /// Identificador único de la tarea.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Descripción detallada de la tarea.
    /// </summary>
    /// <remarks>
    /// La descripción debe cumplir con las siguientes reglas:
    /// - Es obligatoria
    /// - Debe tener entre 10 y 100 caracteres
    /// - No puede ser null o estar vacía
    /// </remarks>
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 100 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual de la tarea.
    /// </summary>
    /// <remarks>
    /// Por defecto, una nueva tarea se crea en estado Pending.
    /// </remarks>
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>
    /// Fecha y hora de creación de la tarea.
    /// </summary>
    /// <remarks>
    /// Se establece automáticamente al crear la tarea y no debe modificarse.
    /// </remarks>
    public DateTime CreationDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Fecha límite opcional para completar la tarea.
    /// </summary>
    /// <remarks>
    /// - Puede ser null si la tarea no tiene fecha límite
    /// - Debe ser una fecha futura cuando se establece
    /// </remarks>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Nivel de prioridad de la tarea.
    /// </summary>
    /// <remarks>
    /// Por defecto, una nueva tarea se crea con prioridad Media.
    /// </remarks>
    public Priority Priority { get; set; } = Priority.Media;
    
    /// <summary>
    /// Fecha y hora de la última modificación de la tarea.
    /// </summary>
    /// <remarks>
    /// Se actualiza automáticamente cada vez que se modifica la tarea.
    /// </remarks>
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Lista de categorías o etiquetas asociadas a la tarea.
    /// </summary>
    /// <remarks>
    /// - Se inicializa como una lista vacía
    /// - Permite organizar y filtrar tareas por categorías
    /// - Una tarea puede tener múltiples categorías
    /// </remarks>
    public List<string> Categories { get; set; } = new List<string>();

    /// <summary>
    /// Constructor que inicializa una nueva tarea con sus propiedades básicas.
    /// </summary>
    /// <param name="id">Identificador único de la tarea</param>
    /// <param name="description">Descripción detallada de la tarea</param>
    /// <param name="status">Estado inicial de la tarea</param>
    /// <exception cref="ArgumentException">Si el ID es menor o igual a 0</exception>
    /// <exception cref="ArgumentNullException">Si la descripción es null</exception>
    /// <remarks>
    /// Este constructor:
    /// - Establece las propiedades básicas requeridas
    /// - Inicializa las fechas de creación y modificación
    /// - Establece valores por defecto para otras propiedades
    /// </remarks>
    public TaskItem(int id, string description, TaskStatus status)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));

        Id = id;
        Description = description;
        Status = status;
        CreationDate = DateTime.Now;
        LastModifiedDate = DateTime.Now;
    }

    /// <summary>
    /// Genera una representación en cadena de la tarea con todos sus detalles.
    /// </summary>
    /// <returns>String con los detalles formateados de la tarea</returns>
    /// <remarks>
    /// Incluye:
    /// - Todas las propiedades básicas
    /// - Fecha límite (si está establecida)
    /// - Categorías (si existen)
    /// </remarks>
    public override string ToString()
    {
        var dueDate = DueDate.HasValue ? $", Due: {DueDate.Value}" : "";
        var categories = Categories.Any() ? $", Categories: {string.Join(", ", Categories)}" : "";
        return $"Task ID: {Id}, Description: {Description}, Status: {Status}, Priority: {Priority}, Created: {CreationDate}, Last Modified: {LastModifiedDate}{dueDate}{categories}";
    }
}
}
