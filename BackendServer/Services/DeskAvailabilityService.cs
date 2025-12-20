using System.Threading.Tasks;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services;

public class DeskAvailabilityService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    public DeskAvailabilityService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<List<Desk>> GetAvailableDesks(int buildingId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var bookedDeskIds = await _context.ReservationTimeSpans
            .Include(ts => ts.Reservation)
            .Where(ts =>
                ts.Reservation!.Desk!.BuildingId == buildingId &&
                ts.Reservation.ReservationDate == date &&
                ts.Reservation.Status == ReservationStatus.Active &&
                ts.Status == ReservationStatus.Active &&
                ts.StartTime < endTime &&
                ts.EndTime > startTime)
            .Select(ts => ts.Reservation!.DeskId)
            .Distinct()
            .ToListAsync();

        // Get remaining desks that are not booked
        var availableDesks = await _context.Desks.Where(d =>
            d.BuildingId == buildingId &&
            !bookedDeskIds.Contains(d.Id))
        .ToListAsync();

        return availableDesks;
    }

    // Returns all booked time spans for a desk on a specific date
    public async Task<List<TimeSpanDto>> GetDeskBookedTimeSpans(int deskId, DateOnly date)
    {
        // Get all booked time spans for this desk on this date (ordered by start time)
        var bookedSpans = await _context.ReservationTimeSpans
            .Include(ts => ts.Reservation)
            .Where(ts =>
                ts.Reservation!.DeskId == deskId &&
                ts.Reservation.ReservationDate == date &&
                ts.Reservation.Status == ReservationStatus.Active &&
                ts.Status == ReservationStatus.Active)
            .OrderBy(ts => ts.StartTime)
            .ToListAsync();
        return _mapper.Map<List<TimeSpanDto>>(bookedSpans);
    }

    // public async Task<List<DeskTimeSpan>> GetAllDeskTimeSpans (int deskId, DateOnly date)
    // {
    //     // Get the desk
    //     var desk = await _context.Desks
    //         .Include(d => d.Building)
    //         .FirstOrDefaultAsync(d => d.Id == deskId);
        
    //     if (desk == null)
    //         throw new ArgumentException($"Desk with ID {deskId} not found");

    //     // Find operating hours
    //     var openingTime = GetOperatingHoursByDate(desk.BuildingId, date);

    //     // Get all booked time spans
    //     var bookedSpans = GetDeskBookedTimeSpans(deskId, date);

    //     // Get closed time spans
    //     var closedSpans = await _context.Clo

    // }

    private async Task<OperatingHours> GetOperatingHoursByDate(int buildingId, DateOnly date)
    {
        var dayOfWeek = date.DayOfWeek;
        var operatingHours = await _context.OperatingHours
            .FirstOrDefaultAsync(oh =>
                oh.BuildingId == buildingId &&
                oh.DayOfWeek == dayOfWeek);

        if (operatingHours == null)
        {
            throw new ArgumentException($"No operating hours found for building: {buildingId} in {date}");
        }
        return operatingHours;
    }
    // Calculates the status of a desk at a specific moment in time
    public async Task<DeskStatus> CalculateDeskStatus(int deskId, DateOnly date, TimeOnly time)
    {
        // Get desk with building and operating hours
        var desk = await _context.Desks
            .Include(d => d.Building)
                .ThenInclude(b => b!.OperatingHours)
            .FirstOrDefaultAsync(d => d.Id == deskId);

        if (desk == null)
        {
            return DeskStatus.Unavailable;
        }

        var operatingHours = desk.Building?.OperatingHours
            .FirstOrDefault(oh => oh.DayOfWeek == date.DayOfWeek);

        // Check if within operating hours
        if (operatingHours == null || operatingHours.IsClosed ||
            time < operatingHours.OpeningTime || time >= operatingHours.ClosingTime)
        {
            return DeskStatus.Unavailable;
        }

        // Check if desk is booked at this specific time
        var isBooked = await _context.ReservationTimeSpans
            .Include(ts => ts.Reservation)
            .AnyAsync(ts =>
                ts.Reservation!.DeskId == deskId &&
                ts.Reservation.ReservationDate == date &&
                ts.Reservation.Status == ReservationStatus.Active &&
                ts.Status == ReservationStatus.Active &&
                ts.StartTime <= time &&
                ts.EndTime > time);

        return isBooked ? DeskStatus.Booked : DeskStatus.Available;
    }
}