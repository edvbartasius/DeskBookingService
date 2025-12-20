namespace DeskBookingService.Models.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required int DeskId { get; set; }
    public required DateOnly ReservationDate { get; set; }

    // Status and audit
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CanceledAt { get; set; }

    // Time spans (supports multiple time periods per day)
    public List<TimeSpanDto> TimeSpans { get; set; } = new List<TimeSpanDto>();
}
