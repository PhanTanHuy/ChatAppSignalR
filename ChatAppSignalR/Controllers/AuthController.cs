using ChatAppSignalR.Services;
using ChatAppSignalR.DTOs;
using ChatAppSignalR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {


        [HttpPost("register")]
        public async Task<ActionResult<object>> Register(RegisterRequest request)
        {
            var user = await authService.RegisterAsync(request);

            if (user is null)
                return BadRequest("User already exists");
            return Ok(new
            {
                user.Username,
                user.Email,
               
                user.AvatarUrl
            });
            //return Ok(new
            //{
            //    user.Username,
            //    user.Email
            //}); 
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await authService.LoginAsync(request);

            if (result == null)
                return BadRequest("Invalid email or password!");

            return Ok(new
            {
                accessToken = result.Value.accessToken,
                refreshToken = result.Value.refreshToken
            });
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            var result = await authService.RefreshAsync(request.RefreshToken);

            if (result == null)
                return BadRequest("Invalid refresh token");

            return Ok(new
            {
                accessToken = result.Value.accessToken,
                refreshToken = result.Value.refreshToken
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshRequest request)
        {
            var success = await authService.LogoutAsync(request.RefreshToken);

            if (!success)
                return BadRequest("Invalid refresh token");

            return Ok("Logged out successfully");
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!");
        }

    }
}