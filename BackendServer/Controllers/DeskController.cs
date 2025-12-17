using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;


namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/desks")]
    public class DeskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DeskController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("GetDesks")]
        public async Task<IActionResult> GetDesks()
        {
            var desks = await _context.Desks.ToListAsync();
            var deskDtos = _mapper.Map<List<DeskDto>>(desks);
            return Ok(deskDtos);
        }
    }
}