using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Models;
using DeskBookingService.Services.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace BackendServer.Tests.Validators;

public class ReservationValidatorTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReservationValidator _validator;

    public ReservationValidatorTests()
    {
        // Use a unique database name for each test instance
        var databaseName = $"ReservationValidatorTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
        _context = TestDbContextFactory.CreateInMemoryContext(databaseName);
        TestDbContextFactory.SeedTestData(_context);
        _validator = new ReservationValidator(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region ReservationDate Validation Tests

    [Fact]
    public async Task ReservationDate_ValidDate_PassesValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.ReservationDate);
    }

    [Fact]
    public async Task ReservationDate_Today_PassesValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.ReservationDate);
    }

    [Fact]
    public async Task ReservationDate_PastDate_FailsValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.ReservationDate)
            .WithErrorMessage("Reservation date cannot be in the past");
    }

    [Fact]
    public async Task ReservationDate_Exactly60DaysInAdvance_PassesValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(60))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.ReservationDate);
    }

    [Fact]
    public async Task ReservationDate_MoreThan60DaysInAdvance_FailsValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(61))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.ReservationDate)
            .WithErrorMessage("Reservations cannot be made more than 60 days in advance");
    }

    #endregion

    #region UserId Validation Tests

    [Fact]
    public async Task UserId_ValidUser_PassesValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.UserId);
    }

    [Fact]
    public async Task UserId_NonExistentUser_FailsValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "nonexistent",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.UserId)
            .WithErrorMessage("User does not exist");
    }

    [Fact]
    public async Task UserId_Empty_FailsValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.UserId)
            .WithErrorMessage("User ID is required");
    }

    #endregion

    #region DeskId Validation Tests

    [Fact]
    public async Task DeskId_ValidDesk_PassesValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.DeskId);
    }

    [Fact]
    public async Task DeskId_NonExistentDesk_FailsValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 999,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.DeskId)
            .WithErrorMessage("Desk does not exist");
    }

    #endregion

    #region Conflict Validation Tests

    [Fact]
    public async Task DeskConflict_DeskAlreadyBooked_FailsValidation()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date);

        var reservation = new Reservation
        {
            UserId = "user2",
            DeskId = 1,
            ReservationDate = date
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r)
            .WithErrorMessage("This desk is already booked for the selected date");
    }

    [Fact]
    public async Task DeskConflict_DeskAvailable_PassesValidation()
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7))
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert - Should not have desk conflict error
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("already booked")).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public async Task UserConflict_UserAlreadyHasReservationOnDate_FailsValidation()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user1", 2, date);

        var reservation = new Reservation
        {
            UserId = "user1",
            DeskId = 2,
            ReservationDate = date
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r)
            .WithErrorMessage("You already have a reservation on this date");
    }

    [Fact]
    public async Task Conflict_CancelledReservation_PassesValidation()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date, ReservationStatus.Cancelled);

        var reservation = new Reservation
        {
            UserId = "user2",
            DeskId = 1,
            ReservationDate = date
        };

        // Act
        var result = await _validator.TestValidateAsync(reservation);

        // Assert - Should not have conflict errors since existing reservation is cancelled
        var conflictErrors = result.Errors.Where(e =>
            e.ErrorMessage.Contains("already booked") ||
            e.ErrorMessage.Contains("already have a reservation")).ToList();
        Assert.Empty(conflictErrors);
    }

    #endregion
}
