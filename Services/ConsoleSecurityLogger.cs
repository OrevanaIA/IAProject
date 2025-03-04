using System;
using System.Threading.Tasks;
using AIProject.Interfaces;

namespace AIProject.Services
{
    /// <summary>
    /// Implementación simple del logger de seguridad que escribe en la consola.
    /// </summary>
    /// <remarks>
    /// Esta implementación:
    /// - Escribe todos los logs en la consola estándar
    /// - Formatea los mensajes con prefijos para identificar el tipo de log
    /// - Incluye timestamps en UTC para cada entrada
    /// - Es útil para desarrollo, depuración y entornos de prueba
    /// - No es recomendable para producción (usar implementaciones que persistan los logs)
    /// </remarks>
    public class ConsoleSecurityLogger : ISecurityLogger
    {
        /// <summary>
        /// Registra una operación crítica en el sistema escribiendo en la consola.
        /// </summary>
        /// <param name="operation">Nombre de la operación</param>
        /// <param name="details">Detalles de la operación</param>
        /// <param name="userId">ID del usuario que realizó la operación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <remarks>
        /// Formato del log: [OPERATION] {timestamp} - User: {userId} - {operation} - {details}
        /// </remarks>
        public Task LogOperationAsync(string operation, string details, string userId)
        {
            Console.WriteLine($"[OPERATION] {DateTime.UtcNow} - User: {userId} - {operation} - {details}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registra un intento de acceso no autorizado escribiendo en la consola.
        /// </summary>
        /// <param name="resource">Recurso al que se intentó acceder</param>
        /// <param name="ipAddress">Dirección IP del intento</param>
        /// <param name="additionalInfo">Información adicional del intento</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <remarks>
        /// Formato del log: [SECURITY] {timestamp} - IP: {ipAddress} - Resource: {resource} - {additionalInfo}
        /// </remarks>
        public Task LogSecurityViolationAsync(string resource, string ipAddress, string additionalInfo)
        {
            Console.WriteLine($"[SECURITY] {DateTime.UtcNow} - IP: {ipAddress} - Resource: {resource} - {additionalInfo}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registra cambios en datos sensibles del sistema escribiendo en la consola.
        /// </summary>
        /// <param name="entityType">Tipo de entidad modificada</param>
        /// <param name="entityId">ID de la entidad</param>
        /// <param name="changes">Descripción de los cambios realizados</param>
        /// <param name="userId">ID del usuario que realizó los cambios</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <remarks>
        /// Formato del log: [DATA] {timestamp} - User: {userId} - Entity: {entityType}/{entityId} - Changes: {changes}
        /// </remarks>
        public Task LogDataChangeAsync(string entityType, string entityId, string changes, string userId)
        {
            Console.WriteLine($"[DATA] {DateTime.UtcNow} - User: {userId} - Entity: {entityType}/{entityId} - Changes: {changes}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registra errores de validación y sanitización de datos escribiendo en la consola.
        /// </summary>
        /// <param name="inputType">Tipo de entrada que falló la validación</param>
        /// <param name="invalidValue">Valor inválido detectado</param>
        /// <param name="validationError">Descripción del error de validación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <remarks>
        /// Formato del log: [VALIDATION] {timestamp} - Type: {inputType} - Value: {invalidValue} - Error: {validationError}
        /// </remarks>
        public Task LogValidationFailureAsync(string inputType, string invalidValue, string validationError)
        {
            Console.WriteLine($"[VALIDATION] {DateTime.UtcNow} - Type: {inputType} - Value: {invalidValue} - Error: {validationError}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registra eventos de rendimiento y optimización escribiendo en la consola.
        /// </summary>
        /// <param name="operation">Operación monitoreada</param>
        /// <param name="duration">Duración de la operación</param>
        /// <param name="performanceMetrics">Métricas adicionales de rendimiento</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <remarks>
        /// Formato del log: [PERFORMANCE] {timestamp} - Operation: {operation} - Duration: {duration}ms - Metrics: {performanceMetrics}
        /// </remarks>
        public Task LogPerformanceMetricAsync(string operation, TimeSpan duration, string performanceMetrics)
        {
            Console.WriteLine($"[PERFORMANCE] {DateTime.UtcNow} - Operation: {operation} - Duration: {duration.TotalMilliseconds}ms - Metrics: {performanceMetrics}");
            return Task.CompletedTask;
        }
    }
}
