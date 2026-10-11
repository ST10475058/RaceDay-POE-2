/* ============================================================
   RaceDay - Enrolment Model
   System : RaceDay Event Management System
   Author : SIYETHEMBA XULU
   ST     : 10475058
   Target : ASP.NET Core 8.0 with Entity Framework Core
   Notes  : Matches the Enrolments entity in Part 1 ERD.
            Resolves the Participants <-> Categories
            many-to-many relationship.
   ============================================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Confirmed"; // Confirmed / Cancelled

        // Navigation properties
        [ForeignKey("ParticipantID")]
        public Participant? Participant { get; set; }
        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }
        public Result? Result { get; set; }
    }
}