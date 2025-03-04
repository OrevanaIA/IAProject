using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AIProject.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación y autorización.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa la interfaz IAuthService y proporciona:
    /// - Autenticación de usuarios mediante credenciales
    /// - Registro de nuevos usuarios
    /// - Generación y validación de tokens JWT
    /// - Gestión segura de contraseñas mediante hashing
    /// - Registro de eventos de seguridad
    /// </remarks>
    public class AuthService : IAuthService
    {
        /// <summary>
        /// Configuración de la aplicación para acceder a los ajustes de JWT.
        /// </summary>
        private readonly IConfiguration _configuration;
        
        /// <summary>
        /// Repositorio para acceder a los datos de usuarios.
        /// </summary>
        private readonly IUserRepository _userRepository;
        
        /// <summary>
        /// Registrador de eventos de seguridad.
        /// </summary>
        private readonly ISecurityLogger _securityLogger;

        /// <summary>
        /// Constructor que inicializa una nueva instancia del servicio de autenticación.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="userRepository">Repositorio de usuarios</param>
        /// <param name="securityLogger">Registrador de eventos de seguridad</param>
        public AuthService(
            IConfiguration configuration,
            IUserRepository userRepository,
            ISecurityLogger securityLogger)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _securityLogger = securityLogger;
        }

        /// <summary>
        /// Autentica a un usuario con sus credenciales y genera un token JWT.
        /// </summary>
        /// <param name="loginDto">DTO con las credenciales del usuario</param>
        /// <returns>DTO con el token JWT y datos del usuario, o null si la autenticación falla</returns>
        /// <remarks>
        /// Este método:
        /// - Busca al usuario por su nombre de usuario
        /// - Verifica la contraseña utilizando el hash almacenado
        /// - Registra intentos de inicio de sesión fallidos
        /// - Actualiza la fecha del último inicio de sesión
        /// - Genera un token JWT para el usuario autenticado
        /// </remarks>
        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            
            if (user == null)
            {
                await _securityLogger.LogSecurityViolationAsync(
                    "Login", 
                    "Unknown", 
                    $"Failed login attempt for non-existent user: {loginDto.Username}");
                return null;
            }

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                await _securityLogger.LogSecurityViolationAsync(
                    "Login", 
                    "Unknown", 
                    $"Failed login attempt for user: {loginDto.Username}");
                return null;
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate JWT token
            string token = GenerateJwtToken(user);

            await _securityLogger.LogOperationAsync(
                "Login", 
                $"Successful login for user: {user.Username}", 
                user.Id.ToString());

            return new AuthResponseDTO
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                Username = user.Username,
                Role = user.Role
            };
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="registerDto">DTO con los datos del nuevo usuario</param>
        /// <returns>DTO con el token JWT y datos del usuario, o null si el registro falla</returns>
        /// <remarks>
        /// Este método:
        /// - Verifica que el nombre de usuario no exista
        /// - Valida que las contraseñas coincidan
        /// - Crea un hash seguro de la contraseña
        /// - Crea y almacena el nuevo usuario
        /// - Genera un token JWT para el usuario registrado
        /// </remarks>
        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
        {
            if (await UserExistsAsync(registerDto.Username))
            {
                return null;
            }

            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return null;
            }

            var passwordHash = CreatePasswordHash(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Role = "User", // Default role
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user);

            await _securityLogger.LogOperationAsync(
                "Register", 
                $"New user registered: {user.Username}", 
                user.Id.ToString());

            // Generate JWT token
            string token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1),
                Username = user.Username,
                Role = user.Role
            };
        }

        /// <summary>
        /// Verifica si un nombre de usuario ya existe en el sistema.
        /// </summary>
        /// <param name="username">Nombre de usuario a verificar</param>
        /// <returns>True si el usuario existe, False en caso contrario</returns>
        public async Task<bool> UserExistsAsync(string username)
        {
            return await _userRepository.ExistsByUsernameAsync(username);
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario a buscar</param>
        /// <returns>El usuario encontrado, o null si no existe</returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

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
        /// - Fecha de expiración (1 hora)
        /// - Firma con clave simétrica HMAC-SHA512
        /// </remarks>
        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JwtSettings:Key").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = creds,
                Issuer = _configuration.GetSection("JwtSettings:Issuer").Value,
                Audience = _configuration.GetSection("JwtSettings:Audience").Value
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Crea un hash seguro para una contraseña.
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash de la contraseña como string en Base64</returns>
        /// <remarks>
        /// Este método:
        /// - Utiliza HMACSHA512 para generar un salt aleatorio
        /// - Calcula el hash de la contraseña con el salt
        /// - Combina el salt y el hash en un único string codificado en Base64
        /// </remarks>
        private string CreatePasswordHash(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Combine salt and hash
            var hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash almacenado.
        /// </summary>
        /// <param name="password">Contraseña en texto plano a verificar</param>
        /// <param name="storedHash">Hash almacenado (salt + hash) en Base64</param>
        /// <returns>True si la contraseña coincide, False en caso contrario</returns>
        /// <remarks>
        /// Este método:
        /// - Decodifica el hash almacenado
        /// - Extrae el salt (primeros 64 bytes)
        /// - Calcula el hash de la contraseña proporcionada con el mismo salt
        /// - Compara byte a byte el hash calculado con el hash almacenado
        /// </remarks>
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            
            // Extract salt (first 64 bytes)
            var salt = new byte[64];
            Array.Copy(hashBytes, 0, salt, 0, 64);
            
            // Compute hash with the same salt
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            // Compare computed hash with stored hash
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != hashBytes[64 + i])
                    return false;
            }
            
            return true;
        }
    }
}
