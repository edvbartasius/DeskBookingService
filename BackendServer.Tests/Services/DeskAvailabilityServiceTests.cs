using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Models;
using DeskBookingService.Services;
using FluentAssertions;
using Xunit;

namespace BackendServer.Tests.Services;

[Collection("Sequential")]
public class DeskAvailabilityServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DeskAvailabilityService _service;

    public DeskAvailabilityServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext($"TestDb_{Guid.NewGuid()}");
        TestDbContextFactory.SeedTestData(_context);
        _service = new DeskAvailabilityService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAvailableDesks Tests

    [Fact]
    public async Task GetAvailableDesks_NoReservations_ReturnsAllDesks()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, date, startTime, endTime);

        // Assert
        availableDesks.Should().HaveCount(3); // All 3 desks in test data
        availableDesks.Select(d => d.Id).Should().Contain(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task GetAvailableDesks_OneDeskBooked_ReturnsOtherDesks()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, date, new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Assert
        availableDesks.Should().HaveCount(2);
        availableDesks.Select(d => d.Id).Should().Contain(new[] { 2, 3 });
        availableDesks.Select(d => d.Id).Should().NotContain(1);
    }

    [Fact]
    public async Task GetAvailableDesks_PartialOverlap_ExcludesBookedDesk()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Request 09:00-11:00 (overlaps with 10:00-12:00 booking)
        var availableDesks = await _service.GetAvailableDesks(buildingId, date, new TimeOnly(9, 0), new TimeOnly(11, 0));

        // Assert
        availableDesks.Should().HaveCount(2);
        availableDesks.Select(d => d.Id).Should().NotContain(1);
    }

    [Fact]
    public async Task GetAvailableDesks_BackToBackBooking_IncludesDesk()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Request 12:00-14:00 (starts exactly when previous ends)
        var availableDesks = await _service.GetAvailableDesks(buildingId, date, new TimeOnly(12, 0), new TimeOnly(14, 0));

        // Assert
        availableDesks.Should().HaveCount(3); // All desks should be available
        availableDesks.Select(d => d.Id).Should().Contain(1);
    }

    [Fact]
    public async Task GetAvailableDesks_MultipleTimeSpans_ExcludesIfAnyOverlap()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);
        // Desk 1 has multiple time spans: 09:00-10:00 and 14:00-15:00
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date,
            (new TimeOnly(9, 0), new TimeOnly(10, 0)),
            (new TimeOnly(14, 0), new TimeOnly(15, 0)));

        // Act - Request 09:30-10:30 (overlaps with first span)
        var availableDesks = await _service.GetAvailableDesks(buildingId, date, new TimeOnly(9, 30), new TimeOnly(10, 30));

        // Assert
        availableDesks.Should().NotContain(d => d.Id == 1);
    }

    [Fact]
    public async Task GetAvailableDesks_BetweenTimeSpans_IncludesDesk()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);
        // Desk 1 has gaps: 09:00-10:00 and 14:00-15:00
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date,
            (new TimeOnly(9, 0), new TimeOnly(10, 0)),
            (new TimeOnly(14, 0), new TimeOnly(15, 0)));

        // Act - Request 11:00-13:00 (in between the two spans)
        var availableDesks = await _service.GetAvailableDesks(buildingId, date, new TimeOnly(11, 0), new TimeOnly(13, 0));

        // Assert
        availableDesks.Should().Contain(d => d.Id == 1);
    }

    [Fact]
    public async Task GetAvailableDesks_CancelledReservation_IncludesDesk()
    {
        // Arrange
        var buildingId = 1;
        var date = new DateOnly(2025, 12, 25);
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));
        reservation.Status = ReservationStatus.Cancelled;
        _context.SaveChanges();

        // Act
        var availableDesks = await _service.GetAvailableDesks(buildingId, date, new TimeOnly(10, 0), new TimeOnly(12, 0));

        // Assert
        availableDesks.Should().Contain(d => d.Id == 1); // Should be available since reservation is cancelled
    }

    #endregion

    #region GetDeskBookedTimeSpans Tests

    [Fact]
    public async Task GetDeskBookedTimeSpans_NoReservations_ReturnsEmptyList()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);

        // Act
        var bookedSpans = await _service.GetDeskBookedTimeSpans(deskId, date);

        // Assert
        bookedSpans.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDeskBookedTimeSpans_OneReservation_ReturnsOneTimeSpan()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act
        var bookedSpans = await _service.GetDeskBookedTimeSpans(deskId, date);

        // Assert
        bookedSpans.Should().HaveCount(1);
        bookedSpans[0].StartTime.Should().Be(new TimeOnly(10, 0));
        bookedSpans[0].EndTime.Should().Be(new TimeOnly(12, 0));
    }

    [Fact]
    public async Task GetDeskBookedTimeSpans_MultipleTimeSpans_ReturnsSortedByStartTime()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date,
            (new TimeOnly(14, 0), new TimeOnly(16, 0)),
            (new TimeOnly(9, 0), new TimeOnly(10, 0)),
            (new TimeOnly(11, 0), new TimeOnly(12, 0)));

        // Act
        var bookedSpans = await _service.GetDeskBookedTimeSpans(deskId, date);

        // Assert
        bookedSpans.Should().HaveCount(3);
        bookedSpans[0].StartTime.Should().Be(new TimeOnly(9, 0));
        bookedSpans[1].StartTime.Should().Be(new TimeOnly(11, 0));
        bookedSpans[2].StartTime.Should().Be(new TimeOnly(14, 0));
    }

    [Fact]
    public async Task GetDeskBookedTimeSpans_DifferentDesk_ReturnsEmpty()
    {
        // Arrange
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Query different desk
        var bookedSpans = await _service.GetDeskBookedTimeSpans(2, date);

        // Assert
        bookedSpans.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDeskBookedTimeSpans_DifferentDate_ReturnsEmpty()
    {
        // Arrange
        var deskId = 1;
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, new DateOnly(2025, 12, 25), (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - Query different date
        var bookedSpans = await _service.GetDeskBookedTimeSpans(deskId, new DateOnly(2025, 12, 26));

        // Assert
        bookedSpans.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDeskBookedTimeSpans_CancelledReservation_NotIncluded()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));
        reservation.Status = ReservationStatus.Cancelled;
        _context.SaveChanges();

        // Act
        var bookedSpans = await _service.GetDeskBookedTimeSpans(deskId, date);

        // Assert
        bookedSpans.Should().BeEmpty();
    }

    #endregion

    #region CalculateDeskStatus Tests

    [Fact]
    public async Task CalculateDeskStatus_DeskNotFound_ReturnsUnavailable()
    {
        // Arrange
        var nonExistentDeskId = 999;
        var date = new DateOnly(2025, 12, 22); // Monday
        var time = new TimeOnly(10, 0);

        // Act
        var status = await _service.CalculateDeskStatus(nonExistentDeskId, date, time);

        // Assert
        status.Should().Be(DeskStatus.Unavailable);
    }

    [Fact]
    public async Task CalculateDeskStatus_OutsideOperatingHours_ReturnsUnavailable()
    {
        // Arrange - Building open 8:00-18:00
        var deskId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        var time = new TimeOnly(7, 0); // Before opening

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, time);

        // Assert
        status.Should().Be(DeskStatus.Unavailable);
    }

    [Fact]
    public async Task CalculateDeskStatus_BuildingClosed_ReturnsUnavailable()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 20); // Saturday - closed
        var time = new TimeOnly(10, 0);

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, time);

        // Assert
        status.Should().Be(DeskStatus.Unavailable);
    }

    [Fact]
    public async Task CalculateDeskStatus_WithinOperatingHoursAndNoBooking_ReturnsAvailable()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        var time = new TimeOnly(10, 0);

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, time);

        // Assert
        status.Should().Be(DeskStatus.Available);
    }

    [Fact]
    public async Task CalculateDeskStatus_DuringReservation_ReturnsBooked()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, new TimeOnly(11, 0));

        // Assert
        status.Should().Be(DeskStatus.Booked);
    }

    [Fact]
    public async Task CalculateDeskStatus_AtReservationStartTime_ReturnsBooked()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, new TimeOnly(10, 0));

        // Assert
        status.Should().Be(DeskStatus.Booked);
    }

    [Fact]
    public async Task CalculateDeskStatus_AtReservationEndTime_ReturnsAvailable()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 25);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act - At exactly 12:00 (end time)
        var status = await _service.CalculateDeskStatus(deskId, date, new TimeOnly(12, 0));

        // Assert
        status.Should().Be(DeskStatus.Available); // EndTime is exclusive
    }

    [Fact]
    public async Task CalculateDeskStatus_BeforeReservation_ReturnsAvailable()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, new TimeOnly(9, 0));

        // Assert
        status.Should().Be(DeskStatus.Available);
    }

    [Fact]
    public async Task CalculateDeskStatus_AfterReservation_ReturnsAvailable()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, new TimeOnly(13, 0));

        // Assert
        status.Should().Be(DeskStatus.Available);
    }

    [Fact]
    public async Task CalculateDeskStatus_CancelledReservation_ReturnsAvailable()
    {
        // Arrange
        var deskId = 1;
        var date = new DateOnly(2025, 12, 22); // Monday
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date, (new TimeOnly(10, 0), new TimeOnly(12, 0)));
        reservation.Status = ReservationStatus.Cancelled;
        _context.SaveChanges();

        // Act
        var status = await _service.CalculateDeskStatus(deskId, date, new TimeOnly(11, 0));

        // Assert
        status.Should().Be(DeskStatus.Available);
    }

    #endregion
}
