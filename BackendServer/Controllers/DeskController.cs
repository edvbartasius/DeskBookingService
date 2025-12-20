using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Data.Common;
using DeskBookingService.Services;
using System.ComponentModel.DataAnnotations;


namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/desks")]
    public class DeskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly DeskAvailabilityService _deskAvailabilityService;

        public DeskController(AppDbContext context, IMapper mapper, DeskAvailabilityService deskAvailabilityService)
        {
            _context = context;
            _mapper = mapper;
            _deskAvailabilityService = deskAvailabilityService;
        }

        [HttpGet("get-desks")]
        public async Task<IActionResult> GetDesks()
        {
            var desks = await _context.Desks.ToListAsync();
            var deskDtos = _mapper.Map<List<DeskDto>>(desks);
            return Ok(deskDtos);
        }

        [HttpGet("floor-plan")]
        public async Task<IActionResult> GetFloorPlan(int buildingId)
        {
            try
            {
                var building = await _context.Buildings
                    .Include(b => b.Desks.Where(d => d.BuildingId == buildingId))
                    .Where(b =>
                    b.Id == buildingId)
                    .FirstOrDefaultAsync();
                if (building == null)
                {
                    return BadRequest("No building found.");
                }
                Console.WriteLine($"Returned building (id:{buildingId}): {building}");
                return Ok(_mapper.Map<FloorPlanDto>(building));
            }
            catch (DbException ex)
            {
                return StatusCode(500,  "An error occurred while processing your request: " + ex.Message);
            }
        }

        // [HttpGet("availability")]
        // public async Task<IActionResult> GetAvailability(int deskId, DateOnly date)
        // {
        //     throw new NotImplementedException();
        // }

        /// <summary>
        /// Get desk availability for a date range
        /// </summary>
        [HttpGet("{id}/availability")]
        public async Task<IActionResult> GetDeskAvailability(
            int id,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            var desk = await _context.Desks.FindAsync(id);
            if (desk == null)
            {
                return NotFound();
            }

            var statusByDate = await _deskAvailabilityService.GetDeskStatusForDateRange(id, startDate, endDate);

            var result = new DeskAvailabilityDto
            {
                DeskId = desk.Id,
                Description = desk.Description,
                PositionX = desk.PositionX,
                PositionY = desk.PositionY,
                Type = desk.Type,
                StatusByDate = new List<DeskStatusDto>()
            };

            foreach (var kvp in statusByDate)
            {
                var statusDto = new DeskStatusDto
                {
                    DeskId = id,
                    Date = kvp.Key,
                    Status = kvp.Value
                };

                // If booked, get reserver info
                if (kvp.Value == DeskStatus.Booked)
                {
                    var reservation = await _deskAvailabilityService.GetDeskReservation(id, kvp.Key);
                    if (reservation != null && reservation.User != null)
                    {
                        statusDto.ReservedByFirstName = reservation.User.Name;
                        statusDto.ReservedByLastName = reservation.User.Surname;
                        statusDto.ReservedByUserId = reservation.User.Id;
                    }
                }

                result.StatusByDate.Add(statusDto);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all desks with their availability for a specific date
        /// </summary>
        [HttpGet("availability/by-date")]
        public async Task<IActionResult> GetDesksAvailabilityForDate(
            [FromQuery] int buildingId,
            [FromQuery] DateOnly date)
        {
            var desks = await _context.Desks
                .Where(d => d.BuildingId == buildingId)
                .ToListAsync();

            var result = new List<DeskAvailabilityDto>();

            foreach (var desk in desks)
            {
                var status = await _deskAvailabilityService.GetDeskStatus(desk.Id, date);
                var statusDto = new DeskStatusDto
                {
                    DeskId = desk.Id,
                    Date = date,
                    Status = status
                };

                // If booked, get reserver info
                if (status == DeskStatus.Booked)
                {
                    var reservation = await _deskAvailabilityService.GetDeskReservation(desk.Id, date);
                    if (reservation != null && reservation.User != null)
                    {
                        statusDto.ReservedByFirstName = reservation.User.Name;
                        statusDto.ReservedByLastName = reservation.User.Surname;
                        statusDto.ReservedByUserId = reservation.User.Id;
                    }
                }

                result.Add(new DeskAvailabilityDto
                {
                    DeskId = desk.Id,
                    Description = desk.Description,
                    PositionX = desk.PositionX,
                    PositionY = desk.PositionY,
                    Type = desk.Type,
                    StatusByDate = new List<DeskStatusDto> { statusDto }
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all desks available for a date range (fully available for all dates)
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableDesks(
            [FromQuery] int buildingId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            var availableDesks = await _deskAvailabilityService.GetAvailableDesks(buildingId, startDate, endDate);
            var deskDtos = _mapper.Map<List<DeskDto>>(availableDesks);
            return Ok(deskDtos);
        }
    }
}