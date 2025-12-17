namespace DeskBookingService.Models;

public class Reservation
{
    public int Id { get; set; }
    public required string UserId { get; set; } // Foreign key
    public required int DeskId { get; set; } // Foreign key
    public required DateOnly ReservationDate { get; set; }
    public required TimeOnly StartDate { get; set; }
    public required TimeOnly EndDate { get; set; }
}

