namespace DeskBookingService.Models.DTOs;

public class TimeSpanDto
{
    public int Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
}
