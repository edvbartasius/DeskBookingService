using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Models;
using DeskBookingService.Services;
using FluentAssertions;
using Xunit;

namespace BackendServer.Tests.Services;

[Collection("Sequential")]
public class ReservationValidationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReservationValidationService _service;

    public ReservationValidationServiceTests()
    {
        // Create a unique database for each test
        _context = TestDbContextFactory.CreateInMemoryContext($"TestDb_{Guid.NewGuid()}");
        TestDbContextFactory.SeedTestData(_context);
        _service = new ReservationValidationService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region ValidateNoConflicts Tests

    [Fact]
    public async Task ValidateNoConflicts_NoExistingReservations_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(12, 0);

        // Act
        var result = await _service.ValidateNoConflicts(deskId, date, startTime, endTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_ExactOverlap_ReturnsFalse()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Try to book exact same time
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateNoConflicts_PartialOverlapAtStart_ReturnsFalse()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - New reservation 09:00-11:00 overlaps with existing 10:00-12:00
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(9, 0), new TimeOnly(11, 0));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateNoConflicts_PartialOverlapAtEnd_ReturnsFalse()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - New reservation 11:00-13:00 overlaps with existing 10:00-12:00
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(11, 0), new TimeOnly(13, 0));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateNoConflicts_NewContainsExisting_ReturnsFalse()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - New reservation 09:00-13:00 completely contains existing 10:00-12:00
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(9, 0), new TimeOnly(13, 0));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateNoConflicts_ExistingContainsNew_ReturnsFalse()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(9, 0), new TimeOnly(13, 0)));

        // Act - New reservation 10:00-12:00 is contained within existing 09:00-13:00
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateNoConflicts_BackToBack_EndsWhenNextStarts_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - New reservation 12:00-14:00 starts exactly when existing ends
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(12, 0), new TimeOnly(14, 0));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_BackToBack_StartsWhenPreviousEnds_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - New reservation 08:00-10:00 ends exactly when existing starts
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(8, 0), new TimeOnly(10, 0));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_CompletelyBefore_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - New reservation 08:00-09:00 is completely before existing
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(8, 0), new TimeOnly(9, 0));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_CompletelyAfter_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - New reservation 13:00-15:00 is completely after existing
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(13, 0), new TimeOnly(15, 0));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_DifferentDesk_ReturnsTrue()
    {
        // Arrange
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Same time, different desk
        var result = await _service.ValidateNoConflicts(2, date, new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_DifferentDate_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, new DateOnly(2025, 12, 25), (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Same desk and time, different date
        var result = await _service.ValidateNoConflicts(deskId, new DateOnly(2025, 12, 26), new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_CancelledReservation_ReturnsTrue()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Cancel the reservation
        reservation.Status = ReservationStatus.Cancelled;
        _context.SaveChanges();

        // Act - Try to book same time (cancelled reservation should not conflict)
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoConflicts_WithExcludeReservationId_IgnoresOwnReservation()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Check conflict but exclude own reservation (useful for updates)
        var result = await _service.ValidateNoConflicts(deskId, date, new TimeOnly(10, 0), new TimeOnly(12, 0), excludeReservationId: reservation.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region ValidateOperatingHours Tests

    [Fact]
    public async Task ValidateOperatingHours_WithinHours_ReturnsValid()
    {
        // Arrange - Building 1 is open 8:00-18:00 on weekdays
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var (isValid, errorMessage) = await _service.ValidateOperatingHours(buildingId, date, startTime, endTime);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateOperatingHours_ExactlyAtOpeningAndClosing_ReturnsValid()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        var startTime = new TimeOnly(8, 0);  // Exactly at opening
        var endTime = new TimeOnly(18, 0);    // Exactly at closing

        // Act
        var (isValid, errorMessage) = await _service.ValidateOperatingHours(buildingId, date, startTime, endTime);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateOperatingHours_StartsBeforeOpening_ReturnsInvalid()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        var startTime = new TimeOnly(7, 59);  // 1 minute before opening
        var endTime = new TimeOnly(10, 0);

        // Act
        var (isValid, errorMessage) = await _service.ValidateOperatingHours(buildingId, date, startTime, endTime);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("before opening time");
    }

    [Fact]
    public async Task ValidateOperatingHours_EndsAfterClosing_ReturnsInvalid()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        var startTime = new TimeOnly(16, 0);
        var endTime = new TimeOnly(18, 1);     // 1 minute after closing

        // Act
        var (isValid, errorMessage) = await _service.ValidateOperatingHours(buildingId, date, startTime, endTime);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("after closing time");
    }

    [Fact]
    public async Task ValidateOperatingHours_BuildingClosed_ReturnsInvalid()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 20); // Saturday - building is closed
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var (isValid, errorMessage) = await _service.ValidateOperatingHours(buildingId, date, startTime, endTime);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("closed");
    }

    [Fact]
    public async Task ValidateOperatingHours_NoOperatingHoursConfigured_ReturnsInvalid()
    {
        // Arrange - Create a building without operating hours
        var newBuilding = new Building { Id = 99, Name = "Building Without Hours" };
        _context.Buildings.Add(newBuilding);
        _context.SaveChanges();

        var date = new DateOnly(2025, 12, 22);
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var (isValid, errorMessage) = await _service.ValidateOperatingHours(99, date, startTime, endTime);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("No operating hours configured");
    }

    #endregion
}
