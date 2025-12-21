using MapsterMapper;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using DeskBookingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ReservationValidationService _validationService;
        private readonly IValidator<Reservation> _validator;

        public ReservationController(
            AppDbContext context,
            IMapper mapper,
            ReservationValidationService validationService,
            IValidator<Reservation> validator)
        {
            _context = context;
            _mapper = mapper;
            _validationService = validationService;
            _validator = validator;
        }

        /// <summary>
        /// Retrieves all reservations from the database with user and desk information
        /// </summary>
        /// <returns>List of reservation DTOs</returns>
        [HttpGet]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Desk)
                .ToListAsync();

            var reservationDtos = _mapper.Map<List<ReservationDto>>(reservations);
            return Ok(reservationDtos);
        }

        /// <summary>
        /// Deletes a reservation from the database by ID
        /// </summary>
        /// <param name="id">Reservation ID to delete</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entry = await _context.Reservations.FindAsync(id);
                if (entry == null)
                {
                    return NotFound();
                }
                _context.Reservations.Remove(entry);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing reservation's information
        /// </summary>
        /// <param name="id">Reservation ID to update</param>
        /// <param name="reservationDto">Updated reservation data</param>
        /// <returns>Success or error message</returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto reservationDto)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(reservationDto));
            try
            {
                var reservation = await _context.Reservations.FindAsync(id);
                if (reservation == null)
                {
                    return NotFound($"Reservation with ID: {id}, not found");
                }

                _mapper.Map(reservationDto, reservation);

                // Validate using FluentValidation
                var validationResult = await _validator.ValidateAsync(reservation);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
                }

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates one or more reservations for a desk on specified dates
        /// </summary>
        /// <param name="dto">Reservation creation data including user ID, desk ID, and dates</param>
        /// <returns>Success message with count of created reservations or error message</returns>
        [HttpPost("add")]
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

            // Validate booking size limit (max 7 reservations per booking)
            var bookingSizeValidation = _validationService.ValidateBookingSize(uniqueDates.Count);
            if (!bookingSizeValidation.IsValid)
            {
                return BadRequest(new { error = bookingSizeValidation.ErrorMessage });
            }

            // Validate user's active reservations limit (max 30 active reservations)
            var activeReservationsValidation = await _validationService.ValidateUserActiveReservationsLimit(
                dto.UserId.ToString(),
                uniqueDates.Count
            );
            if (!activeReservationsValidation.IsValid)
            {
                return BadRequest(new { error = activeReservationsValidation.ErrorMessage });
            }

            // Validate all dates before creating any reservations (fail-fast)
            foreach (var date in uniqueDates)
            {
                var validationResult = await _validationService.ValidateReservation(
                    dto.DeskId,
                    date
                );

                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        error = validationResult.ErrorMessage,
                        failedDate = date.ToString("yyyy-MM-dd")
                    });
                }
            }

            // Generate a unique booking group ID for this reservation batch
            var ReservationGroupId = Guid.NewGuid();

            // Create all reservations at once
            var reservations = uniqueDates.Select(date => new Reservation
            {
                UserId = dto.UserId.ToString(),
                DeskId = dto.DeskId,
                ReservationDate = date,
                Status = ReservationStatus.Active,
                ReservationGroupId = ReservationGroupId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            try
            {
                // Add all reservations at once and save
                _context.Reservations.AddRange(reservations);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Successfully created {reservations.Count} reservation(s)",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to create reservations: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all active reservation dates for a specific desk
        /// </summary>
        /// <param name="deskId">Desk ID</param>
        /// <returns>List of dates when the desk is reserved</returns>
        [HttpGet("desk/{deskId}")]
        public async Task<IActionResult> GetDeskReservations(int deskId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.DeskId == deskId && r.Status == ReservationStatus.Active)
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            // Return only the reservation dates
            var reservationDates = reservations.Select(r => r.ReservationDate).ToList();
            // Log reservation dates to the console
            Console.WriteLine($"Desk {deskId} reservation dates:");
            foreach (var date in reservationDates)
            {
                Console.WriteLine(date.ToString("yyyy-MM-dd"));
            }
            return Ok(reservationDates);
        }

        /// <summary>
        /// Retrieves all active reservations for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's active reservation DTOs with desk and building information</returns>
        [HttpGet("my-reservations/{userId}")]
        public async Task<IActionResult> GetUserReservations(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId && r.Status == ReservationStatus.Active)
                .Include(r => r.Desk)
                    .ThenInclude(d => d!.Building)
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();

            var reservationDtos = _mapper.Map<List<ReservationDto>>(reservations);
            return Ok(reservationDtos);
        }

        /// <summary>
        /// Retrieves user's upcoming reservations grouped by booking group ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of grouped reservations with metadata (dates, desk info, days until first reservation)</returns>
        [HttpGet("my-reservations/grouped/{userId}")]
        public async Task<IActionResult> GetUserReservationsGrouped(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId &&
                           r.Status == ReservationStatus.Active &&
                           r.ReservationDate >= today) // Only upcoming reservations
                .Include(r => r.Desk)
                    .ThenInclude(d => d!.Building)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Group by ReservationGroupId
            var groupedReservations = reservations
                .GroupBy(r => r.ReservationGroupId)
                .Select(g => new
                {
                    ReservationGroupId = g.Key,
                    createdAt = g.First().CreatedAt,
                    deskId = g.First().DeskId,
                    deskDescription = g.First().Desk?.Description,
                    buildingName = g.First().Desk?.Building?.Name,
                    reservationCount = g.Count(),
                    dates = g.OrderBy(r => r.ReservationDate)
                             .Select(r => r.ReservationDate)
                             .ToList(),
                    reservations = _mapper.Map<List<ReservationDto>>(g.OrderBy(r => r.ReservationDate).ToList()),
                    // Business logic: Check if any date is today
                    hasToday = g.Any(r => r.ReservationDate == today),
                    // Business logic: Days until first reservation
                    daysUntilFirst = g.Min(r => r.ReservationDate).DayNumber - today.DayNumber
                })
                .OrderBy(g => g.daysUntilFirst) // Sort by nearest first
                .ToList();

            return Ok(groupedReservations);
        }

        /// <summary>
        /// Retrieves user's reservation history (cancelled and past reservations)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of past and cancelled reservation DTOs</returns>
        [HttpGet("my-reservations/history/{userId}")]
        public async Task<IActionResult> GetUserReservationHistory(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId &&
                    (r.Status == ReservationStatus.Cancelled || r.ReservationDate < today))
                .Include(r => r.Desk)
                    .ThenInclude(d => d!.Building)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            // Map to DTOs and add business logic for effective status
            var reservationDtos = reservations.Select(r =>
            {
                var dto = _mapper.Map<ReservationDto>(r);

                // Business logic: Determine effective status
                // If cancelled, keep as cancelled
                // If date has passed and not cancelled, mark as completed
                if (r.Status != ReservationStatus.Cancelled && r.ReservationDate < today)
                {
                    dto.Status = ReservationStatus.Completed;
                }

                return dto;
            }).ToList();

            return Ok(reservationDtos);
        }
        /// <summary>
        /// Cancels a single reservation for a specific desk and date
        /// </summary>
        /// <param name="deskId">Desk ID</param>
        /// <param name="date">Reservation date</param>
        /// <param name="userId">User ID (for verification)</param>
        /// <returns>Success or error message</returns>
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
        /// <summary>
        /// Cancels all reservations in a booking group by group ID
        /// </summary>
        /// <param name="ReservationGroupId">Reservation group ID</param>
        /// <param name="userId">User ID (for verification)</param>
        /// <returns>Success message with count of cancelled reservations or error message</returns>
        [HttpPatch("my-reservations/cancel-booking-group/{ReservationGroupId}/{userId}")]
        public async Task<IActionResult> CancelBookingGroupByGroupId(Guid ReservationGroupId, string userId)
        {
            try
            {
                // Verify user (for production authenticate without passing userId in request)
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest("User not found");
                }

                // Find all reservations in this booking group for this user
                var reservations = await _context.Reservations
                    .Where(r => r.ReservationGroupId == ReservationGroupId &&
                    r.UserId == userId &&
                    r.Status == ReservationStatus.Active
                    ).ToListAsync();

                if (reservations.Count == 0)
                {
                    return NotFound($"No active reservations found for booking group: {ReservationGroupId}");
                }

                // Cancel all reservations in the booking group
                foreach (var reservation in reservations)
                {
                    reservation.CanceledAt = DateTime.UtcNow;
                    reservation.Status = ReservationStatus.Cancelled;
                }

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = $"Successfully cancelled {reservations.Count} reservation(s)",
                    cancelledCount = reservations.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to cancel reservation(s): " + ex.Message);
            }
        }
        /// <summary>
        /// Cancels all reservations in a booking group by finding the group through desk ID and date
        /// </summary>
        /// <param name="deskId">Desk ID</param>
        /// <param name="date">Any date in the booking group</param>
        /// <param name="userId">User ID (for verification)</param>
        /// <returns>Success message with count of cancelled reservations or error message</returns>
        [HttpPatch("my-reservations/cancel-booking-group-by-desk/{deskId}/{date}/{userId}")]
        public async Task<IActionResult> CancelBookingGroupByDesk(int deskId, DateOnly date, string userId)
        {
            try
            {
                // Verify user (for production authenticate without passing userId in request)
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest("User not found");
                }

                // Find the reservation entry for the given desk and date
                var reservation = await _context.Reservations
                    .Where(r => r.DeskId == deskId && r.ReservationDate == date && r.UserId == userId)
                    .FirstOrDefaultAsync();

                if (reservation == null)
                {
                    return NotFound($"No reservation found for desk: {deskId} on {date}");
                }

                // Get the reservation group ID from the found reservation
                var reservationGroupId = reservation.ReservationGroupId;

                // Find all reservations in this booking group for this user
                var reservations = await _context.Reservations
                    .Where(r => r.ReservationGroupId == reservationGroupId &&
                    r.UserId == userId &&
                    r.Status == ReservationStatus.Active
                    ).ToListAsync();

                if (reservations.Count == 0)
                {
                    return NotFound($"No active reservations found for booking group: {reservationGroupId}");
                }

                // Cancel all reservations in the booking group
                foreach (var res in reservations)
                {
                    res.CanceledAt = DateTime.UtcNow;
                    res.Status = ReservationStatus.Cancelled;
                }

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = $"Successfully cancelled {reservations.Count} reservation(s)",
                    cancelledCount = reservations.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to cancel reservation(s): " + ex.Message);
            }
        }
    }
}
