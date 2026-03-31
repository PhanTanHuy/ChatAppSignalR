using ChatAppSignalR.Data;
using ChatAppSignalR.DTOs;
using ChatAppSignalR.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatAppSignalR.Services
{
    public class UserService
    {
        private readonly MongoDbContext _context;

        public UserService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(string userId)
        {
            return await _context.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserDto>> SearchUsersAsync(string currentUserId, string username)
        {
            var filterBuilder = Builders<User>.Filter;

            var filter = filterBuilder.And(
                filterBuilder.Ne(u => u.Id, currentUserId),
                filterBuilder.Regex(u => u.Username, new BsonRegularExpression(username, "i"))
            );

            var users = await _context.Users
                .Find(filter)
                .Limit(20)
                .ToListAsync();

            var friendRelations = await _context.Friends
                .Find(f => f.UserId == currentUserId)
                .ToListAsync();

            var friendIds = friendRelations
                .Select(f => f.FriendUserId)
                .ToHashSet();

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl,
                IsFriend = friendIds.Contains(u.Id)
            }).ToList();

            return result;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersExceptAsync(string currentUserId)
        {
            var filter = Builders<User>.Filter.Ne(u => u.Id, currentUserId);

            var users = await _context.Users
                .Find(filter)
                .ToListAsync();

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
            }).ToList();

            return result;
        }

        public static UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
               
            };
        }
    }
}