namespace DeskBookingService.Models.DTOs;

public class BuildingDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Floor plan dimensions (in grid cells)
    public int FloorPlanWidth { get; set; }
    public int FloorPlanHeight { get; set; }
}