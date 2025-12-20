namespace DeskBookingService.Models;

public class Desk
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public int BuildingId { get; set; } // Foreign key
    public Building? Building { get; set; } // Navigation property

    public float PositionX { get; set; } // Position for desk grid visualisation
    public float PositionY { get; set; }
    public DeskType Type { get; set;} // Visualise table size by type

    public ICollection<Reservation> Reservations { get; set;} = new List<Reservation>();
}


