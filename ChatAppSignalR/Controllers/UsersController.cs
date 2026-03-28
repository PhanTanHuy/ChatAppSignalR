using System.Security.Claims;
using ChatAppSignalR.DTOs;
using ChatAppSignalR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppSignalR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserResponse>> GetMe()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized(new { message = "User không tồn tại hoặc token không hợp lệ" });
            }

            var response = UserService.ToUserResponse(user);
            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<UserDto>>> Search([FromQuery] UserSearchQuery query)
        {
            var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            var result = await _userService.SearchUsersAsync(currentUserId, query.Username);
            return Ok(result);
        }
    }
}