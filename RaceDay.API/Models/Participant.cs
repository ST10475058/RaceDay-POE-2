/* ============================================================
   RaceDay - Participant Model
   System : RaceDay Event Management System
   Author : SIYETHEMBA XULU
   ST     : 10475058
   Target : ASP.NET Core 8.0 with Entity Framework Core
   Notes  : Matches the Participants entity in Part 1 ERD.
   ============================================================ */

using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}