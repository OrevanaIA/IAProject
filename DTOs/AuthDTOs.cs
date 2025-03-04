using System;

namespace AIProject.DTOs
{
    /// <summary>
    /// DTO para la solicitud de inicio de sesión.
    /// </summary>
    /// <remarks>
    /// Contiene las credenciales necesarias para autenticar a un usuario.
    /// </remarks>
    public class LoginDTO
    {
        /// <summary>
        /// Nombre de usuario para el inicio de sesión.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Contraseña del usuario en texto plano (solo para transmisión).
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// DTO para la solicitud de registro de un nuevo usuario.
    /// </summary>
    /// <remarks>
    /// Contiene los datos necesarios para crear una nueva cuenta de usuario.
    /// </remarks>
    public class RegisterDTO
    {
        /// <summary>
        /// Nombre de usuario único para el nuevo usuario.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Correo electrónico del nuevo usuario.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Contraseña elegida por el usuario en texto plano (solo para transmisión).
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Confirmación de la contraseña para verificar que coincide con la contraseña original.
        /// </summary>
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO para la respuesta de autenticación.
    /// </summary>
    /// <remarks>
    /// Contiene el token JWT y la información del usuario autenticado.
    /// </remarks>
    public class AuthResponseDTO
    {
        /// <summary>
        /// Token JWT para la autenticación del usuario.
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Fecha y hora de expiración del token.
        /// </summary>
        public DateTime Expiration { get; set; }
        
        /// <summary>
        /// Nombre de usuario del usuario autenticado.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Rol del usuario autenticado en el sistema.
        /// </summary>
        public string Role { get; set; }
    }
}
