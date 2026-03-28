using JWT.DTO;
using JWT.Entities;
using JWTAuth.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTAuth.Services
{
    public class AuthService(MongoDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await context.Users
                .Find(u => u.Email == request.Email) 
                .FirstOrDefaultAsync();

            if (user is null)
                return null;

            if (new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return CreateToken(user);
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim (ClaimTypes.Name, user.Username),
                new Claim (ClaimTypes.NameIdentifier, user.Id.ToString()),
               
            };

            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds

                );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }

        public async Task<User?> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("username is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("password is required");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("email is required");

            
            var existingUser = await context.Users
                .Find(u => u.Username == request.Username || u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return null;

            var user = new User();

            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.Email = request.Email;
            user.PasswordHash = hashedPassword;

            
            await context.Users.InsertOneAsync(user);

            return user;
        }
    }
}