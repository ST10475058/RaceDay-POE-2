namespace RaceDay.API.DTOs
{
    public class EventDto
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public List<CategoryDto> Categories { get; set; } = new();
    }

    public class CreateEventDto
    {
        public string EventName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateEventDto
    {
        public string? EventName { get; set; }
        public string? EventType { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
}