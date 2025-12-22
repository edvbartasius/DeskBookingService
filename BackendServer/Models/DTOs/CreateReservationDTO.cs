using System.Text.Json.Serialization;

namespace DeskBookingService.Models.DTOs;

public class CreateReservationDTO
{
    public required string UserId { get; set; }
    public required int DeskId { get; set; }
    public List<DateOnly>? ReservationDates { get; set; }
    public DateOnly? ReservationDate { get; set; } // To fix DB insert reservation
    public List<DateOnly> GetEffectiveDates()
    {
        // If single date is provided, use it (takes priority)
        if (ReservationDate.HasValue)
        {
            return new List<DateOnly> { ReservationDate.Value };
        }

        // Otherwise use the list (or empty if null)
        return ReservationDates ?? new List<DateOnly>();
    }
}
