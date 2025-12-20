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
    /// Check if a desk is available for a date range
    /// </summary>
    public async Task<(bool IsAvailable, List<DateOnly> ConflictingDates)> IsDeskAvailableForDateRange(
        int deskId,
        DateOnly startDate,
        DateOnly endDate,
        int? excludeReservationId = null)
    {
        // Get all existing reservations for this desk in the date range
        var conflictingDates = await _context.Reservations
            .Where(r =>
                r.DeskId == deskId &&
                r.ReservationDate >= startDate &&
                r.ReservationDate <= endDate &&
                r.Status == Models.ReservationStatus.Active &&
                (excludeReservationId == null || r.Id != excludeReservationId))
            .Select(r => r.ReservationDate)
            .ToListAsync();

        return (conflictingDates.Count == 0, conflictingDates);
    }

    /// <summary>
    /// Validate that the reservation date is within operating hours for the building
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateOperatingHours(int buildingId, DateOnly date)
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

        return (true, null);
    }

    /// <summary>
    /// Validate operating hours for a date range
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage, List<DateOnly> ClosedDates)> ValidateOperatingHoursForDateRange(
        int buildingId,
        DateOnly startDate,
        DateOnly endDate)
    {
        var closedDates = new List<DateOnly>();

        // Get all operating hours for this building
        var operatingHoursList = await _context.OperatingHours
            .Where(oh => oh.BuildingId == buildingId)
            .ToListAsync();

        // Check each date in the range
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;
            var operatingHours = operatingHoursList.FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

            if (operatingHours == null)
            {
                return (false, $"No operating hours configured for {dayOfWeek}", closedDates);
            }

            if (operatingHours.IsClosed)
            {
                closedDates.Add(date);
            }
        }

        if (closedDates.Any())
        {
            return (false, $"Building is closed on {closedDates.Count} day(s) in the selected range", closedDates);
        }

        return (true, null, closedDates);
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

        // Validate operating hours
        var operatingHoursValidation = await ValidateOperatingHours(desk.BuildingId, date);
        if (!operatingHoursValidation.IsValid)
        {
            return operatingHoursValidation;
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
