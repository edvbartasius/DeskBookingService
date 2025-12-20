namespace DeskBookingService.Models;
public class OperatingHours // No support for holidays or other closing reasons
{
    public int Id { get; set; }
    public required int BuildingId { get; set; } // FK to building table
    public DayOfWeek DayOfWeek { get; set; }  // 0=Sunday, 1=Monday, etc.
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public bool IsClosed { get; set; }  // For days when building is closed entirely
    public Building? Building { get; set; }
}