using Microsoft.EntityFrameworkCore;
using Fracto.Api.Models;

namespace Fracto.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) {}

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Rating> Ratings => Set<Rating>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Doctor>()
                .Property(d => d.ProfileImagePath)
                .HasDefaultValue("default.png")
                .IsRequired();

            // seed some initial data for specializations and doctors will be seeded in SeedData class
        }
    }
}
