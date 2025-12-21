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
    /// Validate booking size limit (max reservations per booking request)
    /// </summary>
    public (bool IsValid, string? ErrorMessage) ValidateBookingSize(int reservationCount, int maxPerBooking = 7)
    {
        if (reservationCount > maxPerBooking)
        {
            return (false, $"Cannot create more than {maxPerBooking} reservations in a single booking. You requested {reservationCount} reservations.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validate user's total active reservations limit
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateUserActiveReservationsLimit(
        string userId,
        int additionalReservations,
        int maxActiveReservations = 30)
    {
        var currentActiveCount = await _context.Reservations
            .Where(r => r.UserId == userId && r.Status == Models.ReservationStatus.Active)
            .CountAsync();

        var totalAfterBooking = currentActiveCount + additionalReservations;

        if (totalAfterBooking > maxActiveReservations)
        {
            return (false, $"Cannot create reservation. You currently have {currentActiveCount} active reservations. " +
                          $"Adding {additionalReservations} more would exceed the maximum limit of {maxActiveReservations} active reservations.");
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
