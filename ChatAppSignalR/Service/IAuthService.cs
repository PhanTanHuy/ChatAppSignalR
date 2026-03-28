using JWT.DTO;
using JWT.Entities;

namespace JWTAuth.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterRequest request); 
        Task<string?> LoginAsync(LoginRequest request);    
    }
}