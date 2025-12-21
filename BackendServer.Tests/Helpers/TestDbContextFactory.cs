using DeskBookingService;
using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendServer.Tests.Helpers;

public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static AppDbContext CreateInMemoryContext(string databaseName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new AppDbContext(options);

        // Ensure the database is clean before use
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Seeds the database with basic test data
    /// </summary>
    public static void SeedTestData(AppDbContext context)
    {
        // Create test building
        var building = new Building
        {
            Id = 1,
            Name = "Test Building",
            FloorPlanWidth = 15,
            FloorPlanHeight = 10
        };
        context.Buildings.Add(building);

        // Create test desks
        var desk1 = new Desk
        {
            Id = 1,
            BuildingId = 1,
            Description = "Desk 1",
            PositionX = 1,
            PositionY = 1,
            Type = DeskType.RegularDesk,
            IsInMaintenance = false
        };
        var desk2 = new Desk
        {
            Id = 2,
            BuildingId = 1,
            Description = "Desk 2",
            PositionX = 2,
            PositionY = 1,
            Type = DeskType.RegularDesk,
            IsInMaintenance = false
        };
        var desk3 = new Desk
        {
            Id = 3,
            BuildingId = 1,
            Description = "Conference Room",
            PositionX = 5,
            PositionY = 5,
            Type = DeskType.ConferenceRoom,
            IsInMaintenance = false
        };
        context.Desks.AddRange(desk1, desk2, desk3);

        // Create test users
        var user1 = new User
        {
            Id = "user1",
            Name = "John",
            Surname = "Doe",
            Email = "john.doe@test.com",
            Password = "password123",
            Role = UserRole.User
        };
        var user2 = new User
        {
            Id = "user2",
            Name = "Jane",
            Surname = "Smith",
            Email = "jane.smith@test.com",
            Password = "password456",
            Role = UserRole.Admin
        };
        context.Users.AddRange(user1, user2);

        context.SaveChanges();
    }

    /// <summary>
    /// Creates a test reservation
    /// </summary>
    public static Reservation CreateTestReservation(
        AppDbContext context,
        string userId,
        int deskId,
        DateOnly date,
        ReservationStatus status = ReservationStatus.Active,
        Guid? groupId = null)
    {
        var reservation = new Reservation
        {
            UserId = userId,
            DeskId = deskId,
            ReservationDate = date,
            Status = status,
            ReservationGroupId = groupId ?? Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        context.Reservations.Add(reservation);
        context.SaveChanges();

        return reservation;
    }

    /// <summary>
    /// Creates multiple test reservations with the same group ID
    /// </summary>
    public static List<Reservation> CreateTestReservationGroup(
        AppDbContext context,
        string userId,
        int deskId,
        List<DateOnly> dates,
        ReservationStatus status = ReservationStatus.Active)
    {
        var groupId = Guid.NewGuid();
        var reservations = new List<Reservation>();

        foreach (var date in dates)
        {
            var reservation = CreateTestReservation(context, userId, deskId, date, status, groupId);
            reservations.Add(reservation);
        }

        return reservations;
    }
}

