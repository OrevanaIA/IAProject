using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIProject.Interfaces;
using AIProject.Models;

namespace AIProject.Infrastructure
{
    /// <summary>
    /// Implementación del repositorio de usuarios que almacena los datos en un archivo JSON.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa la interfaz IUserRepository y proporciona:
    /// - Persistencia de usuarios en un archivo JSON
    /// - Operaciones CRUD para la entidad Usuario
    /// - Sincronización para acceso concurrente mediante bloqueos
    /// - Búsqueda de usuarios por diferentes criterios
    /// </remarks>
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// Ruta del archivo JSON donde se almacenan los usuarios.
        /// </summary>
        private readonly string _filePath;
        
        /// <summary>
        /// Lista en memoria de los usuarios cargados desde el archivo.
        /// </summary>
        private List<User> _users;
        
        /// <summary>
        /// Objeto de bloqueo para sincronización de acceso concurrente.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor que inicializa una nueva instancia del repositorio de usuarios.
        /// </summary>
        /// <param name="filePath">Ruta del archivo JSON donde se almacenarán los usuarios (por defecto: "users.json")</param>
        /// <remarks>
        /// Al crear una instancia, se cargan automáticamente los usuarios desde el archivo especificado.
        /// Si el archivo no existe, se crea uno nuevo con una lista vacía.
        /// </remarks>
        public UserRepository(string filePath = "users.json")
        {
            _filePath = filePath;
            LoadUsers();
        }

        /// <summary>
        /// Carga los usuarios desde el archivo JSON a la memoria.
        /// </summary>
        /// <remarks>
        /// Este método:
        /// - Utiliza un bloqueo para garantizar acceso seguro en entornos multi-hilo
        /// - Crea un archivo nuevo si no existe
        /// - Deserializa los usuarios desde JSON a objetos User
        /// </remarks>
        private void LoadUsers()
        {
            lock (_lock)
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
                else
                {
                    _users = new List<User>();
                    SaveChanges(); // Create the file
                }
            }
        }

        /// <summary>
        /// Guarda los cambios en los usuarios al archivo JSON.
        /// </summary>
        /// <remarks>
        /// Este método:
        /// - Utiliza un bloqueo para garantizar acceso seguro en entornos multi-hilo
        /// - Serializa la lista de usuarios a formato JSON con formato indentado
        /// - Sobrescribe el archivo existente con los nuevos datos
        /// </remarks>
        private void SaveChanges()
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
        }

        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador único del usuario</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        public async Task<User> GetByIdAsync(int id)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario a buscar</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        /// <remarks>
        /// La búsqueda no distingue entre mayúsculas y minúsculas.
        /// </remarks>
        public async Task<User> GetByUsernameAsync(string username)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo electrónico a buscar</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        /// <remarks>
        /// La búsqueda no distingue entre mayúsculas y minúsculas.
        /// </remarks>
        public async Task<User> GetByEmailAsync(string email)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Verifica si existe un usuario con el nombre de usuario especificado.
        /// </summary>
        /// <param name="username">Nombre de usuario a verificar</param>
        /// <returns>True si existe un usuario con ese nombre, False en caso contrario</returns>
        /// <remarks>
        /// La verificación no distingue entre mayúsculas y minúsculas.
        /// </remarks>
        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await Task.FromResult(_users.Any(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Verifica si existe un usuario con el correo electrónico especificado.
        /// </summary>
        /// <param name="email">Correo electrónico a verificar</param>
        /// <returns>True si existe un usuario con ese correo, False en caso contrario</returns>
        /// <remarks>
        /// La verificación no distingue entre mayúsculas y minúsculas.
        /// </remarks>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await Task.FromResult(_users.Any(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Añade un nuevo usuario al repositorio.
        /// </summary>
        /// <param name="user">Usuario a añadir</param>
        /// <returns>El identificador asignado al nuevo usuario</returns>
        /// <exception cref="ArgumentNullException">Si el usuario es null</exception>
        /// <remarks>
        /// Este método:
        /// - Genera automáticamente un ID si no está establecido
        /// - Añade el usuario a la colección en memoria
        /// - Guarda los cambios en el archivo JSON
        /// </remarks>
        public async Task<int> AddAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            // Generate ID if not set
            if (user.Id <= 0)
            {
                user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            }
            
            _users.Add(user);
            SaveChanges();
            
            return await Task.FromResult(user.Id);
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        /// <param name="user">Usuario con los datos actualizados</param>
        /// <exception cref="ArgumentNullException">Si el usuario es null</exception>
        /// <exception cref="InvalidOperationException">Si el usuario no existe en el repositorio</exception>
        /// <remarks>
        /// Este método:
        /// - Busca el usuario por su ID
        /// - Reemplaza el usuario existente con el nuevo
        /// - Guarda los cambios en el archivo JSON
        /// </remarks>
        public async Task UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var existingIndex = _users.FindIndex(u => u.Id == user.Id);
            if (existingIndex == -1)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found");
            }

            _users[existingIndex] = user;
            SaveChanges();
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Elimina un usuario del repositorio.
        /// </summary>
        /// <param name="id">Identificador del usuario a eliminar</param>
        /// <remarks>
        /// Este método:
        /// - Busca el usuario por su ID
        /// - Si existe, lo elimina de la colección
        /// - Guarda los cambios en el archivo JSON
        /// - No lanza excepciones si el usuario no existe
        /// </remarks>
        public async Task DeleteAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _users.Remove(user);
                SaveChanges();
            }
            
            await Task.CompletedTask;
        }
    }
}
