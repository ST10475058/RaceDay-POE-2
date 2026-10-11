/* ============================================================
   RaceDay - Category Model
   System : RaceDay Event Management System
   Author : SIYETHEMBA XULU
   ST     : 10475058
   Target : ASP.NET Core 8.0 with Entity Framework Core
   Notes  : Matches the Categories entity in Part 1 ERD.
   ============================================================ */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

        public int MaxParticipants { get; set; } = 100;

        [Column(TypeName = "decimal(8,2)")]
        public decimal Price { get; set; } = 0;

        // Navigation properties
        [ForeignKey("EventID")]
        public Event? Event { get; set; }
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}