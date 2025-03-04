using System;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Security;
using AIProject.Models;

namespace AIProject.Services
{
    /// <summary>
    /// Implementación del validador de tareas que verifica la integridad y validez de los datos de las tareas.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa la interfaz ITaskValidator y proporciona:
    /// - Validación completa de tareas y sus componentes
    /// - Sanitización de datos para prevenir inyecciones y XSS
    /// - Verificación de reglas de negocio específicas
    /// - Operaciones asíncronas para validaciones complejas
    /// </remarks>
    /// <example>
    /// Ejemplo de uso básico:
    /// <code>
    /// var validator = new TaskValidator();
    /// var task = new TaskDTO { Description = "Nueva tarea", Status = TaskStatus.Pending };
    /// await validator.ValidateTaskAsync(task);
    /// </code>
    /// </example>
    public class TaskValidator : ITaskValidator
    {
        /// <summary>
        /// Longitud mínima permitida para la descripción de una tarea.
        /// </summary>
        private const int MinDescriptionLength = 10;

        /// <summary>
        /// Longitud máxima permitida para la descripción de una tarea.
        /// </summary>
        private const int MaxDescriptionLength = 100;

        /// <summary>
        /// Valida una tarea completa de forma asíncrona, verificando todos sus campos y reglas de negocio.
        /// </summary>
        /// <param name="task">DTO de la tarea a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si task es null</exception>
        /// <exception cref="ArgumentException">Si algún campo de la tarea no cumple con las reglas de validación</exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la tarea no sea null
        /// - Sanitiza y valida la descripción
        /// - Valida el estado y la prioridad
        /// - Sanitiza y valida las categorías
        /// - Verifica que la fecha límite no esté en el pasado
        /// - Actualiza la fecha de última modificación
        /// </remarks>
        public async Task ValidateTaskAsync(TaskDTO task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            // Sanitizar y validar la descripción
            task.Description = InputSanitizer.SanitizeTaskDescription(task.Description);
            await ValidateDescriptionAsync(task.Description);

            // Validar estado y prioridad
            await ValidateStatusAsync(task.Status);
            await ValidatePriorityAsync(task.Priority);

            // Sanitizar y validar categorías
            if (task.Categories != null)
            {
                for (int i = 0; i < task.Categories.Count; i++)
                {
                    task.Categories[i] = InputSanitizer.SanitizeCategory(task.Categories[i]);
                    await ValidateCategoryAsync(task.Categories[i]);
                }
            }

            // Validar fecha límite
            if (task.DueDate.HasValue && task.DueDate.Value < DateTime.Now.Date)
            {
                throw new ArgumentException("Due date cannot be in the past");
            }

            // Actualizar fecha de última modificación
            task.LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Valida la descripción de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="description">Descripción a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentException">
        /// Si la descripción es null, vacía, solo espacios en blanco,
        /// o no cumple con los límites de longitud establecidos
        /// </exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la descripción no sea null o vacía
        /// - Sanitiza la descripción para prevenir XSS e inyecciones
        /// - Verifica que la longitud esté entre los límites establecidos (10-100 caracteres)
        /// </remarks>
        public async Task ValidateDescriptionAsync(string description)
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    throw new ArgumentException("Description cannot be empty", nameof(description));
                }

                var sanitizedDescription = InputSanitizer.SanitizeTaskDescription(description);

                if (sanitizedDescription.Length < MinDescriptionLength || sanitizedDescription.Length > MaxDescriptionLength)
                {
                    throw new ArgumentException(
                        $"Description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters",
                        nameof(description));
                }
            });
        }

        /// <summary>
        /// Valida el estado de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="status">Estado a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentException">Si el estado no es un valor válido del enum TaskStatus</exception>
        /// <remarks>
        /// Este método verifica que el estado sea uno de los valores definidos en el enum TaskStatus:
        /// - Pending
        /// - InProgress
        /// - Completed
        /// - Cancelled
        /// </remarks>
        public async Task ValidateStatusAsync(AIProject.Models.TaskStatus status)
        {
            await Task.Run(() =>
            {
                if (!Enum.IsDefined(typeof(AIProject.Models.TaskStatus), status))
                {
                    throw new ArgumentException("Invalid task status", nameof(status));
                }
            });
        }

        /// <summary>
        /// Valida la prioridad de una tarea de forma asíncrona.
        /// </summary>
        /// <param name="priority">Prioridad a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentException">Si la prioridad no es un valor válido del enum Priority</exception>
        /// <remarks>
        /// Este método verifica que la prioridad sea uno de los valores definidos en el enum Priority:
        /// - Alta
        /// - Media
        /// - Baja
        /// </remarks>
        public async Task ValidatePriorityAsync(Priority priority)
        {
            await Task.Run(() =>
            {
                if (!Enum.IsDefined(typeof(Priority), priority))
                {
                    throw new ArgumentException("Invalid priority level", nameof(priority));
                }
            });
        }

        /// <summary>
        /// Valida una categoría de tarea de forma asíncrona.
        /// </summary>
        /// <param name="category">Categoría a validar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentException">
        /// Si la categoría es null, vacía, solo espacios en blanco,
        /// o no cumple con el formato requerido
        /// </exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la categoría no sea null o vacía
        /// - Sanitiza la categoría para prevenir XSS e inyecciones
        /// - Verifica que la categoría no esté vacía después de la sanitización
        /// - Verifica que la longitud no exceda 50 caracteres
        /// - Verifica que la categoría solo contenga caracteres válidos (letras, números, espacios y guiones)
        /// </remarks>
        public async Task ValidateCategoryAsync(string category)
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    throw new ArgumentException("Category cannot be empty", nameof(category));
                }

                var sanitizedCategory = InputSanitizer.SanitizeCategory(category);

                if (string.IsNullOrWhiteSpace(sanitizedCategory))
                {
                    throw new ArgumentException("Category contains no valid characters after sanitization", nameof(category));
                }

                if (sanitizedCategory.Length > 50)
                {
                    throw new ArgumentException("Category name cannot exceed 50 characters", nameof(category));
                }

                // Verificar que la categoría solo contenga caracteres válidos después de la sanitización
                if (!System.Text.RegularExpressions.Regex.IsMatch(sanitizedCategory, @"^[a-zA-Z0-9\s\-]+$"))
                {
                    throw new ArgumentException("Category can only contain letters, numbers, spaces, and hyphens", nameof(category));
                }
            });
        }
    }
}
