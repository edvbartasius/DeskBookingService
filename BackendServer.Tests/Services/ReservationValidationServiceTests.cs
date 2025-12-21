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

    #region IsDeskAvailable Tests

    [Fact]
    public async Task IsDeskAvailable_NoReservations_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);

        // Act
        var result = await _service.IsDeskAvailable(deskId, date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsDeskAvailable_DeskAlreadyBooked_ReturnsFalse()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date);

        // Act
        var result = await _service.IsDeskAvailable(deskId, date);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsDeskAvailable_CancelledReservation_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, ReservationStatus.Cancelled);

        // Act
        var result = await _service.IsDeskAvailable(deskId, date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsDeskAvailable_WithExcludeReservationId_IgnoresOwnReservation()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date);

        // Act
        var result = await _service.IsDeskAvailable(deskId, date, reservation.Id);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region ValidateDateNotInPast Tests

    [Fact]
    public void ValidateDateNotInPast_FutureDate_ReturnsValid()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        // Act
        var (isValid, errorMessage) = _service.ValidateDateNotInPast(futureDate);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateDateNotInPast_Today_ReturnsValid()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var (isValid, errorMessage) = _service.ValidateDateNotInPast(today);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateDateNotInPast_PastDate_ReturnsInvalid()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Act
        var (isValid, errorMessage) = _service.ValidateDateNotInPast(pastDate);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Cannot make reservations for past dates", errorMessage);
    }

    #endregion

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

    #endregion

    #region ValidateReservation Tests

    [Fact]
    public async Task ValidateReservation_ValidReservation_ReturnsValid()
    {
        // Arrange
        var deskId = 1;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        // Act
        var (isValid, errorMessage) = await _service.ValidateReservation(deskId, date);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public async Task ValidateReservation_PastDate_ReturnsInvalid()
    {
        // Arrange
        var deskId = 1;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Act
        var (isValid, errorMessage) = await _service.ValidateReservation(deskId, date);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Cannot make reservations for past dates", errorMessage);
    }

    [Fact]
    public async Task ValidateReservation_DeskNotFound_ReturnsInvalid()
    {
        // Arrange
        var deskId = 999;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        // Act
        var (isValid, errorMessage) = await _service.ValidateReservation(deskId, date);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Desk with ID 999 not found", errorMessage);
    }

    [Fact]
    public async Task ValidateReservation_DeskAlreadyBooked_ReturnsInvalid()
    {
        // Arrange
        var deskId = 1;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date);

        // Act
        var (isValid, errorMessage) = await _service.ValidateReservation(deskId, date);

        // Assert
        Assert.False(isValid);
        Assert.Contains("already reserved", errorMessage);
    }

    #endregion
}

