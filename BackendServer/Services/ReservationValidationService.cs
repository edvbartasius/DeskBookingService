using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services;

public class ReservationValidationService
{
    private readonly AppDbContext _context;
    public ReservationValidationService(AppDbContext context)
    {
        _context = context;
    }
    // Check if time range is withing operating hours
    public bool ValidateTimeRange()
    {
        throw new NotImplementedException();
    }
    // Check for overlapping reservations on the same desk
    public async Task<bool> ValidateNoConflicts(int deskId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeReservationId = null)
    {
        var conflictingSpans = await _context.ReservationTimeSpans
            .Include(ts => ts.Reservation)
            .Where(ts => 
                ts.Reservation!.DeskId == deskId &&
                ts.Reservation.ReservationDate == date &&
                ts.Reservation.Status == Models.ReservationStatus.Active &&
                ts.Status == Models.ReservationStatus.Active &&
                ts.StartTime < endTime &&
                ts.EndTime > startTime &&
                (excludeReservationId == null || ts.ReservationId != excludeReservationId))
            .ToListAsync();
        return !conflictingSpans.Any(); // True, if no conflicts
    }
    
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateOperatingHours(int buildingId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var dayOfWeek = date.DayOfWeek;

        var operatingHours = await _context.OperatingHours
            .FirstOrDefaultAsync(oh => 
                oh.BuildingId == buildingId &&
                oh.DayOfWeek == dayOfWeek);

        // If no operating hours set for the day
        if (operatingHours == null)
        {
            return (false, $"No operating hours configured for {dayOfWeek}");
        }

        // Building is closed for the day
        if (operatingHours.IsClosed)
        {
            return (false, $"Building is closed on {dayOfWeek}");
        }
        // Check if times are withing operating hours
        if (startTime < operatingHours.OpeningTime)
            return (false, $"Start time {startTime} is before opening time {operatingHours.OpeningTime}");
        if (endTime > operatingHours.ClosingTime)
            return (false, $"End time {endTime} is after closing time {operatingHours.ClosingTime}");

        // Handle midnight crossing (overnight operations)
        // Example: Opening = 22:00, Closing = 06:00
        if (operatingHours.ClosingTime < operatingHours.OpeningTime)
        {
            bool afterOpening = startTime >= operatingHours.OpeningTime && endTime >= operatingHours.OpeningTime;
            bool beforeClosing = startTime <= operatingHours.ClosingTime && endTime <= operatingHours.ClosingTime;

            if (!afterOpening && !beforeClosing)
            {
                return (false, "Reservation must be entirely within the overnight operating hours");
            }

            // Check: can't span across midnight in a single reservation
            if (startTime >= operatingHours.OpeningTime && endTime <= operatingHours.ClosingTime)
            {
                return (false, "Reservation cannot span across midnight");
            }
        }

        return (true, null);
    }
}