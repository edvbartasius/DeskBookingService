using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using DeskBookingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Data.Common;
using FluentValidation;


namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/buildings")]
    public class BuildingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<Building> _validator;

        public BuildingController(AppDbContext context, IMapper mapper, IValidator<Building> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        [HttpGet("get-buildings")]
        public async Task<IActionResult> GetBuildings()
        {
            try
            {
                var buildings = await _context.Buildings.ToListAsync();
                var buildingDtos = _mapper.Map<List<BuildingDto>>(buildings);
                return Ok(buildingDtos);
            }
            catch (DbException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            try
            {
                var entry = await _context.Buildings.FindAsync(id);
                if (entry == null)
                {
                    return NotFound();
                }
                _context.Buildings.Remove(entry);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddBuilding([FromBody] BuildingDto buildingDto)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(buildingDto));
            try
            {
                var building = _mapper.Map<Building>(buildingDto);

                // Validate using FluentValidation
                var validationResult = await _validator.ValidateAsync(building);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
                }

                _context.Buildings.Add(building);
                await _context.SaveChangesAsync();

                return Ok(building);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, [FromBody] BuildingDto buildingDto)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(buildingDto));
            try
            {
                var building = await _context.Buildings.FindAsync(id);
                if (building == null)
                {
                    return NotFound($"Building with ID: {id}, not found");
                }

                _mapper.Map(buildingDto, building);

                // Validate using FluentValidation
                var validationResult = await _validator.ValidateAsync(building);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
                }

                _context.Buildings.Update(building);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpGet("get-floor-plan/{buildingId}/{date}/{userId}")]
        public async Task<IActionResult> GetFloorPlan(int buildingId, DateOnly date, string userId)
        {
            try
            {
                // Since no authentification is used, accept user ID to know which reservations are made by user
                var building = await _context.Buildings
                    .Include(b => b.Desks)
                        .ThenInclude(d => d.Reservations.Where(r =>
                            r.ReservationDate == date &&
                            r.Status == ReservationStatus.Active))
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(b => b.Id == buildingId);
                if (building == null)
                {
                    return NotFound($"Building with ID: {buildingId} not found");
                }

                var floorPlanDto = new FloorPlanDto
                {
                    buildingName = building.Name,
                    FloorPlanWidth = building.FloorPlanWidth,
                    FloorPlanHeight = building.FloorPlanHeight,
                    FloorPlanDesks = building.Desks.Select(desk =>
                    {
                        var activeReservation = desk.Reservations.FirstOrDefault();

                        DeskStatus status;
                        if (desk.IsInMaintenance)
                        {
                            status = DeskStatus.Maintenance;
                        }
                        else if (activeReservation != null)
                        {
                            status = DeskStatus.Reserved;
                        }
                        else
                        {
                            status = DeskStatus.Open;
                        }

                        return new DeskDto
                        {
                            Id = desk.Id,
                            Description = desk.Description,
                            BuildingId = desk.BuildingId,
                            PositionX = desk.PositionX,
                            PositionY = desk.PositionY,
                            Type = desk.Type,
                            Status = status,
                            IsReservedByCaller = activeReservation?.UserId == userId,
                            ReservedByFullName = activeReservation?.User != null
                                ? $"{activeReservation.User.Name} {activeReservation.User.Surname}"
                                : null,
                            IsInMaintenance = desk.IsInMaintenance,
                            MaintenanceReason = desk.MaintenanceReason
                        };
                    }).ToList()
                };

                return Ok(floorPlanDto);
            }
            catch (DbException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }

        }

        [HttpGet("closed-dates/{buildingId}")]
        public async Task<IActionResult> GetBuildingClosedDate(int buildingId)
        {
            try
            {
                // Select building to get closed days from
                var building = await _context.Buildings
                    .Include(b => b.OperatingHours)
                    .FirstOrDefaultAsync(b => b.Id == buildingId);

                if (building == null)
                {
                    return NotFound($"Building with ID: {buildingId} not found");
                }
                var closedDates = building.OperatingHours
                    .Where(oh => oh.IsClosed)
                    .Select(oh => oh.DayOfWeek)
                    .ToList();
                
                return Ok(closedDates);
            } catch (DbException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }
    }
}
