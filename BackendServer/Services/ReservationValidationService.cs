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
}
