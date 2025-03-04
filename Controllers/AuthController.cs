using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIProject.Controllers
{
    /// <summary>
    /// Controlador que gestiona la autenticación y registro de usuarios.
    /// </summary>
    /// <remarks>
    /// Este controlador proporciona endpoints para:
    /// - Registro de nuevos usuarios
    /// - Inicio de sesión de usuarios existentes
    /// - Generación de tokens JWT para autenticación
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Servicio de autenticación utilizado por el controlador.
        /// </summary>
        private readonly IAuthService _authService;

        /// <summary>
        /// Constructor que inicializa una nueva instancia del controlador de autenticación.
        /// </summary>
        /// <param name="authService">Servicio de autenticación a utilizar</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="registerDto">DTO con los datos del nuevo usuario</param>
        /// <returns>
        /// 200 OK con el token JWT y datos del usuario si el registro es exitoso.
        /// 400 Bad Request si los datos son inválidos o el usuario ya existe.
        /// </returns>
        /// <remarks>
        /// Este endpoint:
        /// - Valida que el modelo sea válido
        /// - Verifica que el nombre de usuario no exista
        /// - Confirma que las contraseñas coincidan
        /// - Registra al usuario y genera un token JWT
        /// </remarks>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _authService.UserExistsAsync(registerDto.Username))
                return BadRequest("Username already exists");

            if (registerDto.Password != registerDto.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
                return BadRequest("Registration failed");

            return Ok(result);
        }

        /// <summary>
        /// Autentica a un usuario existente.
        /// </summary>
        /// <param name="loginDto">DTO con las credenciales del usuario</param>
        /// <returns>
        /// 200 OK con el token JWT y datos del usuario si la autenticación es exitosa.
        /// 400 Bad Request si el modelo es inválido.
        /// 401 Unauthorized si las credenciales son incorrectas.
        /// </returns>
        /// <remarks>
        /// Este endpoint:
        /// - Valida que el modelo sea válido
        /// - Verifica las credenciales del usuario
        /// - Genera un token JWT para el usuario autenticado
        /// </remarks>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
                return Unauthorized("Invalid username or password");

            return Ok(result);
        }
    }
}
