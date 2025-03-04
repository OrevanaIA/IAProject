using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Services;
using AIProject.Models;

namespace AIProject.Tests
{
    [TestClass]
    public class TaskValidatorTests
    {
        private TaskValidator _validator;
        private Mock<ISecurityLogger> _mockSecurityLogger;

        [TestInitialize]
        public void Initialize()
        {
            _mockSecurityLogger = new Mock<ISecurityLogger>();
            _validator = new TaskValidator();
        }

        #region Validación de Tareas

        [TestMethod]
        public async Task ValidateTaskAsync_ValidTask_Succeeds()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Esta es una descripción válida",
                Status = AIProject.Models.TaskStatus.Pending,
                Priority = Priority.Media,
                CreationDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            // Act & Assert
            // No exception should be thrown
            await _validator.ValidateTaskAsync(taskDto);
            
            // Verify logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_NullTask_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
            {
                await _validator.ValidateTaskAsync(null);
            });
        }

        [TestMethod]
        public async Task ValidateTaskAsync_EmptyDescription_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "",
                Status = AIProject.Models.TaskStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("descripción"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_ShortDescription_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Corta", // Less than 10 characters
                Status = AIProject.Models.TaskStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("10 caracteres"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_LongDescription_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = new string('a', 101), // More than 100 characters
                Status = AIProject.Models.TaskStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("100 caracteres"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_InvalidStatus_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Esta es una descripción válida",
                Status = (AIProject.Models.TaskStatus)999 // Invalid status
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("estado"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_InvalidPriority_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Esta es una descripción válida",
                Status = AIProject.Models.TaskStatus.Pending,
                Priority = (Priority)999 // Invalid priority
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("prioridad"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_PastDueDate_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Esta es una descripción válida",
                Status = AIProject.Models.TaskStatus.Pending,
                DueDate = DateTime.Now.AddDays(-1) // Past due date
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("fecha límite"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_FutureDueDate_Succeeds()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Esta es una descripción válida",
                Status = AIProject.Models.TaskStatus.Pending,
                DueDate = DateTime.Now.AddDays(1) // Future due date
            };

            // Act & Assert
            // No exception should be thrown
            await _validator.ValidateTaskAsync(taskDto);
            
            // Verify logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_NullDueDate_Succeeds()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Esta es una descripción válida",
                Status = AIProject.Models.TaskStatus.Pending,
                DueDate = null // No due date
            };

            // Act & Assert
            // No exception should be thrown
            await _validator.ValidateTaskAsync(taskDto);
            
            // Verify logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_InvalidId_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = -1, // Invalid ID
                Description = "Esta es una descripción válida",
                Status = AIProject.Models.TaskStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("ID"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task ValidateTaskAsync_ZeroId_ThrowsException()
        {
            // Arrange
            var taskDto = new TaskDTO
            {
                Id = 0, // Invalid ID
                Description = "Esta es una descripción válida",
                Status = AIProject.Models.TaskStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
            {
                await _validator.ValidateTaskAsync(taskDto);
            });

            Assert.IsTrue(exception.Message.Contains("ID"));
            
            // Verify error logging
            _mockSecurityLogger.Verify(l => l.LogOperationAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<string>()
            ), Times.Once);
        }

        #endregion
    }
}
