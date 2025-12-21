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

    /// <summary>
    /// Check if a desk is available for the specified date (no conflicting active reservations)
    /// </summary>
    public async Task<bool> IsDeskAvailable(int deskId, DateOnly date, int? excludeReservationId = null)
    {
        var conflictingReservation = await _context.Reservations
            .Where(r =>
                r.DeskId == deskId &&
                r.ReservationDate == date &&
                r.Status == Models.ReservationStatus.Active &&
                (excludeReservationId == null || r.Id != excludeReservationId))
            .AnyAsync();

        return !conflictingReservation;
    }

    /// <summary>
    /// Validate that the date is not in the past
    /// </summary>
    public (bool IsValid, string? ErrorMessage) ValidateDateNotInPast(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (date < today)
        {
            return (false, $"Cannot make reservations for past dates. Date: {date}, Today: {today}");
        }

        return (true, null);
    }

    /// <summary>
    /// Comprehensive validation for creating a reservation
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateReservation(
        int deskId,
        DateOnly date,
        int? excludeReservationId = null)
    {
        // Check date not in past
        var dateValidation = ValidateDateNotInPast(date);
        if (!dateValidation.IsValid)
        {
            return dateValidation;
        }

        // Get desk with building info
        var desk = await _context.Desks
            .Include(d => d.Building)
            .FirstOrDefaultAsync(d => d.Id == deskId);

        if (desk == null)
        {
            return (false, $"Desk with ID {deskId} not found");
        }

        // Check for conflicts
        var isAvailable = await IsDeskAvailable(deskId, date, excludeReservationId);
        if (!isAvailable)
        {
            return (false, $"Desk {deskId} is already reserved for {date}");
        }

        return (true, null);
    }
}
