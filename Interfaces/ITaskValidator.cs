using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Models;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que define las operaciones de validación asíncrona para las tareas y sus componentes.
    /// Implementa el patrón Validator para centralizar y estandarizar las reglas de validación.
    /// </summary>
    /// <remarks>
    /// Esta interfaz es fundamental para mantener la integridad de los datos y
    /// asegurar que todas las tareas cumplan con las reglas de negocio establecidas.
    /// Las operaciones son asíncronas para permitir validaciones que requieran
    /// acceso a recursos externos o procesamiento intensivo.
    /// </remarks>
    /// <example>
    /// Ejemplo de uso básico:
    /// <code>
    /// ITaskValidator validator = new TaskValidator();
    /// var task = new TaskDTO { Description = "Nueva tarea" };
    /// await validator.ValidateTaskAsync(task); // Valida toda la tarea
    /// await validator.ValidateDescriptionAsync("Descripción específica"); // Valida solo la descripción
    /// </code>
    /// </example>
    public interface ITaskValidator
    {
        /// <summary>
        /// Valida una tarea completa de forma asíncrona, verificando todos sus campos y reglas de negocio.
        /// </summary>
        /// <param name="task">DTO de la tarea a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si task es null</exception>
        /// <exception cref="ValidationException">Si algún campo de la tarea no cumple con las reglas de validación</exception>
        /// <remarks>
        /// Realiza una validación completa de todos los campos de la tarea:
        /// - Descripción no vacía y dentro del límite de caracteres
        /// - Estado válido
        /// - Prioridad válida
        /// - Fecha límite válida (si está establecida)
        /// - Categorías válidas
        /// Incluye sanitización de datos para prevenir inyecciones y XSS.
        /// </remarks>
        Task ValidateTaskAsync(TaskDTO task);

        /// <summary>
        /// Valida la descripción de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="description">Descripción a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ValidationException">
        /// Si la descripción es null, vacía, solo espacios en blanco,
        /// o excede el límite de caracteres permitido
        /// </exception>
        /// <remarks>
        /// La descripción debe:
        /// - No ser null o vacía
        /// - Tener al menos 3 caracteres
        /// - No exceder 500 caracteres
        /// - No contener solo espacios en blanco
        /// - Estar sanitizada contra XSS e inyecciones
        /// </remarks>
        Task ValidateDescriptionAsync(string description);

        /// <summary>
        /// Valida el estado de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="status">Estado a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ValidationException">Si el estado no es un valor válido del enum TaskStatus</exception>
        /// <remarks>
        /// Verifica que el estado sea uno de los valores definidos en el enum TaskStatus:
        /// - Pending
        /// - InProgress
        /// - Completed
        /// - Cancelled
        /// </remarks>
        Task ValidateStatusAsync(AIProject.Models.TaskStatus status);

        /// <summary>
        /// Valida la prioridad de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="priority">Prioridad a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ValidationException">Si la prioridad no es un valor válido del enum Priority</exception>
        /// <remarks>
        /// Verifica que la prioridad sea uno de los valores definidos en el enum Priority:
        /// - Alta
        /// - Media
        /// - Baja
        /// </remarks>
        Task ValidatePriorityAsync(Priority priority);

        /// <summary>
        /// Valida una categoría de tarea de forma asíncrona.
        /// </summary>
        /// <param name="category">Categoría a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ValidationException">
        /// Si la categoría es null, vacía, solo espacios en blanco,
        /// o no cumple con el formato requerido
        /// </exception>
        /// <remarks>
        /// La categoría debe:
        /// - No ser null o vacía
        /// - Tener entre 2 y 50 caracteres
        /// - Contener solo letras, números, espacios y guiones
        /// - No comenzar ni terminar con espacios
        /// - Estar sanitizada contra XSS e inyecciones
        /// </remarks>
        Task ValidateCategoryAsync(string category);
    }
}
