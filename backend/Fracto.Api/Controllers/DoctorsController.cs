

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fracto.Api.Data;
using Fracto.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DoctorsController(ApplicationDbContext db) => _db = db;

        //  GET all doctors with filters
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? city, [FromQuery] int? specializationId, [FromQuery] decimal? minRating)
        {
            var q = _db.Doctors.Include(d => d.Specialization).AsQueryable();
            if (!string.IsNullOrWhiteSpace(city)) q = q.Where(d => d.City == city);
            if (specializationId.HasValue) q = q.Where(d => d.SpecializationId == specializationId.Value);
            if (minRating.HasValue) q = q.Where(d => d.Rating >= minRating.Value);
            var list = await q.ToListAsync();
            return Ok(list);
        }

        //  GET doctor by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var doc = await _db.Doctors.Include(d => d.Specialization).SingleOrDefaultAsync(d => d.Id == id);
            if (doc == null) return NotFound();
            return Ok(doc);
        }

        //GET available time slots for a doctor
        [HttpGet("{id}/timeslots")]
        public async Task<IActionResult> Timeslots(int id, [FromQuery] DateTime date)
        {
            var doctor = await _db.Doctors.SingleOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();

            var slots = new List<string>();
            var start = doctor.StartTime;
            var end = doctor.EndTime;
            var slotDuration = TimeSpan.FromMinutes(doctor.SlotDurationMinutes);
            var current = start;
            while (current + slotDuration <= end)
            {
                slots.Add($"{current:hh\\:mm}-{(current + slotDuration):hh\\:mm}");
                current += slotDuration;
            }

            var booked = await _db.Appointments
                .Where(a => a.DoctorId == id && a.AppointmentDate.Date == date.Date && a.Status == "Booked")
                .Select(a => a.TimeSlot)
                .ToListAsync();

            var available = slots.Except(booked).ToList();
            return Ok(available);
        }

        //  POST - Create new doctor (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            if (doctor == null) return BadRequest("Invalid doctor data.");

            _db.Doctors.Add(doctor);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = doctor.Id }, doctor);
        }

        // PUT - Update doctor (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] Doctor doctor)
        {
            var existing = await _db.Doctors.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = doctor.Name;
            existing.City = doctor.City;
            existing.Rating = doctor.Rating;
            existing.StartTime = doctor.StartTime;
            existing.EndTime = doctor.EndTime;
            existing.SlotDurationMinutes = doctor.SlotDurationMinutes;
            existing.ProfileImagePath = doctor.ProfileImagePath;
            existing.SpecializationId = doctor.SpecializationId;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        //  DELETE - Remove doctor (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            _db.Doctors.Remove(doctor);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Doctor deleted successfully." });
        }
    }
}
