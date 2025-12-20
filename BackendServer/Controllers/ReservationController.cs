using MapsterMapper;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReservationController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("get-reservations")]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _context.Reservations.ToListAsync();
            var reservationDtos = _mapper.Map<List<ReservationDto>>(reservations);
            return Ok(reservationDtos);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation(CreateReservationDTO dto)
        {
            // Validate using ReservationValidationService
            // Create reservation
            // Return ReservationDetailsDto
            throw new NotImplementedException();
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetUserReservations() // Retrieve user's reservations
        {
            // Get reservations for current user
            // Return reservations with desk and building info
            throw new NotImplementedException();            
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {   
            // Verify ownership
            // Set status to cancelled
            throw new NotImplementedException();
        }

        // [HttpPost("create-multiple")]
        // public async Task<IActionResult> CreateMultipleReservations(CreateMultipleReservationsDTO dto)
        // {
        //     // Accept array of time spans for same desk/day
        //     // Validate each reservation & create records
        //     // Return created reservations
        //     throw new NotImplementedException();
        // }
    }
}