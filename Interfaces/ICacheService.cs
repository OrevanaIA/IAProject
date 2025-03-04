using System;
using System.Threading.Tasks;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que define las operaciones de caché para optimizar el rendimiento
    /// del sistema de gestión de tareas.
    /// </summary>
    /// <remarks>
    /// Esta interfaz implementa el patrón Cache Aside para:
    /// - Reducir la carga en la base de datos
    /// - Mejorar los tiempos de respuesta
    /// - Optimizar el acceso a datos frecuentemente utilizados
    /// </remarks>
    /// <example>
    /// Ejemplo de uso básico:
    /// <code>
    /// ICacheService cache = new RedisCacheService();
    /// var key = $"task_{taskId}";
    /// 
    /// // Intentar obtener de caché primero
    /// var task = await cache.GetAsync&lt;TaskDTO&gt;(key);
    /// if (task == null)
    /// {
    ///     // Si no está en caché, obtener de la base de datos
    ///     task = _taskRepository.GetById(taskId);
    ///     // Guardar en caché para futuras consultas
    ///     await cache.SetAsync(key, task, TimeSpan.FromMinutes(30));
    /// }
    /// </code>
    /// </example>
    public interface ICacheService
    {
        /// <summary>
        /// Obtiene un valor de la caché de forma asíncrona.
        /// </summary>
        /// <typeparam name="T">Tipo del valor a obtener</typeparam>
        /// <param name="key">Clave única del valor en caché</param>
        /// <returns>El valor almacenado o null si no existe</returns>
        /// <remarks>
        /// Implementa un patrón de retry con exponential backoff para
        /// manejar fallos temporales en el servicio de caché.
        /// </remarks>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Almacena un valor en la caché de forma asíncrona.
        /// </summary>
        /// <typeparam name="T">Tipo del valor a almacenar</typeparam>
        /// <param name="key">Clave única para el valor</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="expiration">Tiempo de expiración del valor en caché</param>
        /// <returns>True si se almacenó correctamente, False en caso contrario</returns>
        /// <exception cref="ArgumentNullException">Si key o value son null</exception>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;

        /// <summary>
        /// Elimina un valor de la caché de forma asíncrona.
        /// </summary>
        /// <param name="key">Clave del valor a eliminar</param>
        /// <returns>True si se eliminó correctamente, False si no existía</returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// Verifica si un valor existe en la caché.
        /// </summary>
        /// <param name="key">Clave a verificar</param>
        /// <returns>True si existe, False en caso contrario</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Actualiza el tiempo de expiración de un valor en caché.
        /// </summary>
        /// <param name="key">Clave del valor</param>
        /// <param name="expiration">Nuevo tiempo de expiración</param>
        /// <returns>True si se actualizó correctamente, False si no existe</returns>
        Task<bool> UpdateExpirationAsync(string key, TimeSpan expiration);
    }
}
