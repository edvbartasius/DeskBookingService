using FluentValidation;
using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services;

public class UserValidator : AbstractValidator<User>
{
    private readonly AppDbContext _context;

    public UserValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(u => u.Name)
            .NotEmpty().WithMessage("User name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(u => u.Surname)
            .NotEmpty().WithMessage("User last name is required.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("User email is required")
            .EmailAddress().WithMessage("User email must be a valid email address")
            .MinimumLength(2).WithMessage("User email must be at least 2 characters.")
            .MaximumLength(100).WithMessage("User email cannot exceed 100 characters");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("User password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Must(password => password.Any(char.IsDigit))
            .WithMessage("Password must contain at least one digit");
    }

}