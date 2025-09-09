
using Fracto.Api.Models;
using System;
using System.Linq;

namespace Fracto.Api.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Specializations.Any())
            {
                return;   
            }

            var specializations = new Specialization[]
            {
                new Specialization { Name = "Cardiology" },
                new Specialization { Name = "Dermatology" },
                new Specialization { Name = "Pediatrics" },
                new Specialization { Name = "Neurology" }
            };
            context.Specializations.AddRange(specializations);
            context.SaveChanges();

            if (context.Doctors.Any())
            {
                return;
            }

            // Seed Doctors (and link them to specializations)
            var doctors = new Doctor[]
            {
                new Doctor {
                    Name = "Dr. Jane Smith",
                    City = "New York",
                    SpecializationId = specializations.Single(s => s.Name == "Cardiology").Id,
                    Rating = 4.5m,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    SlotDurationMinutes = 30
                },
                new Doctor {
                    Name = "Dr. John Doe",
                    City = "New York",
                    SpecializationId = specializations.Single(s => s.Name == "Dermatology").Id,
                    Rating = 4.0m,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    SlotDurationMinutes = 30
                },
                new Doctor {
                    Name = "Dr. Emily White",
                    City = "New York",
                    SpecializationId = specializations.Single(s => s.Name == "Pediatrics").Id,
                    Rating = 5.0m,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    SlotDurationMinutes = 30
                },
                new Doctor {
                    Name = "Dr. Michael Green",
                    City = "New York",
                    SpecializationId = specializations.Single(s => s.Name == "Neurology").Id,
                    Rating = 3.8m,
                    StartTime = new TimeSpan(11, 0, 0),
                    EndTime = new TimeSpan(19, 0, 0),
                    SlotDurationMinutes = 30
                }
            };
            context.Doctors.AddRange(doctors);
            context.SaveChanges();
        }
    }
}
