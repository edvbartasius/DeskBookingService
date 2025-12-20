namespace DeskBookingService.Models.AdminDTOs;

public class AdminReservationDto
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required int DeskId { get; set; }
    public required DateOnly ReservationDate { get; set; }

    // Status and audit
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CanceledAt { get; set; }

    // Time spans (new multi-timespan support)
    public List<ReservationTimeSpanDto> TimeSpans { get; set; } = new List<ReservationTimeSpanDto>();
}

public class ReservationTimeSpanDto
{
    public int Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
}
