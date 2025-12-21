namespace DeskBookingService.Models;

public class DeskTimeSpan
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeSpanType Type { get; set; }
    // public string? Reason { get; set; }
    public int? ReservationId { get; set; } // Only for booked spans
}