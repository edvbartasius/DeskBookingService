namespace DeskBookingService.Models.DTOs;

public class CreateReservationDTO
{
    public required string UserId { get; set;}
    public required int DeskId { get; set; }
    public required List<DateOnly> ReservationDates { get; set; }
}
