namespace DeskBookingService.Models.AdminDTOs;

public class AdminReservationDto
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required int DeskId { get; set; }
    public required DateOnly ReservationDate { get; set; }
    public required TimeOnly StartDate { get; set; }
    public required TimeOnly EndDate { get; set; }
}
