using DeskBookingService;
using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendServer.Tests.Helpers;

public static class TestDbContextFactory
{
    private static readonly object _lock = new();
    private static int _globalIdCounter = 1000; // Start high to avoid conflicts with seed data

    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static AppDbContext CreateInMemoryContext(string databaseName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        var context = new AppDbContext(options);
        return context;
    }

    /// <summary>
    /// Seeds the database with test data for desk booking scenarios
    /// </summary>
    public static void SeedTestData(AppDbContext context)
    {
        // Clear existing data and reset
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Remove any existing data first (for in-memory safety)
        context.Buildings.RemoveRange(context.Buildings);
        context.OperatingHours.RemoveRange(context.OperatingHours);
        context.Desks.RemoveRange(context.Desks);
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();

        // Create a test building
        var building = new Building
        {
            Id = 1,
            Name = "Test Building",
            FloorPlanWidth = 15,
            FloorPlanHeight = 10
        };
        context.Buildings.Add(building);
        context.SaveChanges(); // Save immediately

        // Add operating hours (Monday-Friday 8:00-18:00)
        var operatingHours = new List<OperatingHours>
        {
            new() { Id = 1, BuildingId = 1, DayOfWeek = DayOfWeek.Monday, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0), IsClosed = false },
            new() { Id = 2, BuildingId = 1, DayOfWeek = DayOfWeek.Tuesday, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0), IsClosed = false },
            new() { Id = 3, BuildingId = 1, DayOfWeek = DayOfWeek.Wednesday, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0), IsClosed = false },
            new() { Id = 4, BuildingId = 1, DayOfWeek = DayOfWeek.Thursday, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0), IsClosed = false },
            new() { Id = 5, BuildingId = 1, DayOfWeek = DayOfWeek.Friday, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0), IsClosed = false },
            new() { Id = 6, BuildingId = 1, DayOfWeek = DayOfWeek.Saturday, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0), IsClosed = true },
            new() { Id = 7, BuildingId = 1, DayOfWeek = DayOfWeek.Sunday, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0), IsClosed = true }
        };
        context.OperatingHours.AddRange(operatingHours);
        context.SaveChanges(); // Save immediately

        // Create test desks
        var desk1 = new Desk { Id = 1, BuildingId = 1, Description = "Desk 1", PositionX = 1, PositionY = 1, Type = DeskType.RegularDesk };
        var desk2 = new Desk { Id = 2, BuildingId = 1, Description = "Desk 2", PositionX = 2, PositionY = 1, Type = DeskType.RegularDesk };
        var desk3 = new Desk { Id = 3, BuildingId = 1, Description = "Conference Room", PositionX = 5, PositionY = 5, Type = DeskType.ConferenceRoom };
        context.Desks.AddRange(desk1, desk2, desk3);
        context.SaveChanges(); // Save immediately

        // Create a test user
        var user = new User
        {
            Id = "user1",
            Name = "User",
            Surname = "Tester",
            Email = "test@example.com",
            Password = "passwordTest1",
            Role = UserRole.User
        };
        context.Users.Add(user);

        context.SaveChanges();
    }

    /// <summary>
    /// Creates a reservation with time spans for testing
    /// </summary>
    public static Reservation CreateTestReservation(
        AppDbContext context,
        string userId,
        int deskId,
        DateOnly date,
        params (TimeOnly start, TimeOnly end)[] timeSpans)
    {
        var reservation = new Reservation
        {
            UserId = userId,
            DeskId = deskId,
            ReservationDate = date,
            Status = ReservationStatus.Active
        };
        context.Reservations.Add(reservation);
        context.SaveChanges();

        foreach (var (start, end) in timeSpans)
        {
            int id;
            lock (_lock)
            {
                id = _globalIdCounter++;
            }

            var timeSpan = new ReservationTimeSpan
            {
                Id = id, // In-memory DB requires explicit unique ID
                ReservationId = reservation.Id,
                StartTime = start,
                EndTime = end,
                Status = ReservationStatus.Active
            };
            context.ReservationTimeSpans.Add(timeSpan);
        }
        context.SaveChanges();

        return reservation;
    }
}
