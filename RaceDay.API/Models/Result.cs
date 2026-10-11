/* ============================================================
   RaceDay - Result Model
   System : RaceDay Event Management System
   Author : SIYETHEMBA XULU
   ST     : 10475058
   Target : ASP.NET Core 8.0 with Entity Framework Core
   Notes  : Matches the Results entity in Part 1 ERD.
            Linked 1:1 with Enrolments (UNIQUE constraint).
   ============================================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public int RecordedByOrganiserID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        public int? Position { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("EnrolmentID")]
        public Enrolment? Enrolment { get; set; }
        [ForeignKey("RecordedByOrganiserID")]
        public Organiser? Organiser { get; set; }
    }
}