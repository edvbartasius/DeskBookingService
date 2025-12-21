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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteDesk(int id)
        {
            try
            {
                var entry = await _context.Desks.FindAsync(id);
                if (entry == null)
                {
                    return NotFound();
                }
                _context.Desks.Remove(entry);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }
    }
}