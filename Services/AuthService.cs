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
