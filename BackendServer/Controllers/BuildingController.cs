using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Data.Common;


namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/buildings")]
    public class BuildingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BuildingController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("get-buildings")]
        public async Task<IActionResult> GetBuildings()
        {
            try
            {
                var buildings = await _context.Buildings.ToListAsync();
                var buildingListItemDtos = _mapper.Map<List<BuildingListItemDto>>(buildings);
                return Ok(buildingListItemDtos);
            }
            catch (DbException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }
        [HttpGet("get-floor-plan/{buildingId}/{date}/{userId}")]
        public async Task<IActionResult> GetFloorPlan(int buildingId, DateOnly date, string userId)
        {
            // Since no authentification is used, accept user ID to know which reservations are made by user
            var building = await _context.Buildings
                .Include(b => b.Desks)
                    .ThenInclude( d => d.Reservations.Where(r =>
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
    }
}
