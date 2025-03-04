using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Models;

namespace AIProject.Interfaces
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
