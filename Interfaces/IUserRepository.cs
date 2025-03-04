using System.Threading.Tasks;
using AIProject.Models;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que define las operaciones de acceso a datos para la entidad Usuario.
    /// </summary>
    /// <remarks>
    /// Esta interfaz proporciona métodos para:
    /// - Recuperar usuarios por ID, nombre de usuario o correo electrónico
    /// - Verificar la existencia de usuarios
    /// - Añadir, actualizar y eliminar usuarios
    /// </remarks>
    public interface IUserRepository
    {
        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador único del usuario</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        Task<User> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario a buscar</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        Task<User> GetByUsernameAsync(string username);

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo electrónico a buscar</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        Task<User> GetByEmailAsync(string email);

        /// <summary>
        /// Verifica si existe un usuario con el nombre de usuario especificado.
        /// </summary>
        /// <param name="username">Nombre de usuario a verificar</param>
        /// <returns>True si existe un usuario con ese nombre, False en caso contrario</returns>
        Task<bool> ExistsByUsernameAsync(string username);

        /// <summary>
        /// Verifica si existe un usuario con el correo electrónico especificado.
        /// </summary>
        /// <param name="email">Correo electrónico a verificar</param>
        /// <returns>True si existe un usuario con ese correo, False en caso contrario</returns>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Añade un nuevo usuario al repositorio.
        /// </summary>
        /// <param name="user">Usuario a añadir</param>
        /// <returns>El identificador asignado al nuevo usuario</returns>
        Task<int> AddAsync(User user);

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        /// <param name="user">Usuario con los datos actualizados</param>
        Task UpdateAsync(User user);

        /// <summary>
        /// Elimina un usuario del repositorio.
        /// </summary>
        /// <param name="id">Identificador del usuario a eliminar</param>
        Task DeleteAsync(int id);
    }
}
