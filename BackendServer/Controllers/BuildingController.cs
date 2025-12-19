using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;


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
            var buildings = await _context.Buildings.ToListAsync();
            var buildingDtos = _mapper.Map<List<BuildingDto>>(buildings);
            return Ok(buildingDtos);
        }
    }
}
