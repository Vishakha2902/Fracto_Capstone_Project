

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fracto.Api.Data;
using Fracto.Api.DTOs;
using Fracto.Api.Models;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AppointmentsController(ApplicationDbContext db) => _db = db;

        // ===========================
        // Book appointment (User/Admin)
        // ===========================
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Book([FromBody] AppointmentDto dto)
        {
            if (dto is null) return BadRequest("Invalid payload.");
            if (string.IsNullOrWhiteSpace(dto.TimeSlot))
                return BadRequest("TimeSlot is required.");

            // Check doctor exists
            var doctor = await _db.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null) return NotFound("Doctor not found.");

            // Prevent double-booking the same doctor/date/timeslot unless the other is Cancelled
            var slotTaken = await _db.Appointments.AnyAsync(a =>
                a.DoctorId == dto.DoctorId &&
                a.AppointmentDate.Date == dto.AppointmentDate.Date &&
                a.TimeSlot == dto.TimeSlot &&
                (a.Status != "Cancelled"));

            if (slotTaken) return BadRequest("This slot is already booked.");

            var appt = new Appointment
            {
                UserId = dto.UserId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate.Date,
                TimeSlot = dto.TimeSlot,
                Status = "Pending" // set to "Booked" if you do not require approval
            };

            _db.Appointments.Add(appt);
            await _db.SaveChangesAsync();

            var spec = doctor.Specialization != null
                ? new { doctor.Specialization.Id, doctor.Specialization.Name }
                : null;

            return Ok(new
            {
                appt.Id,
                appt.AppointmentDate,
                TimeSlot = appt.TimeSlot ?? string.Empty,
                Status = appt.Status ?? "Pending",
                Doctor = new
                {
                    doctor.Id,
                    doctor.Name,
                    doctor.City,
                    Specialization = spec
                }
            });
        }

        // ===========================
        // Cancel appointment (User/Admin)
        // ===========================
        [HttpPut("cancel/{id:int}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Appointment cancelled successfully." });
        }

        // ===================================================
        // Get all appointments for a specific user (User/Admin)
        // ===================================================
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ByUser(int userId)
        {
            var list = await _db.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    TimeSlot = a.TimeSlot ?? string.Empty,
                    Status = a.Status ?? "Pending",
                    Doctor = a.Doctor == null ? null : new
                    {
                        a.Doctor.Id,
                        a.Doctor.Name,
                        a.Doctor.City,
                        Specialization = a.Doctor.Specialization == null ? null : new
                        {
                            a.Doctor.Specialization.Id,
                            a.Doctor.Specialization.Name
                        }
                    }
                })
                .ToListAsync();

            return Ok(list);
        }

        // ===========================
        // -------- Admin-only -------
        // ===========================

        // Get ALL appointments (Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.DoctorId,
                    a.AppointmentDate,
                    TimeSlot = a.TimeSlot ?? string.Empty,
                    Status = a.Status ?? "Pending",
                    Doctor = a.Doctor == null ? null : new
                    {
                        a.Doctor.Id,
                        a.Doctor.Name,
                        a.Doctor.City,
                        Specialization = a.Doctor.Specialization == null ? null : new
                        {
                            a.Doctor.Specialization.Id,
                            a.Doctor.Specialization.Name
                        }
                    }
                })
                .ToListAsync();

            return Ok(list);
        }

        // Get only Pending appointments (Admin)
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var list = await _db.Appointments
                .Where(a => a.Status == "Pending") // change to "Booked" if you don't use approval
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.DoctorId,
                    a.AppointmentDate,
                    TimeSlot = a.TimeSlot ?? string.Empty,
                    Status = a.Status ?? "Pending",
                    Doctor = a.Doctor == null ? null : new
                    {
                        a.Doctor.Id,
                        a.Doctor.Name,
                        a.Doctor.City,
                        Specialization = a.Doctor.Specialization == null ? null : new
                        {
                            a.Doctor.Specialization.Id,
                            a.Doctor.Specialization.Name
                        }
                    }
                })
                .ToListAsync();

            return Ok(list);
        }

        // Approve appointment (Admin)
        [HttpPut("approve/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (string.Equals(appt.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Cannot approve a cancelled appointment.");

            appt.Status = "Approved";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Appointment approved successfully." });
        }
    }
}
