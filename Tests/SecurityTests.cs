using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using AIProject.Security;
using AIProject.Services;

namespace AIProject.Tests
{
    [TestClass]
    public class SecurityTests
    {
        #region Sanitización de Entrada

        [TestMethod]
        public void SanitizeTaskDescription_ValidInput_ReturnsSanitizedInput()
        {
            // Arrange
            string input = "Descripción normal de una tarea";

            // Act
            string result = InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void SanitizeTaskDescription_NullInput_ReturnsEmptyString()
        {
            // Act
            string result = InputSanitizer.SanitizeTaskDescription(null);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void SanitizeTaskDescription_ScriptTags_RemovesTags()
        {
            // Arrange
            string input = "Tarea con <script>alert('XSS')</script> código malicioso";
            string expected = "Tarea con  código malicioso";

            // Act
            string result = InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SanitizeTaskDescription_HtmlTags_RemovesTags()
        {
            // Arrange
            string input = "Tarea con <b>formato</b> y <i>estilos</i>";
            string expected = "Tarea con formato y estilos";

            // Act
            string result = InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SanitizeTaskDescription_SqlInjection_SanitizesInput()
        {
            // Arrange
            string input = "Tarea'; DROP TABLE Tasks; --";
            string expected = "Tarea DROP TABLE Tasks ";

            // Act
            string result = InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SanitizeTaskDescription_SpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            string input = "Tarea con caracteres especiales: !@#$%^&*()_+";

            // Act
            string result = InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void SanitizeTaskDescription_ExcessiveWhitespace_NormalizesWhitespace()
        {
            // Arrange
            string input = "Tarea   con    muchos     espacios";
            string expected = "Tarea con muchos espacios";

            // Act
            string result = InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SanitizeTaskDescription_LeadingTrailingWhitespace_TrimsWhitespace()
        {
            // Arrange
            string input = "   Tarea con espacios al inicio y final   ";
            string expected = "Tarea con espacios al inicio y final";

            // Act
            string result = InputSanitizer.SanitizeTaskDescription(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region Logging de Seguridad

        [TestMethod]
        public async Task ConsoleSecurityLogger_LogOperationAsync_LogsOperation()
        {
            // Arrange
            var logger = new ConsoleSecurityLogger();
            string operation = "TestOperation";
            string details = "Test operation details";
            string user = "testuser";

            // Act & Assert
            // Since we can't easily test console output, we just ensure no exception is thrown
            await logger.LogOperationAsync(operation, details, user);
        }

        [TestMethod]
        public async Task ConsoleSecurityLogger_LogDataChangeAsync_LogsDataChange()
        {
            // Arrange
            var logger = new ConsoleSecurityLogger();
            string entityType = "Task";
            string entityId = "1";
            string changeDetails = "Updated task status";
            string user = "testuser";

            // Act & Assert
            // Since we can't easily test console output, we just ensure no exception is thrown
            await logger.LogDataChangeAsync(entityType, entityId, changeDetails, user);
        }

        [TestMethod]
        public async Task ConsoleSecurityLogger_LogPerformanceMetricAsync_LogsPerformanceMetric()
        {
            // Arrange
            var logger = new ConsoleSecurityLogger();
            string operation = "CreateTask";
            TimeSpan duration = TimeSpan.FromMilliseconds(150);
            string details = "Task ID: 1";

            // Act & Assert
            // Since we can't easily test console output, we just ensure no exception is thrown
            await logger.LogPerformanceMetricAsync(operation, duration, details);
        }

        [TestMethod]
        public async Task ConsoleSecurityLogger_LogSecurityViolationAsync_LogsSecurityViolation()
        {
            // Arrange
            var logger = new ConsoleSecurityLogger();
            string resource = "LoginPage";
            string ipAddress = "192.168.1.1";
            string additionalInfo = "Failed login attempt for user: testuser";

            // Act & Assert
            // Since we can't easily test console output, we just ensure no exception is thrown
            await logger.LogSecurityViolationAsync(resource, ipAddress, additionalInfo);
        }

        #endregion
    }
}
