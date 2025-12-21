using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DeskBookingService.Services;

/// <summary>
/// Background service that runs periodically to update expired reservations
/// from Active status to Completed status
/// </summary>
public class ReservationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservationCleanupService> _logger;
    private readonly BackgroundJobOptions _options;
    private readonly TimeSpan _checkInterval;

    public ReservationCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ReservationCleanupService> logger,
        IOptions<BackgroundJobOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
        _checkInterval = TimeSpan.FromMinutes(_options.ReservationCleanupIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Reservation Cleanup Service started. Running every {Interval} minutes",
            _options.ReservationCleanupIntervalMinutes);

        // Wait a bit before first run (let the app start up)
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateExpiredReservations(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating expired reservations");
            }

            // Wait for the next interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Reservation Cleanup Service stopped");
    }

    private async Task UpdateExpiredReservations(CancellationToken cancellationToken)
    {
        // Create a new scope to get a scoped DbContext
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _logger.LogInformation("Checking for expired reservations (date < {Today})", today);

        // Find all active reservations where the date has passed
        var expiredReservations = await dbContext.Reservations
            .Where(r => r.Status == ReservationStatus.Active && 
                       r.ReservationDate < today)
            .ToListAsync(cancellationToken);

        if (expiredReservations.Count == 0)
        {
            _logger.LogInformation("No expired reservations found");
            return;
        }

        _logger.LogInformation("Found {Count} expired reservations to update", expiredReservations.Count);

        // Update all expired reservations to Completed status
        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
            
            _logger.LogDebug(
                "Updated reservation {ReservationId} for desk {DeskId} on {Date} to Completed",
                reservation.Id,
                reservation.DeskId,
                reservation.ReservationDate);
        }

        // Save changes
        var updatedCount = await dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Successfully updated {Count} reservations to Completed status",
            updatedCount);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reservation Cleanup Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}

