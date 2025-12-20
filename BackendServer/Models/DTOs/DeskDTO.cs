namespace DeskBookingService.Models.DTOs;

public class DeskDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public int BuildingId { get; set; } // Foreign key

    // Floor plan positioning (in grid cells)
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public DeskType Type { get; set; }
}