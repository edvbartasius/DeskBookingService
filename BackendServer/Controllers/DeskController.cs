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

        [HttpGet("unavailability")]
        public async Task<IActionResult> GetUnavailableTimeSpans(int deskId, DateOnly date)
        {
            try
            {
                var timeSpans = await _deskAvailabilityService.GetDeskBookedTimeSpans(deskId, date);
                return Ok(timeSpans);
            } catch (DbException ex)
            {
                return StatusCode(500,  "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpGet("available-desks")]
        public async Task<IActionResult> GetAvailableDesks(int buildingId, int floorNumber, DateOnly date, TimeOnly start, TimeOnly end)
        {
            throw new NotImplementedException();
        }
    }
}