using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AIProject.Interfaces;

namespace AIProject.Services
{
    /// <summary>
    /// Implementación en memoria del servicio de caché que proporciona almacenamiento temporal de datos.
    /// </summary>
    /// <remarks>
    /// Esta implementación:
    /// - Almacena datos en memoria utilizando un diccionario concurrente
    /// - Gestiona la expiración automática de elementos
    /// - Es thread-safe para entornos multi-hilo
    /// - Es adecuada para desarrollo, pruebas y aplicaciones con carga moderada
    /// - No persiste datos entre reinicios de la aplicación
    /// </remarks>
    /// <example>
    /// Ejemplo de uso básico:
    /// <code>
    /// var cacheService = new InMemoryCacheService();
    /// await cacheService.SetAsync("key1", myObject, TimeSpan.FromMinutes(10));
    /// var cachedObject = await cacheService.GetAsync&lt;MyType&gt;("key1");
    /// </code>
    /// </example>
    public class InMemoryCacheService : ICacheService
    {
        /// <summary>
        /// Clase interna que representa un elemento almacenado en caché.
        /// </summary>
        /// <remarks>
        /// Contiene el valor almacenado y su tiempo de expiración.
        /// </remarks>
        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        /// <summary>
        /// Diccionario concurrente que almacena los elementos en caché.
        /// </summary>
        /// <remarks>
        /// Se utiliza ConcurrentDictionary para garantizar operaciones thread-safe.
        /// </remarks>
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        /// <summary>
        /// Obtiene un elemento de la caché de forma asíncrona.
        /// </summary>
        /// <typeparam name="T">Tipo del elemento a recuperar</typeparam>
        /// <param name="key">Clave única que identifica el elemento</param>
        /// <returns>El elemento almacenado, o null si no existe o ha expirado</returns>
        /// <exception cref="ArgumentNullException">Si la clave es null o vacía</exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la clave no sea null o vacía
        /// - Busca el elemento en la caché
        /// - Verifica si el elemento ha expirado
        /// - Elimina automáticamente elementos expirados
        /// - Convierte el elemento al tipo especificado
        /// </remarks>
        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    return Task.FromResult(item.Value as T);
                }
                
                // Remove expired item
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult<T?>(null);
        }

        /// <summary>
        /// Almacena un elemento en la caché de forma asíncrona.
        /// </summary>
        /// <typeparam name="T">Tipo del elemento a almacenar</typeparam>
        /// <param name="key">Clave única para identificar el elemento</param>
        /// <param name="value">Elemento a almacenar</param>
        /// <param name="expiration">Tiempo de expiración del elemento</param>
        /// <returns>True si el elemento se almacenó correctamente</returns>
        /// <exception cref="ArgumentNullException">Si la clave o el valor son null</exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la clave y el valor no sean null
        /// - Crea un nuevo elemento de caché con el valor y tiempo de expiración
        /// - Almacena o actualiza el elemento en la caché
        /// - Si ya existe un elemento con la misma clave, lo sobrescribe
        /// </remarks>
        public Task<bool> SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var cacheItem = new CacheItem
            {
                Value = value,
                ExpirationTime = DateTime.UtcNow.Add(expiration)
            };

            _cache[key] = cacheItem;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Elimina un elemento de la caché de forma asíncrona.
        /// </summary>
        /// <param name="key">Clave del elemento a eliminar</param>
        /// <returns>True si el elemento existía y fue eliminado, False en caso contrario</returns>
        /// <exception cref="ArgumentNullException">Si la clave es null o vacía</exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la clave no sea null o vacía
        /// - Intenta eliminar el elemento de la caché
        /// - Devuelve True si el elemento existía y fue eliminado
        /// </remarks>
        public Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return Task.FromResult(_cache.TryRemove(key, out _));
        }

        /// <summary>
        /// Verifica si un elemento existe en la caché de forma asíncrona.
        /// </summary>
        /// <param name="key">Clave del elemento a verificar</param>
        /// <returns>True si el elemento existe y no ha expirado, False en caso contrario</returns>
        /// <exception cref="ArgumentNullException">Si la clave es null o vacía</exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la clave no sea null o vacía
        /// - Busca el elemento en la caché
        /// - Verifica si el elemento ha expirado
        /// - Elimina automáticamente elementos expirados
        /// </remarks>
        public Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    return Task.FromResult(true);
                }
                
                // Remove expired item
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Actualiza el tiempo de expiración de un elemento en la caché de forma asíncrona.
        /// </summary>
        /// <param name="key">Clave del elemento a actualizar</param>
        /// <param name="expiration">Nuevo tiempo de expiración</param>
        /// <returns>True si el elemento existía y fue actualizado, False en caso contrario</returns>
        /// <exception cref="ArgumentNullException">Si la clave es null o vacía</exception>
        /// <remarks>
        /// Este método:
        /// - Verifica que la clave no sea null o vacía
        /// - Busca el elemento en la caché
        /// - Verifica si el elemento ha expirado
        /// - Actualiza el tiempo de expiración si el elemento existe y no ha expirado
        /// - Elimina automáticamente elementos expirados
        /// </remarks>
        public Task<bool> UpdateExpirationAsync(string key, TimeSpan expiration)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    item.ExpirationTime = DateTime.UtcNow.Add(expiration);
                    return Task.FromResult(true);
                }
                
                // Remove expired item
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult(false);
        }
    }
}
