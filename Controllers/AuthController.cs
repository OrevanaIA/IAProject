using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

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
