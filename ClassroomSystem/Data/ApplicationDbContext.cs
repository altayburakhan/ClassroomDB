using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ClassroomSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace ClassroomSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public ApplicationDbContext() { }

        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<Logs> Logs { get; set; }
        public DbSet<AcademicTerm> AcademicTerms { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Reservation configuration
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Classroom)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Term)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TermId)
                .OnDelete(DeleteBehavior.Restrict);

            // Feedback configuration
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Classroom)
                .WithMany(c => c.Feedbacks)
                .HasForeignKey(f => f.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserPreference configuration
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.UserPreference)
                .WithOne(up => up.User)
                .HasForeignKey<UserPreference>(up => up.UserId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Migration sırasında kullanılacak varsayılan connection string
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ClassroomSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }
    }
} 