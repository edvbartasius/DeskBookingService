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
}
