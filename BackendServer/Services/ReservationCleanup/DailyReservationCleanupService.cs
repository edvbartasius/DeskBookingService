using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingService.Services;

/// <summary>
/// Background service that runs once per day at midnight to update expired reservations
/// Alternative to ReservationCleanupService - use this if you want to run at a specific time
/// </summary>
public class DailyReservationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyReservationCleanupService> _logger;
    private readonly TimeOnly _scheduledTime = new TimeOnly(0, 0); // Midnight

    public DailyReservationCleanupService(
        IServiceProvider serviceProvider,
        ILogger<DailyReservationCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Daily Reservation Cleanup Service started. Scheduled to run at {Time}",
            _scheduledTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var scheduledDateTime = GetNextScheduledTime(now);
                var delay = scheduledDateTime - now;

                _logger.LogInformation(
                    "Next cleanup scheduled for {ScheduledTime} (in {Hours:F1} hours)",
                    scheduledDateTime,
                    delay.TotalHours);

                // Wait until the scheduled time
                await Task.Delay(delay, stoppingToken);

                // Run the cleanup
                await UpdateExpiredReservations(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in daily cleanup service");
                // Wait a bit before retrying
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Daily Reservation Cleanup Service stopped");
    }

    private DateTime GetNextScheduledTime(DateTime currentTime)
    {
        var today = DateOnly.FromDateTime(currentTime);
        var scheduledToday = today.ToDateTime(_scheduledTime);

        // If the scheduled time has already passed today, schedule for tomorrow
        if (currentTime >= scheduledToday)
        {
            return today.AddDays(1).ToDateTime(_scheduledTime);
        }

        return scheduledToday;
    }

    private async Task UpdateExpiredReservations(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _logger.LogInformation("Running daily cleanup for reservations before {Today}", today);

        var expiredReservations = await dbContext.Reservations
            .Where(r => r.Status == ReservationStatus.Active && 
                       r.ReservationDate < today)
            .ToListAsync(cancellationToken);

        if (expiredReservations.Count == 0)
        {
            _logger.LogInformation("No expired reservations found");
            return;
        }

        _logger.LogInformation("Updating {Count} expired reservations", expiredReservations.Count);

        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
        }

        var updatedCount = await dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Daily cleanup completed. Updated {Count} reservations to Completed status",
            updatedCount);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Daily Reservation Cleanup Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}

