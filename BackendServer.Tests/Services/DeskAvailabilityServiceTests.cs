using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Models;
using DeskBookingService.Services;
using Xunit;

namespace BackendServer.Tests.Services;

public class DeskAvailabilityServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DeskAvailabilityService _service;

    public DeskAvailabilityServiceTests()
    {
        // Use a unique database name for each test instance
        var databaseName = $"DeskAvailabilityTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
        _context = TestDbContextFactory.CreateInMemoryContext(databaseName);
        TestDbContextFactory.SeedTestData(_context);
        _service = new DeskAvailabilityService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAvailableDesks (Date Range) Tests

    [Fact]
    public async Task GetAvailableDesks_NoReservations_ReturnsAllDesks()
    {
        // Arrange
        var buildingId = 1;
        var startDate = new DateOnly(2025, 12, 25);
        var endDate = new DateOnly(2025, 12, 27);

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, startDate, endDate);

        // Assert
        Assert.Equal(3, availableDesks.Count); // All 3 desks in test data
        var deskIds = availableDesks.Select(d => d.Id).ToList();
        Assert.Contains(1, deskIds);
        Assert.Contains(2, deskIds);
        Assert.Contains(3, deskIds);
    }

    [Fact]
    public async Task GetAvailableDesks_OneDeskBookedForOneDay_ExcludesThatDesk()
    {
        // Arrange
        var buildingId = 1;
        var startDate = new DateOnly(2025, 12, 25);
        var endDate = new DateOnly(2025, 12, 27);

        // Book desk 1 for Dec 26
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, new DateOnly(2025, 12, 26));

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, startDate, endDate);

        // Assert
        Assert.Equal(2, availableDesks.Count);
        var deskIds = availableDesks.Select(d => d.Id).ToList();
        Assert.Contains(2, deskIds);
        Assert.Contains(3, deskIds);
        Assert.DoesNotContain(1, deskIds);
    }

    [Fact]
    public async Task GetAvailableDesks_DeskBookedOutsideDateRange_IncludesDesk()
    {
        // Arrange
        var buildingId = 1;
        var startDate = new DateOnly(2025, 12, 25);
        var endDate = new DateOnly(2025, 12, 27);

        // Book desk 1 for Dec 24 (before range)
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, new DateOnly(2025, 12, 24));

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, startDate, endDate);

        // Assert
        Assert.Equal(3, availableDesks.Count);
        var deskIds = availableDesks.Select(d => d.Id).ToList();
        Assert.Contains(1, deskIds);
    }

    [Fact]
    public async Task GetAvailableDesks_CancelledReservation_IncludesDesk()
    {
        // Arrange
        var buildingId = 1;
        var startDate = new DateOnly(2025, 12, 25);
        var endDate = new DateOnly(2025, 12, 27);

        // Book and cancel desk 1
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, new DateOnly(2025, 12, 26), ReservationStatus.Cancelled);

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, startDate, endDate);

        // Assert
        Assert.Equal(3, availableDesks.Count);
        var deskIds = availableDesks.Select(d => d.Id).ToList();
        Assert.Contains(1, deskIds);
    }

    [Fact]
    public async Task GetAvailableDesks_DifferentBuilding_ReturnsEmpty()
    {
        // Arrange
        var buildingId = 999; // Non-existent building
        var startDate = new DateOnly(2025, 12, 25);
        var endDate = new DateOnly(2025, 12, 27);

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, startDate, endDate);

        // Assert
        Assert.Empty(availableDesks);
    }

    #endregion

    #region GetAvailableDesksForDate (Single Date) Tests

    [Fact]
    public async Task GetAvailableDesksForDate_NoReservations_ReturnsAllDesks()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);

        // Act
        var availableDesks = await _service.GetAvailableDesksForDate(buildingId, date);

        // Assert
        Assert.Equal(3, availableDesks.Count);
        var deskIds = availableDesks.Select(d => d.Id).ToList();
        Assert.Contains(1, deskIds);
        Assert.Contains(2, deskIds);
        Assert.Contains(3, deskIds);
    }

    [Fact]
    public async Task GetAvailableDesksForDate_OneDeskBooked_ReturnsOtherDesks()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);

        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date);

        // Act
        var availableDesks = await _service.GetAvailableDesksForDate(buildingId, date);

        // Assert
        Assert.Equal(2, availableDesks.Count);
        var deskIds = availableDesks.Select(d => d.Id).ToList();
        Assert.Contains(2, deskIds);
        Assert.Contains(3, deskIds);
        Assert.DoesNotContain(1, deskIds);
    }

    [Fact]
    public async Task GetAvailableDesksForDate_AllDesksBooked_ReturnsEmpty()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);

        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 2, date);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 3, date);

        // Act
        var availableDesks = await _service.GetAvailableDesksForDate(buildingId, date);

        // Assert
        Assert.Empty(availableDesks);
    }

    #endregion
}

