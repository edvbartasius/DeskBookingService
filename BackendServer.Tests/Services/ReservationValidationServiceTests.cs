using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Models;
using DeskBookingService.Services;
using Xunit;

namespace BackendServer.Tests.Services;

public class ReservationValidationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReservationValidationService _service;

    public ReservationValidationServiceTests()
    {
        // Use a unique database name for each test instance
        var databaseName = $"ReservationValidationTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
        _context = TestDbContextFactory.CreateInMemoryContext(databaseName);
        TestDbContextFactory.SeedTestData(_context);
        _service = new ReservationValidationService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    #region ValidateBookingSize Tests

    [Fact]
    public void ValidateBookingSize_WithinLimit_ReturnsValid()
    {
        // Arrange
        var reservationCount = 5;

        // Act
        var (isValid, errorMessage) = _service.ValidateBookingSize(reservationCount);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateBookingSize_ExactlyAtLimit_ReturnsValid()
    {
        // Arrange
        var reservationCount = 7;

        // Act
        var (isValid, errorMessage) = _service.ValidateBookingSize(reservationCount);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateBookingSize_ExceedsLimit_ReturnsInvalid()
    {
        // Arrange
        var reservationCount = 8;

        // Act
        var (isValid, errorMessage) = _service.ValidateBookingSize(reservationCount);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Cannot create more than 7 reservations", errorMessage);
    }

    #endregion

    #region ValidateUserActiveReservationsLimit Tests

    [Fact]
    public async Task ValidateUserActiveReservationsLimit_NoExistingReservations_ReturnsValid()
    {
        // Arrange
        var userId = "user1";
        var additionalReservations = 5;

        // Act
        var (isValid, errorMessage) = await _service.ValidateUserActiveReservationsLimit(userId, additionalReservations);


        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public async Task ValidateUserActiveReservationsLimit_WithinLimit_ReturnsValid()
    {
        // Arrange
        var userId = "user1";
        var additionalReservations = 5;

        // Create 20 existing reservations
        for (int i = 0; i < 20; i++)
        {
            TestDbContextFactory.CreateTestReservation(_context, userId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)));
        }

        // Act
        var (isValid, errorMessage) = await _service.ValidateUserActiveReservationsLimit(userId, additionalReservations);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public async Task ValidateUserActiveReservationsLimit_ExceedsLimit_ReturnsInvalid()
    {
        // Arrange
        var userId = "user1";
        var additionalReservations = 5;

        // Create 28 existing reservations (28 + 5 = 33 > 30)
        for (int i = 0; i < 28; i++)
        {
            TestDbContextFactory.CreateTestReservation(_context, userId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)));
        }

        // Act
        var (isValid, errorMessage) = await _service.ValidateUserActiveReservationsLimit(userId, additionalReservations);

        // Assert
        Assert.False(isValid);
        Assert.Contains("exceed the maximum limit of 30", errorMessage);
    }

    [Fact]
    public async Task ValidateUserActiveReservationsLimit_CancelledReservationsNotCounted_ReturnsValid()
    {
        // Arrange
        var userId = "user1";
        var additionalReservations = 5;

        // Create 28 cancelled reservations (should not count)
        for (int i = 0; i < 28; i++)
        {
            TestDbContextFactory.CreateTestReservation(_context, userId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)), ReservationStatus.Cancelled);
        }

        // Act
        var (isValid, errorMessage) = await _service.ValidateUserActiveReservationsLimit(userId, additionalReservations);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }
}

#endregion

