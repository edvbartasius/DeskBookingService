namespace DeskBookingService.Models;

public class Desk
{
    public int Id { get; set; }
    public string DeskNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BuildingId { get; set; } // Foreign key
    public Building? Building { get; set; } // Navigation property

    public float PositionX { get; set; } // Position for desk grid visualisation
    public float PositionY { get; set; }
    public DeskType Type { get; set;} // To visualise table size by type

    public bool IsInMaintenance { get; set; } = false;
    public string? MaintenanceReason { get; set; }

    public ICollection<Reservation> Reservations { get; set;} = new List<Reservation>();
}


