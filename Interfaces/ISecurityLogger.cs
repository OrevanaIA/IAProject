using System;
using System.Threading.Tasks;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que define las operaciones de logging para seguridad y auditoría
    /// del sistema de gestión de tareas.
    /// </summary>
    /// <remarks>
    /// Esta interfaz implementa:
    /// - Logging de operaciones críticas
    /// - Registro de intentos de acceso no autorizados
    /// - Tracking de modificaciones de datos sensibles
    /// - Auditoría de cambios en el sistema
    /// </remarks>
    public interface ISecurityLogger
    {
        /// <summary>
        /// Registra una operación crítica en el sistema.
        /// </summary>
        /// <param name="operation">Nombre de la operación</param>
        /// <param name="details">Detalles de la operación</param>
        /// <param name="userId">ID del usuario que realizó la operación</param>
        /// <remarks>
        /// Registra operaciones como:
        /// - Modificaciones de datos
        /// - Eliminaciones
        /// - Cambios de estado importantes
        /// Los logs incluyen timestamp, IP, y otros metadatos relevantes.
        /// </remarks>
        Task LogOperationAsync(string operation, string details, string userId);

        /// <summary>
        /// Registra un intento de acceso no autorizado.
        /// </summary>
        /// <param name="resource">Recurso al que se intentó acceder</param>
        /// <param name="ipAddress">Dirección IP del intento</param>
        /// <param name="additionalInfo">Información adicional del intento</param>
        /// <remarks>
        /// Se utiliza para:
        /// - Detectar patrones de ataque
        /// - Identificar intentos de acceso malicioso
        /// - Mantener un registro de seguridad
        /// </remarks>
        Task LogSecurityViolationAsync(string resource, string ipAddress, string additionalInfo);

        /// <summary>
        /// Registra cambios en datos sensibles del sistema.
        /// </summary>
        /// <param name="entityType">Tipo de entidad modificada</param>
        /// <param name="entityId">ID de la entidad</param>
        /// <param name="changes">Descripción de los cambios realizados</param>
        /// <param name="userId">ID del usuario que realizó los cambios</param>
        /// <remarks>
        /// Mantiene un historial detallado de:
        /// - Qué datos fueron modificados
        /// - Quién realizó los cambios
        /// - Cuándo se realizaron los cambios
        /// - Valores anteriores y nuevos
        /// </remarks>
        Task LogDataChangeAsync(string entityType, string entityId, string changes, string userId);

        /// <summary>
        /// Registra errores de validación y sanitización de datos.
        /// </summary>
        /// <param name="inputType">Tipo de entrada que falló la validación</param>
        /// <param name="invalidValue">Valor inválido detectado</param>
        /// <param name="validationError">Descripción del error de validación</param>
        /// <remarks>
        /// Ayuda a:
        /// - Identificar intentos de inyección
        /// - Detectar patrones de entrada maliciosa
        /// - Mejorar las validaciones del sistema
        /// </remarks>
        Task LogValidationFailureAsync(string inputType, string invalidValue, string validationError);

        /// <summary>
        /// Registra eventos de rendimiento y optimización.
        /// </summary>
        /// <param name="operation">Operación monitoreada</param>
        /// <param name="duration">Duración de la operación</param>
        /// <param name="performanceMetrics">Métricas adicionales de rendimiento</param>
        /// <remarks>
        /// Utilizado para:
        /// - Monitorear tiempos de respuesta
        /// - Identificar cuellos de botella
        /// - Optimizar consultas y operaciones
        /// </remarks>
        Task LogPerformanceMetricAsync(string operation, TimeSpan duration, string performanceMetrics);
    }
}
