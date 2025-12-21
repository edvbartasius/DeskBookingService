namespace DeskBookingService.Services;

/// <summary>
/// Configuration options for background jobs
/// </summary>
public class BackgroundJobOptions
{
    public const string SectionName = "BackgroundJobs";

    /// <summary>
    /// Interval in minutes between reservation cleanup runs
    /// Default: 60 minutes (1 hour)
    /// </summary>
    public int ReservationCleanupIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Time of day to run daily cleanup (HH:mm format, 24-hour)
    /// Default: "00:00" (midnight)
    /// </summary>
    public string DailyCleanupTime { get; set; } = "00:00";

    /// <summary>
    /// Enable or disable the reservation cleanup service
    /// Default: true
    /// </summary>
    public bool EnableReservationCleanup { get; set; } = true;

    /// <summary>
    /// Use daily cleanup instead of interval-based cleanup
    /// Default: false (use interval-based)
    /// </summary>
    public bool UseDailyCleanup { get; set; } = false;
}

