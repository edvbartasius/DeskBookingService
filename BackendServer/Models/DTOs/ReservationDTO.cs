namespace DeskBookingService.Models.DTOs;

public class ReservationDto
{
    public required string UserId { get; set; }
    public required int DeskId { get; set; }
    public required DateOnly ReservationDate { get; set; }
    public required TimeOnly StartDate { get; set; }
    public required TimeOnly EndDate { get; set; }
}
