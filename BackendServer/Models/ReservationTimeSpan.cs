namespace DeskBookingService.Models;

public class ReservationTimeSpan
{
    public required int Id {get; set;}
    public required int ReservationId { get; set;} // FK to reservation table
    public required TimeOnly StartTime { get; set;}
    public required TimeOnly EndTime { get; set;}
    // Status for individual timespan cancellation
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public Reservation? Reservation { get; set; } // Navigation property back to parent
    
    // Audit field
    public DateTime? CancelledAt { get; set; }
}