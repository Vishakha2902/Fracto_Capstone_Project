using Microsoft.AspNetCore.Mvc;
using Fracto.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        public UsersController(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();
            return Ok(u);
        }

        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile file)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            user.ProfileImagePath = fileName;
            await _db.SaveChangesAsync();
            return Ok(new { path = fileName });
        }
    }
}
