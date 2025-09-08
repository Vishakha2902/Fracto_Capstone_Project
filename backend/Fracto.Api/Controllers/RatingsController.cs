

using Fracto.Api.Data;
using Fracto.Api.DTOs;                 // CreateRatingDto
using Fracto.Api.Infrastructure;       // GetUserId()
using Fracto.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public RatingsController(ApplicationDbContext db) => _db = db;

        /// <summary>Create a rating for a doctor (logged-in users)</summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRatingDto dto)
        {
            if (dto.Score < 1 || dto.Score > 5)
                return BadRequest("Score must be 1..5.");

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            var doctor = await _db.Doctors.FindAsync(dto.DoctorId);
            if (doctor is null) return NotFound("Doctor not found.");

            _db.Ratings.Add(new Rating
            {
                DoctorId = dto.DoctorId,
                UserId = userId,
                Score = dto.Score
            });
            await _db.SaveChangesAsync();

            // Compute average as DECIMAL to avoid precision / cast issues.
            var avgDec = await _db.Ratings
                .Where(r => r.DoctorId == dto.DoctorId)
                .Select(r => (decimal)r.Score)   // works whether Score is int or decimal
                .AverageAsync();

            var roundedDec = Math.Round(avgDec, 2);

            // --- SAFE ASSIGNMENT to Doctor.Rating no matter its type (decimal/double/float/int, nullable or not)
            var ratingProp = doctor.GetType().GetProperty("Rating");
            if (ratingProp is not null && ratingProp.CanWrite)
            {
                var targetType = Nullable.GetUnderlyingType(ratingProp.PropertyType) ?? ratingProp.PropertyType;
                object value = roundedDec; // default decimal

                if      (targetType == typeof(double)) value = (double)roundedDec;
                else if (targetType == typeof(float))  value = (float)roundedDec;
                else if (targetType == typeof(int))    value = (int)Math.Round(roundedDec, 0);
                // if decimal or anything else, keep the decimal

                ratingProp.SetValue(doctor, value);
                await _db.SaveChangesAsync();
            }

            // Return DECIMAL to the client (no double casting)
            return Ok(new { doctorId = dto.DoctorId, average = roundedDec });
        }

        /// <summary>Get average rating for a doctor</summary>
        [HttpGet("doctor/{doctorId:int}/avg")]
        public async Task<IActionResult> GetAverage(int doctorId)
        {
            var exists = await _db.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!exists) return NotFound();

            var avgDec = await _db.Ratings
                .Where(r => r.DoctorId == doctorId)
                .Select(r => (decimal?)r.Score)
                .AverageAsync() ?? 0m;

            return Ok(new { doctorId, average = Math.Round(avgDec, 2) });
        }

        /// <summary>Get all ratings for a doctor</summary>
        [HttpGet("doctor/{doctorId:int}")]
        public async Task<IActionResult> GetDoctorRatings(int doctorId)
        {
            var list = await _db.Ratings
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.Id)      // no CreatedAt field => sort by Id
                .Select(r => new { id = r.Id, r.Score, r.UserId })
                .ToListAsync();

            return Ok(list);
        }
    }
}
