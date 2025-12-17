namespace DeskBookingService.Models.DTOs;

public class DeskDto
{
    public string? Description { get; set; }
    public DeskStatus Status { get; set; }
    public int BuildingId { get; set; } // Foreign key
}