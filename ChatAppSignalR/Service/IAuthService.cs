using JWT.DTO;
using JWT.Entities;

namespace JWTAuth.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterRequest request); 
        Task<string?> LoginAsync(LoginRequest request);
        Task<(string accessToken, string refreshToken)?> RefreshAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
    }
}