namespace DeskBookingService.Models;

public class Building
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Desk> Desks { get; set; } = new List<Desk>();
}
