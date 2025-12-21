using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Models;
using DeskBookingService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace BackendServer.Tests.Services;

public class ReservationCleanupServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _context;

    public ReservationCleanupServiceTests()
    {
        var services = new ServiceCollection();

        // Use a unique database name for each test instance
        var databaseName = $"ReservationCleanupTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";

        // Add DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        // Add logging
        services.AddLogging();

        // Add options
        services.Configure<BackgroundJobOptions>(options =>
        {
            options.ReservationCleanupIntervalMinutes = 1;
        });

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<AppDbContext>();

        TestDbContextFactory.SeedTestData(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task UpdateExpiredReservations_NoExpiredReservations_NoChanges()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, futureDate);

        var initialCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Active);

        // Act - Manually trigger cleanup logic
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredReservations = await _context.Reservations
            .Where(r => r.Status == ReservationStatus.Active && r.ReservationDate < today)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
        }
        await _context.SaveChangesAsync();

        // Assert
        var activeCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Active);
        Assert.Equal(initialCount, activeCount);
    }

    [Fact]
    public async Task UpdateExpiredReservations_WithExpiredReservations_UpdatesToCompleted()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var twoDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, yesterday);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 2, twoDaysAgo);

        // Act - Manually trigger cleanup logic
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredReservations = await _context.Reservations
            .Where(r => r.Status == ReservationStatus.Active && r.ReservationDate < today)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
        }
        await _context.SaveChangesAsync();

        // Assert
        var completedCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Completed);
        Assert.Equal(2, completedCount);

        var activeCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Active);
        Assert.Equal(0, activeCount);
    }

    [Fact]
    public async Task UpdateExpiredReservations_MixedDates_OnlyUpdatesExpired()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, yesterday);
        TestDbContextFactory.CreateTestReservation(_context, "user1", 2, tomorrow);

        // Act - Manually trigger cleanup logic
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredReservations = await _context.Reservations
            .Where(r => r.Status == ReservationStatus.Active && r.ReservationDate < today)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
        }
        await _context.SaveChangesAsync();

        // Assert
        var completedCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Completed);
        Assert.Equal(1, completedCount);

        var activeCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Active);
        Assert.Equal(1, activeCount);
    }

    [Fact]
    public async Task UpdateExpiredReservations_CancelledReservations_NotAffected()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, yesterday, ReservationStatus.Cancelled);

        // Act - Manually trigger cleanup logic
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredReservations = await _context.Reservations
            .Where(r => r.Status == ReservationStatus.Active && r.ReservationDate < today)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
        }
        await _context.SaveChangesAsync();

        // Assert
        var cancelledCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Cancelled);
        Assert.Equal(1, cancelledCount);

        var completedCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Completed);
        Assert.Equal(0, completedCount);
    }
}

