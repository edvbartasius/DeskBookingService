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
            Console.WriteLine("Received CreateReservation for ", dto.ReservationDates.Count, " reservations");
            // Validate user exists
            var user = await _context.Users.FindAsync(dto.UserId.ToString());
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Validate desk exists
            var desk = await _context.Desks.FindAsync(dto.DeskId);
            if (desk == null)
            {
                return BadRequest("Desk not found");
            }

            // Deduplicate dates first to avoid duplicate validation
            var uniqueDates = dto.ReservationDates.Distinct().ToList();

            // Validate all dates before creating any reservations (fail-fast)
            foreach (var date in uniqueDates)
            {
                var validationResult = await _validationService.ValidateReservation(
                    dto.DeskId,
                    date
                );

                if (!validationResult.IsValid)
                {
                    return BadRequest(new {
                        error = validationResult.ErrorMessage,
                        failedDate = date.ToString("yyyy-MM-dd")
                    });
                }
            }

            // Create all reservations at once
            var reservations = uniqueDates.Select(date => new Reservation
                {
                    UserId = dto.UserId.ToString(),
                    DeskId = dto.DeskId,
                    ReservationDate = date,
                    Status = ReservationStatus.Active,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

            try
            {
                // Add all reservations at once and save
                _context.Reservations.AddRange(reservations);
                await _context.SaveChangesAsync();

                return Ok(new {
                    success = true,
                    message = $"Successfully created {reservations.Count} reservation(s)",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to create reservations: " + ex.Message);
            }
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

        [HttpPatch("my-reservations/cancel-single-day/{deskId}/{date}/{userId}")]
        public async Task<IActionResult> CancelReservation(int deskId, DateOnly date, string userId)
        {
            try
            {
                // Verify user (for production authentificate without passing userId in request)
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest("User not found");
                }
                // Find reservation entry
                var reservation = await _context.Reservations
                    .Where(r => r.DeskId == deskId && 
                    r.UserId == userId && 
                    r.ReservationDate == date
                    ).FirstOrDefaultAsync();

                if (reservation == null)
                {
                    return NotFound($"No reservation found for desk: {deskId} on {date}");
                }
                // Soft delete
                reservation.CanceledAt = DateTime.UtcNow;
                reservation.Status = ReservationStatus.Cancelled;

                await _context.SaveChangesAsync();
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to cancel reservation: " + ex.Message);
            }
        }

        [HttpPatch("my-reservations/cancel-date-range/{deskId}/{userId}")]
        public async Task<IActionResult> CancelReservationsForDateRange(int deskId, string userId)
        {
            try
            {
                // Verify user (for production authentificate without passing userId in request)
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest("User not found");
                }
                // Find reservation entry
                var reservations = await _context.Reservations
                    .Where(r => r.DeskId == deskId && 
                    r.UserId == userId
                    ).ToListAsync();

                foreach (var reservation in reservations)
                {
                    reservation.CanceledAt = DateTime.UtcNow;
                    reservation.Status = ReservationStatus.Cancelled;
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to cancel reservation(s): " + ex.Message);
            }
        }
    }
}
