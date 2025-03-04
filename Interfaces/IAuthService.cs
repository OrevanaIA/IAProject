using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Models;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que define los servicios de autenticación y autorización para el sistema.
    /// </summary>
    /// <remarks>
    /// Esta interfaz proporciona métodos para:
    /// - Autenticación de usuarios (login)
    /// - Registro de nuevos usuarios
    /// - Verificación de existencia de usuarios
    /// - Generación de tokens JWT para autenticación
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica a un usuario con sus credenciales y genera un token JWT.
        /// </summary>
        /// <param name="loginDto">DTO con las credenciales del usuario (nombre de usuario y contraseña)</param>
        /// <returns>DTO con el token JWT, fecha de expiración y datos del usuario, o null si la autenticación falla</returns>
        /// <remarks>
        /// Este método:
        /// - Verifica las credenciales del usuario
        /// - Registra intentos de inicio de sesión fallidos
        /// - Actualiza la fecha del último inicio de sesión
        /// - Genera un token JWT para el usuario autenticado
        /// </remarks>
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="registerDto">DTO con los datos del nuevo usuario</param>
        /// <returns>DTO con el token JWT, fecha de expiración y datos del usuario, o null si el registro falla</returns>
        /// <remarks>
        /// Este método:
        /// - Verifica que el nombre de usuario no exista
        /// - Valida que las contraseñas coincidan
        /// - Crea un hash seguro de la contraseña
        /// - Registra el nuevo usuario en el sistema
        /// - Genera un token JWT para el usuario registrado
        /// </remarks>
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto);

        /// <summary>
        /// Verifica si un nombre de usuario ya existe en el sistema.
        /// </summary>
        /// <param name="username">Nombre de usuario a verificar</param>
        /// <returns>True si el usuario existe, False en caso contrario</returns>
        Task<bool> UserExistsAsync(string username);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario a buscar</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        Task<User> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Genera un token JWT para un usuario.
        /// </summary>
        /// <param name="user">Usuario para el que se generará el token</param>
        /// <returns>Token JWT como string</returns>
        /// <remarks>
        /// El token incluye:
        /// - Identificador del usuario
        /// - Nombre de usuario
        /// - Correo electrónico
        /// - Rol del usuario
        /// - Fecha de expiración
        /// </remarks>
        string GenerateJwtToken(User user);
    }
}
