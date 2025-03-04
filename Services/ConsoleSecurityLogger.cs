using System;
using System.Threading.Tasks;
using AIProject.Interfaces;

namespace AIProject.Services
{
    /// <summary>
    /// Implementación simple del logger de seguridad que escribe en la consola.
    /// Esta implementación es útil para desarrollo y pruebas.
    /// </summary>
    public class ConsoleSecurityLogger : ISecurityLogger
    {
        public Task LogOperationAsync(string operation, string details, string userId)
        {
            Console.WriteLine($"[OPERATION] {DateTime.UtcNow} - User: {userId} - {operation} - {details}");
            return Task.CompletedTask;
        }

        public Task LogSecurityViolationAsync(string resource, string ipAddress, string additionalInfo)
        {
            Console.WriteLine($"[SECURITY] {DateTime.UtcNow} - IP: {ipAddress} - Resource: {resource} - {additionalInfo}");
            return Task.CompletedTask;
        }

        public Task LogDataChangeAsync(string entityType, string entityId, string changes, string userId)
        {
            Console.WriteLine($"[DATA] {DateTime.UtcNow} - User: {userId} - Entity: {entityType}/{entityId} - Changes: {changes}");
            return Task.CompletedTask;
        }

        public Task LogValidationFailureAsync(string inputType, string invalidValue, string validationError)
        {
            Console.WriteLine($"[VALIDATION] {DateTime.UtcNow} - Type: {inputType} - Value: {invalidValue} - Error: {validationError}");
            return Task.CompletedTask;
        }

        public Task LogPerformanceMetricAsync(string operation, TimeSpan duration, string performanceMetrics)
        {
            Console.WriteLine($"[PERFORMANCE] {DateTime.UtcNow} - Operation: {operation} - Duration: {duration.TotalMilliseconds}ms - Metrics: {performanceMetrics}");
            return Task.CompletedTask;
        }
    }
}
