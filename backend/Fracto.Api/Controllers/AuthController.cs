using Microsoft.AspNetCore.Mvc;
using Fracto.Api.Data;
using Fracto.Api.DTOs;
using Fracto.Api.Models;
using Microsoft.AspNetCore.Identity;
using Fracto.Api.Services;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<AppUser> _hasher = new();
        private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                //return BadRequest("Invalid payload.");
                 return BadRequest(new { message = "Invalid payload." });

            if (_db.Users.Any(u => u.Username == dto.Username || u.Email == dto.Email))
                //return BadRequest("User already exists.");
                 return BadRequest(new { message = "User already exists." });

            var user = new AppUser { Username = dto.Username, Email = dto.Email, ProfileImagePath = "default.png" };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(new { user.Id, user.Username });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _db.Users.SingleOrDefault(u => u.Username == dto.Username);
            if (user == null) return Unauthorized();
            var res = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (res == PasswordVerificationResult.Failed) return Unauthorized();
            var token = _tokenService.CreateToken(user);
            return Ok(new { token, user.Id, user.Username, user.Role });
        }
    }
}
