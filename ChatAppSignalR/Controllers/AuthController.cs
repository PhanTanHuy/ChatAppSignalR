using JWT.DTO;
using JWT.Entities;

using JWTAuth.Services;
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
                user.Email
            }); // ✅ không trả PasswordHash
        }

       
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest request)
        {
            var token = await authService.LoginAsync(request);

            if (token is null)
                return BadRequest("Invalid email or password!");

            return Ok(token);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!");
        }

    }
}

//using JWT.DTO;
//using JWT.Entities;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace JWT.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController(IConfiguration configuration) : ControllerBase
//    {
//        public static User user = new User();

//        [HttpPost("Register")]
//        public ActionResult<User> Register (UserDto request)
//        {
//            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
//            user.Username = request.Username;
//            user.PasswordHash = hashedPassword;

//            return Ok(user);
//        }
//        [HttpPost("Login")]
//        public ActionResult<string> Login (UserDto request)
//        {
//            if(user.Username != request.Username)
//            {
//                return BadRequest("not found user");
//            }
//            if(new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
//            {
//                return BadRequest("wrong password");
//            }
//            string token = CreateToken(user);
//            return Ok(token);

//        }
//        private string CreateToken(User user)
//        {
//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name, user.Username)
//            };

//            var key = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token") ?? ""));

//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(claims),
//                Issuer = configuration.GetValue<string>("AppSettings:Issuer"),
//                Audience = configuration.GetValue<string>("AppSettings:Audience"),
//                Expires = DateTime.UtcNow.AddDays(1),
//                SigningCredentials = creds
//            };

//            var tokenHandler = new JwtSecurityTokenHandler();


//            var token = tokenHandler.CreateToken(tokenDescriptor);          
//            return tokenHandler.WriteToken(token);                           
//        }
//        //private string CreateToken(User user)
//        //{
//        //    var claims = new List<Claim>
//        //    {
//        //        new Claim(ClaimTypes.Name, user.Username)
//        //    };
//        //    //var key = new SymmetricSecurityKey(
//        //    //    Encoding.UTF8.GetBytes(ConfigurationBinder.GetValue<string>("AppSettings:Token"))
//        //    //);
//        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")));

//        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

//        //    var tokenDescriptor = new SecurityTokenDescriptor
//        //    {
//        //        Subject = new ClaimsIdentity(claims),
//        //        Issuer = configuration.GetValue<string>("AppSettings:Issuer"),
//        //        Audience = configuration.GetValue<string>("AppSettings:Audience"),
//        //        Expires = DateTime.UtcNow.AddDays(1),
//        //        SigningCredentials = creds
//        //    };

//        //    return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);


//        //}

//    }
//}
