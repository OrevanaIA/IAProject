using System;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Security;
using AIProject.Models;

namespace AIProject.Services
{
    public class TaskValidator : ITaskValidator
    {
        private const int MinDescriptionLength = 10;
        private const int MaxDescriptionLength = 100;

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
