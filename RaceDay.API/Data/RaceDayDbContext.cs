/* ============================================================
   RaceDay - Database Context
   System : RaceDay Event Management System
   Author : SIYETHEMBA XULU
   ST     : 10475058
   Target : ASP.NET Core 8.0 with Entity Framework Core
   Notes  : Matches the ERD and SQL script from Part 1.
            Applies all constraints (UNIQUE, CHECK) as
            defined in Part 1.
   ============================================================ */

using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------------------------------
            // UNIQUE Constraints (Part 1)
            // -----------------------------------------------------
            modelBuilder.Entity<Organiser>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantID, e.CategoryID })
                .IsUnique();

            // -----------------------------------------------------
            // CHECK Constraints (Part 1)
            // -----------------------------------------------------
            modelBuilder.Entity<Event>()
                .ToTable(t => t.HasCheckConstraint("CK_Events_EventType",
                    "[EventType] IN ('Running', 'Walking', 'Cycling')"));

            modelBuilder.Entity<Enrolment>()
                .ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status",
                    "[Status] IN ('Confirmed', 'Cancelled')"));

            modelBuilder.Entity<Category>()
                .ToTable(t => t.HasCheckConstraint("CK_Categories_Distance",
                    "[Distance] > 0"));

            modelBuilder.Entity<Category>()
                .ToTable(t => t.HasCheckConstraint("CK_Categories_MaxParticipants",
                    "[MaxParticipants] > 0"));

            modelBuilder.Entity<Result>()
                .ToTable(t => t.HasCheckConstraint("CK_Results_Position",
                    "[Position] IS NULL OR [Position] > 0"));
        }
    }
}