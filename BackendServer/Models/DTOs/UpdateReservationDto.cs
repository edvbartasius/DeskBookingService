namespace DeskBookingService.Models.DTOs;

public class UpdateReservationDto
{
    public required string UserId { get; set; }
    public required int DeskId { get; set; }
    public required DateOnly ReservationDate { get; set; }
    public ReservationStatus Status { get; set; }
}