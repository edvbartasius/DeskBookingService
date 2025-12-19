namespace DeskBookingService.Models;

public class Desk
{
    public int Id { get; set; }
    public string? Description { get; set; }
    // public DeskStatus Status { get; set; } // Availability should be calculated based on made reservations
    public int BuildingId { get; set; } // Foreign key
    public Building? Building { get; set; } // Navigation property
}


