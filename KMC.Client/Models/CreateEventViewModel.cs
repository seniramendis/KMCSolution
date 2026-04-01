public class CreateEventViewModel
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime Date { get => EventDate; set => EventDate = value; }
    public required string Location { get; set; }
    public int Capacity { get; set; }
    public int MaxAttendees { get => Capacity; set => Capacity = value; }
    public string? ImageUrl { get; set; }
}