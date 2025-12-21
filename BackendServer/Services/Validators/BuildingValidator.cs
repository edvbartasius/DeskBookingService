using FluentValidation;
using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services.Validators;

/// <summary>
/// FluentValidation validator for Building entity
/// Validates building name and floor plan dimensions
/// </summary>
public class BuildingValidator : AbstractValidator<Building>
{
    private readonly AppDbContext _context;

    public BuildingValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(b => b.Name)
            .NotEmpty().WithMessage("Building name is required")
            .MinimumLength(2).WithMessage("Building name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("Building name must not exceed 100 characters");

        RuleFor(b => b.FloorPlanHeight)
            .InclusiveBetween(0, 100).WithMessage("Floor plan height must be between 0 and 100");

        RuleFor(b => b.FloorPlanWidth)
            .InclusiveBetween(0, 100).WithMessage("Floor plan width must be between 0 and 100");
    }
}