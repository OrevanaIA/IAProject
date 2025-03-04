# Guía de Implementación: Control de Acceso con JWT

## Introducción

Esta guía detalla los pasos para implementar un sistema de control de acceso en la aplicación AIModulo03 utilizando autenticación y autorización basada en JWT (JSON Web Tokens). Esta implementación permitirá:

1. Autenticar usuarios de forma segura
2. Autorizar acceso a recursos basado en roles y permisos
3. Proteger las APIs y endpoints de la aplicación
4. Mantener sesiones de usuario sin estado (stateless)

## Requisitos Previos

- .NET 6.0 SDK
- Paquetes NuGet:
  - Microsoft.AspNetCore.Authentication.JwtBearer
  - System.IdentityModel.Tokens.Jwt

## Pasos de Implementación

### 1. Instalación de Paquetes NuGet

Primero, instale los paquetes necesarios:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

### 2. Configuración de Modelos y Entidades

#### 2.1 Crear Modelo de Usuario

Cree un nuevo archivo `Models/User.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace AIModulo03.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Roles: Admin, User, etc.
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
    }
}
```

#### 2.2 Crear DTOs para Autenticación

Cree un nuevo archivo `DTOs/AuthDTOs.cs`:

```csharp
using System;

namespace AIModulo03.DTOs
{
    public class LoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
```

### 3. Implementación de Interfaces

#### 3.1 Crear Interfaz para el Servicio de Autenticación

Cree un nuevo archivo `Interfaces/IAuthService.cs`:

```csharp
using System.Threading.Tasks;
using AIModulo03.DTOs;
using AIModulo03.Models;

namespace AIModulo03.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto);
        Task<bool> UserExistsAsync(string username);
        Task<User> GetUserByUsernameAsync(string username);
        string GenerateJwtToken(User user);
    }
}
```

#### 3.2 Crear Interfaz para el Repositorio de Usuarios

Cree un nuevo archivo `Interfaces/IUserRepository.cs`:

```csharp
using System.Threading.Tasks;
using AIModulo03.Models;

namespace AIModulo03.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
        Task<int> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
```

### 4. Implementación de Repositorios

#### 4.1 Implementar el Repositorio de Usuarios

Cree un nuevo archivo `Infrastructure/UserRepository.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIModulo03.Interfaces;
using AIModulo03.Models;

namespace AIModulo03.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly string _filePath;
        private List<User> _users;
        private readonly object _lock = new object();

        public UserRepository(string filePath = "users.json")
        {
            _filePath = filePath;
            LoadUsers();
        }

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

        private void SaveChanges()
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await Task.FromResult(_users.Any(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await Task.FromResult(_users.Any(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

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
```

### 5. Implementación de Servicios

#### 5.1 Implementar el Servicio de Autenticación

Cree un nuevo archivo `Services/AuthService.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AIModulo03.DTOs;
using AIModulo03.Interfaces;
using AIModulo03.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AIModulo03.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ISecurityLogger _securityLogger;

        public AuthService(
            IConfiguration configuration,
            IUserRepository userRepository,
            ISecurityLogger securityLogger)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _securityLogger = securityLogger;
        }

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

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _userRepository.ExistsByUsernameAsync(username);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

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
```

### 6. Configuración de JWT

#### 6.1 Actualizar appsettings.json

Agregue la configuración JWT a su archivo `appsettings.json`:

```json
{
  "JwtSettings": {
    "Key": "YourSuperSecretKeyHereMakeItLongAndComplex",
    "Issuer": "AIModulo03",
    "Audience": "AIModulo03Users",
    "DurationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

#### 6.2 Configurar Servicios JWT en Program.cs

Actualice su archivo `Program.cs` para configurar la autenticación JWT:

```csharp
using System.Text;
using AIModulo03.Infrastructure;
using AIModulo03.Interfaces;
using AIModulo03.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration.GetSection("JwtSettings:Key").Value)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value,
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetSection("JwtSettings:Audience").Value,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Admin"));
});

// Register services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### 7. Implementación de Controladores

#### 7.1 Crear Controlador de Autenticación

Cree un nuevo archivo `Controllers/AuthController.cs`:

```csharp
using System.Threading.Tasks;
using AIModulo03.DTOs;
using AIModulo03.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIModulo03.Controllers
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
```

#### 7.2 Crear Controlador Protegido

Cree un nuevo archivo `Controllers/TasksController.cs` con protección JWT:

```csharp
using System.Threading.Tasks;
using AIModulo03.DTOs;
using AIModulo03.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIModulo03.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTask = await _taskService.CreateTaskAsync(taskDto);
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDTO taskDto)
        {
            if (id != taskDto.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _taskService.UpdateTaskAsync(taskDto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")] // Solo administradores pueden eliminar
        public async Task<IActionResult> DeleteTask(int id)
        {
            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
```

### 8. Pruebas

#### 8.1 Probar el Registro de Usuario

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","email":"admin@example.com","password":"Password123!","confirmPassword":"Password123!"}'
```

#### 8.2 Probar el Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Password123!"}'
```

#### 8.3 Probar un Endpoint Protegido

```bash
curl -X GET https://localhost:5001/api/tasks \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### 9. Consideraciones de Seguridad

1. **Almacenamiento Seguro de Claves**: Asegúrese de que la clave secreta JWT esté almacenada de forma segura, preferiblemente en variables de entorno o en un almacén de secretos.

2. **Expiración de Tokens**: Configure una expiración razonable para los tokens JWT (por ejemplo, 1 hora).

3. **HTTPS**: Asegúrese de que su aplicación utilice HTTPS para proteger los tokens en tránsito.

4. **Validación de Tokens**: Valide correctamente los tokens JWT, incluyendo emisor, audiencia y firma.

5. **Rotación de Claves**: Implemente un mecanismo para rotar periódicamente las claves de firma JWT.

6. **Revocación de Tokens**: Considere implementar una lista negra de tokens para revocar tokens específicos si es necesario.

## Conclusión

Esta implementación proporciona un sistema de autenticación y autorización basado en JWT para su aplicación AIModulo03. Los usuarios pueden registrarse, iniciar sesión y acceder a recursos protegidos según sus roles y permisos. La solución es escalable y sigue las mejores prácticas de seguridad.

Para mejorar aún más esta implementación, considere:

1. Agregar validación de contraseñas más robusta
2. Implementar autenticación de dos factores
3. Agregar límites de intentos de inicio de sesión
4. Implementar un sistema de gestión de roles más avanzado
5. Agregar auditoría detallada de acciones de seguridad
