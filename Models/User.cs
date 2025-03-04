using System;
using System.Collections.Generic;

namespace AIProject.Models
{
    /// <summary>
    /// Modelo que representa un usuario del sistema.
    /// </summary>
    /// <remarks>
    /// Esta clase contiene:
    /// - Información básica del usuario (ID, nombre, correo)
    /// - Información de autenticación (hash de contraseña, rol)
    /// - Metadatos (fechas de creación y último acceso, estado)
    /// </remarks>
    public class User
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nombre de usuario único en el sistema.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Hash de la contraseña del usuario (no se almacena la contraseña en texto plano).
        /// </summary>
        /// <remarks>
        /// El hash incluye el salt y está codificado en Base64.
        /// </remarks>
        public string PasswordHash { get; set; }
        
        /// <summary>
        /// Rol del usuario en el sistema (Admin, User, etc.).
        /// </summary>
        /// <remarks>
        /// Los roles determinan los permisos y acceso a funcionalidades.
        /// </remarks>
        public string Role { get; set; }
        
        /// <summary>
        /// Fecha y hora de creación de la cuenta.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Fecha y hora del último inicio de sesión.
        /// </summary>
        public DateTime LastLogin { get; set; }
        
        /// <summary>
        /// Indica si la cuenta está activa.
        /// </summary>
        /// <remarks>
        /// Las cuentas inactivas no pueden iniciar sesión.
        /// </remarks>
        public bool IsActive { get; set; }
    }
}
