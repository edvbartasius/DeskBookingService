using FluentValidation;
using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services.Validators;

/// <summary>
/// FluentValidation validator for Desk entity
/// Validates desk description, building ID, position coordinates, and ensures position is within floor plan bounds
/// </summary>
public class DeskValidator : AbstractValidator<Desk>
{
    private readonly AppDbContext _context;

    public DeskValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(d => d.Description)
            .NotEmpty().WithMessage("Desk must have a description")
            .MinimumLength(1).WithMessage("Desk description must be at least 1 character long")
            .MaximumLength(255).WithMessage("Desk description must not exceed 255 characters");

        RuleFor(d => d.BuildingId)
            .NotEmpty().WithMessage("Building ID is required")
            .GreaterThan(0).WithMessage("Invalid Building ID")
            .MustAsync(async (buildingId, cancellation) => await BuildingExists(buildingId))
            .WithMessage("Building does not exist");

        RuleFor(d => d.PositionX)
            .GreaterThanOrEqualTo(0).WithMessage("Position X must be non-negative");

        RuleFor(d => d.PositionY)
            .GreaterThanOrEqualTo(0).WithMessage("Position Y must be non-negative");

        // Check if desk position is within building's floor plan dimensions
        RuleFor(d => d)
            .MustAsync(async (desk, cancellation) => await IsPositionWithinFloorPlan(desk))
            .WithMessage("Desk position is outside the building's floor plan dimensions");
    }

    /// <summary>
    /// Checks if a building exists in the database
    /// </summary>
    private async Task<bool> BuildingExists(int buildingId)
    {
        return await _context.Buildings.AnyAsync(b => b.Id == buildingId);
    }

    /// <summary>
    /// Validates that the desk position is within the building's floor plan dimensions
    /// </summary>
    private async Task<bool> IsPositionWithinFloorPlan(Desk desk)
    {
        var building = await _context.Buildings
            .Where(b => b.Id == desk.BuildingId)
            .Select(b => new { b.FloorPlanWidth, b.FloorPlanHeight })
            .FirstOrDefaultAsync();

        if (building == null)
            return false;

        // Check if position is within bounds
        return desk.PositionX >= 0 && desk.PositionX < building.FloorPlanWidth &&
               desk.PositionY >= 0 && desk.PositionY < building.FloorPlanHeight;
    }
}