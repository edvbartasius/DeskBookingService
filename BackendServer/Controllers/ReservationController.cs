using MapsterMapper;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using DeskBookingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ReservationValidationService _validationService;

        public ReservationController(
            AppDbContext context,
            IMapper mapper,
            ReservationValidationService validationService)
        {
            _context = context;
            _mapper = mapper;
            _validationService = validationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Desk)
                .Where(r => r.Status == ReservationStatus.Active)
                .ToListAsync();

            var reservationDtos = _mapper.Map<List<ReservationDto>>(reservations);
            return Ok(reservationDtos);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation(CreateReservationDTO dto)
        {
            throw new NotImplementedException();
        }

        [HttpGet("desk/{deskId}")]
        public async Task<IActionResult> GetDeskReservations(int deskId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.DeskId == deskId && r.Status == ReservationStatus.Active)
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            // Return only the reservation dates
            var reservationDates = reservations.Select(r => r.ReservationDate).ToList();
            return Ok(reservationDates);
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetUserReservations()
        {
            throw new NotImplementedException();
        }

        [HttpGet("my-reservations/history")]
        public async Task<IActionResult> GetUserReservationHistory()
        {
            throw new NotImplementedException();
        }

        [HttpDelete("my-reservations/{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost("my-reservations/cancel-date-range")]
        public async Task<IActionResult> CancelReservationsForDateRange()
        {
            throw new NotImplementedException();
        }
    }
}
