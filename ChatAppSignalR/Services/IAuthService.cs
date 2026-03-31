using ChatAppSignalR.DTOs;
using ChatAppSignalR.Models;

namespace ChatAppSignalR.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterRequest request);

        Task<(string accessToken, string refreshToken)?> LoginAsync(LoginRequest request);

        Task<(string accessToken, string refreshToken)?> RefreshAsync(string refreshToken);

        Task<bool> LogoutAsync(string refreshToken);
    }
}