namespace DeskBookingService.Models;

public class Building
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<OperatingHours> OperatingHours { get; set;} = new List<OperatingHours>();
    public int FloorPlanWidth { get; set;} = 15; // Floor plan canvas width in cells
    public int FloorPlanHeight { get; set;} = 10; // Floor plan canvas height in cells

    public ICollection<Desk> Desks { get; set; } = new List<Desk>();
}
