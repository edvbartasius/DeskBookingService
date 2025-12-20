namespace DeskBookingService.Models;

public class Reservation
{
    public int Id { get; set; }
    public required string UserId { get; set; } // Foreign key
    public required int DeskId { get; set; } // Foreign key
    public required DateOnly ReservationDate { get; set; }

    public ReservationStatus Status { get; set;} = ReservationStatus.Active; // Status for soft deletion

    // Navigation properties
    public User? User { get; set;}
    public Desk? Desk { get; set;}
    public ICollection<ReservationTimeSpan> TimeSpans { get; set;} = new List<ReservationTimeSpan>();

    // Audit fields
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime? CanceledAt { get; set;}
}

