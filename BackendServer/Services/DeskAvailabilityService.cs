using System.Threading.Tasks;
using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services;

public class DeskAvailabilityService
{
    private readonly AppDbContext _context;

    public DeskAvailabilityService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all desks that are available for the specified date range
    /// </summary>
    public async Task<List<Desk>> GetAvailableDesks(int buildingId, DateOnly startDate, DateOnly endDate)
    {
        // Get all desk IDs that have active reservations within the date range
        var bookedDeskIds = await _context.Reservations
            .Where(r =>
                r.Desk!.BuildingId == buildingId &&
                r.ReservationDate >= startDate &&
                r.ReservationDate <= endDate &&
                r.Status == ReservationStatus.Active)
            .Select(r => r.DeskId)
            .Distinct()
            .ToListAsync();

        // Get all desks in the building that are not booked for any day in the range
        var availableDesks = await _context.Desks
            .Where(d =>
                d.BuildingId == buildingId &&
                !bookedDeskIds.Contains(d.Id))
            .ToListAsync();

        return availableDesks;
    }

    /// <summary>
    /// Get all desks available on a specific single date
    /// </summary>
    public async Task<List<Desk>> GetAvailableDesksForDate(int buildingId, DateOnly date)
    {
        var bookedDeskIds = await _context.Reservations
            .Where(r =>
                r.Desk!.BuildingId == buildingId &&
                r.ReservationDate == date &&
                r.Status == ReservationStatus.Active)
            .Select(r => r.DeskId)
            .Distinct()
            .ToListAsync();

        var availableDesks = await _context.Desks
            .Where(d =>
                d.BuildingId == buildingId &&
                !bookedDeskIds.Contains(d.Id))
            .ToListAsync();

        return availableDesks;
    }

    /// <summary>
    /// Calculate the status of a desk for a specific date
    /// </summary>
    // public async Task<DeskStatus> GetDeskStatus(int deskId, DateOnly date)
    // {
    //     // Check if desk exists
    //     var desk = await _context.Desks
    //         .Include(d => d.Building)
    //             .ThenInclude(b => b!.OperatingHours)
    //         .FirstOrDefaultAsync(d => d.Id == deskId);

    //     if (desk == null)
    //     {
    //         return DeskStatus.;
    //     }

    //     // Check if building is open on this day
    //     var operatingHours = desk.Building?.OperatingHours
    //         .FirstOrDefault(oh => oh.DayOfWeek == date.DayOfWeek);

    //     if (operatingHours == null || operatingHours.IsClosed)
    //     {
    //         return DeskStatus.Unavailable;
    //     }

    //     // Check if desk is booked for this date
    //     var isBooked = await _context.Reservations
    //         .AnyAsync(r =>
    //             r.DeskId == deskId &&
    //             r.ReservationDate == date &&
    //             r.Status == ReservationStatus.Active);

    //     return isBooked ? DeskStatus.Booked : DeskStatus.Available;
    // }

    /// <summary>
    /// Get desk statuses for a date range
    /// </summary>
    // public async Task<Dictionary<DateOnly, DeskStatus>> GetDeskStatusForDateRange(int deskId, DateOnly startDate, DateOnly endDate)
    // {
    //     var result = new Dictionary<DateOnly, DeskStatus>();

    //     // Get all reservations for this desk in the date range
    //     var reservations = await _context.Reservations
    //         .Where(r =>
    //             r.DeskId == deskId &&
    //             r.ReservationDate >= startDate &&
    //             r.ReservationDate <= endDate &&
    //             r.Status == ReservationStatus.Active)
    //         .Select(r => r.ReservationDate)
    //         .ToListAsync();

    //     // Get desk with operating hours
    //     var desk = await _context.Desks
    //         .Include(d => d.Building)
    //             .ThenInclude(b => b!.OperatingHours)
    //         .FirstOrDefaultAsync(d => d.Id == deskId);

    //     if (desk == null)
    //     {
    //         return result;
    //     }

    //     // Iterate through each date in range
    //     for (var date = startDate; date <= endDate; date = date.AddDays(1))
    //     {
    //         // Check operating hours
    //         var operatingHours = desk.Building?.OperatingHours
    //             .FirstOrDefault(oh => oh.DayOfWeek == date.DayOfWeek);

    //         if (operatingHours == null || operatingHours.IsClosed)
    //         {
    //             result[date] = DeskStatus.Unavailable;
    //         }
    //         else if (reservations.Contains(date))
    //         {
    //             result[date] = DeskStatus.Booked;
    //         }
    //         else
    //         {
    //             result[date] = DeskStatus.Available;
    //         }
    //     }

    //     return result;
    // }

    // /// <summary>
    // /// Get reservation details for a desk on a specific date (who booked it)
    // /// </summary>
    // public async Task<Reservation?> GetDeskReservation(int deskId, DateOnly date)
    // {
    //     return await _context.Reservations
    //         .Include(r => r.User)
    //         .Include(r => r.Desk)
    //         .FirstOrDefaultAsync(r =>
    //             r.DeskId == deskId &&
    //             r.ReservationDate == date &&
    //             r.Status == ReservationStatus.Active);
    // }
}
