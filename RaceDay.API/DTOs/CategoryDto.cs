namespace RaceDay.API.DTOs
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int MaxParticipants { get; set; }
        public decimal Price { get; set; }
        public int CurrentEnrolments { get; set; }
    }

    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int MaxParticipants { get; set; } = 100;
        public decimal Price { get; set; } = 0;
    }

    public class UpdateCategoryDto
    {
        public string? CategoryName { get; set; }
        public decimal? Distance { get; set; }
        public int? MaxParticipants { get; set; }
        public decimal? Price { get; set; }
    }
}