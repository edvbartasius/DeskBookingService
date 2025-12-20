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
            // TODO: Get current user ID from authentication context
            var currentUserId = "user-1"; // Hardcoded for now

            var createdReservations = new List<Reservation>();
            var failedDates = new List<DateOnly>();
            var errorMessages = new List<string>();

            foreach (var date in dto.ReservationDates)
            {
                // Validate each date
                var validation = await _validationService.ValidateReservation(dto.DeskId, date);

                if (!validation.IsValid)
                {
                    failedDates.Add(date);
                    errorMessages.Add($"{date}: {validation.ErrorMessage}");
                    continue;
                }

                // Create reservation
                var reservation = new Reservation
                {
                    UserId = currentUserId,
                    DeskId = dto.DeskId,
                    ReservationDate = date,
                    Status = ReservationStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reservations.Add(reservation);
                createdReservations.Add(reservation);
            }

            if (createdReservations.Any())
            {
                await _context.SaveChangesAsync();
            }

            var result = new CreateReservationResultDto
            {
                Success = createdReservations.Count == dto.ReservationDates.Count,
                ErrorMessage = errorMessages.Any() ? string.Join("; ", errorMessages) : null,
                CreatedReservations = _mapper.Map<List<ReservationDto>>(createdReservations),
                FailedDates = failedDates
            };

            if (result.Success)
            {
                return Ok(result);
            }
            else if (createdReservations.Any())
            {
                return StatusCode(207, result); // 207 Multi-Status (partial success)
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetUserReservations()
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = "user-1"; // Hardcoded for now

            var reservations = await _context.Reservations
                .Include(r => r.Desk)
                    .ThenInclude(d => d!.Building)
                .Where(r => r.UserId == currentUserId && r.Status == ReservationStatus.Active)
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            var reservationDtos = _mapper.Map<List<ReservationDto>>(reservations);
            return Ok(reservationDtos);
        }

        [HttpGet("my-reservations/history")]
        public async Task<IActionResult> GetUserReservationHistory()
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = "user-1"; // Hardcoded for now

            var reservations = await _context.Reservations
                .Include(r => r.Desk)
                    .ThenInclude(d => d!.Building)
                .Where(r =>
                    r.UserId == currentUserId &&
                    (r.Status == ReservationStatus.Completed || r.Status == ReservationStatus.Cancelled))
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            var reservationDtos = _mapper.Map<List<ReservationDto>>(reservations);
            return Ok(reservationDtos);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = "user-1"; // Hardcoded for now

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "Reservation not found" });
            }

            // Verify ownership
            if (reservation.UserId != currentUserId)
            {
                return Forbid();
            }

            // Check if already cancelled
            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return BadRequest(new { message = "Reservation is already cancelled" });
            }

            // Soft delete by setting status to cancelled
            reservation.Status = ReservationStatus.Cancelled;
            reservation.CanceledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation cancelled successfully" });
        }

        [HttpPost("cancel-date-range")]
        public async Task<IActionResult> CancelReservationsForDateRange([FromBody] CancelDateRangeDto dto)
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = "user-1"; // Hardcoded for now

            var reservations = await _context.Reservations
                .Where(r =>
                    r.UserId == currentUserId &&
                    r.DeskId == dto.DeskId &&
                    r.ReservationDate >= dto.StartDate &&
                    r.ReservationDate <= dto.EndDate &&
                    r.Status == ReservationStatus.Active)
                .ToListAsync();

            if (!reservations.Any())
            {
                return NotFound(new { message = "No active reservations found for this date range" });
            }

            foreach (var reservation in reservations)
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.CanceledAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Successfully cancelled {reservations.Count} reservation(s)",
                cancelledCount = reservations.Count
            });
        }
    }

    public class CancelDateRangeDto
    {
        public required int DeskId { get; set; }
        public required DateOnly StartDate { get; set; }
        public required DateOnly EndDate { get; set; }
    }
}
