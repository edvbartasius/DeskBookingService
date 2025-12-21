using FluentValidation;
using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services.Validators;

/// <summary>
/// FluentValidation validator for Reservation entity
/// Validates user ID, desk ID, date, and checks for conflicts
/// </summary>
public class ReservationValidator : AbstractValidator<Reservation>
{
    private readonly AppDbContext _context;

    public ReservationValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(r => r.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MustAsync(async (userId, cancellation) => await UserExists(userId))
            .WithMessage("User does not exist");

        RuleFor(r => r.DeskId)
            .NotEmpty().WithMessage("Desk ID is required")
            .MustAsync(async (deskId, cancellation) => await DeskExists(deskId))
            .WithMessage("Desk does not exist");

        RuleFor(r => r.ReservationDate)
            .NotEmpty().WithMessage("Reservation date is required")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Reservation date cannot be in the past");

        // Check for conflicting reservations (make sure desk is not booked)
        RuleFor(r => r)
            .MustAsync(async (reservation, cancellation) => await HasNoDeskConflict(reservation))
            .WithMessage("This desk is already booked for the selected date");

        // Check user doesn't have another reservation on the same date
        RuleFor(r => r)
            .MustAsync(async (reservation, cancellation) => await UserHasNoConflictingReservation(reservation))
            .WithMessage("You already have a reservation on this date");
    }

    /// <summary>
    /// Checks if the user has no conflicting reservation on the same date
    /// </summary>
    private async Task<bool> UserHasNoConflictingReservation(Reservation reservation)
    {
        return !await _context.Reservations
            .Where(r => r.DeskId == reservation.DeskId &&
                        r.Id != reservation.Id && // Exclude current reservation (for updates)
                        r.ReservationDate == reservation.ReservationDate &&
                        r.Status == ReservationStatus.Active)
            .AnyAsync();
    }

    /// <summary>
    /// Checks if the desk has no conflicting active reservation on the same date
    /// </summary>
    private async Task<bool> HasNoDeskConflict(Reservation reservation)
    {
        return !await _context.Reservations
            .Where(r => r.DeskId == reservation.DeskId &&
                        r.Id != reservation.Id && // Exclude current reservation (for updates)
                        r.ReservationDate == reservation.ReservationDate &&
                        r.Status == ReservationStatus.Active)
            .AnyAsync();
    }

    /// <summary>
    /// Checks if a user exists in the database
    /// </summary>
    private async Task<bool> UserExists(string userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    /// <summary>
    /// Checks if a desk exists in the database
    /// </summary>
    private async Task<bool> DeskExists(int deskId)
    {
        return await _context.Desks.AnyAsync(d => d.Id == deskId);
    }
}
