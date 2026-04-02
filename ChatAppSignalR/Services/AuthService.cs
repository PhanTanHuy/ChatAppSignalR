using ChatAppSignalR.Data;
using ChatAppSignalR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatAppSignalR.DTOs;
using MongoDB.Driver;

namespace ChatAppSignalR.Services
{
    public class AuthService(MongoDbContext context, IConfiguration configuration) : IAuthService
    {
        //public async Task<string?> LoginAsync(LoginRequest request)
        public async Task<(string accessToken, string refreshToken)?> LoginAsync(LoginRequest request)
        {
            var user = await context.Users
                .Find(u => u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (user is null) return null;

            if (new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var accessToken = CreateToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await context.RefreshTokens.InsertOneAsync(refreshTokenEntity);

            return (accessToken, refreshToken);
            //return $"{accessToken}|{refreshToken}";
        }

        //private string CreateToken(User user)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim (ClaimTypes.Name, user.Username),
        //        new Claim (ClaimTypes.NameIdentifier, user.Id.ToString()),

        //    };

        //    var key = new SymmetricSecurityKey(
        //            Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        //    var tokenDescriptor = new JwtSecurityToken(
        //        issuer: configuration.GetValue<string>("AppSettings:Issuer"),
        //        audience: configuration.GetValue<string>("AppSettings:Audience"),
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddDays(1),
        //        signingCredentials: creds

        //        );

        //    return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        //}
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
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

            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(u => u.Username, request.Username),
                Builders<User>.Filter.Eq(u => u.Email, request.Email)
            );

            var existingUser = await context.Users
                .Find(filter)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return null;
            var user = new User();

            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.Email = request.Email;
            user.PasswordHash = hashedPassword;
            user.AvatarUrl = request.AvatarUrl;

            await context.Users.InsertOneAsync(user);

            return user;
        }
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
        public async Task<(string accessToken, string refreshToken)?> RefreshAsync(string refreshToken)
        {
            var tokenDoc = await context.RefreshTokens
                .Find(x => x.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (tokenDoc == null || tokenDoc.IsRevoked || tokenDoc.ExpiryDate < DateTime.UtcNow)
                return null;

            var user = await context.Users
                .Find(u => u.Id == tokenDoc.UserId)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            // revoke token cũ
            tokenDoc.IsRevoked = true;
            await context.RefreshTokens.ReplaceOneAsync(x => x.Id == tokenDoc.Id, tokenDoc);

            // tạo token mới
            var newAccessToken = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            await context.RefreshTokens.InsertOneAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });

            return (newAccessToken, newRefreshToken);
        }
        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var tokenDoc = await context.RefreshTokens
                .Find(x => x.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (tokenDoc == null) return false;

            tokenDoc.IsRevoked = true;

            await context.RefreshTokens.ReplaceOneAsync(x => x.Id == tokenDoc.Id, tokenDoc);

            return true;
        }
    }
}