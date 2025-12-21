namespace DeskBookingService.Models.DTOs;

public class DeskDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public int BuildingId { get; set; } // Foreign key
    public float PositionX { get; set; }  // Floor plan positioning (in grid cells)
    public float PositionY { get; set; }
    public DeskStatus Status { get; set; }
    public DeskType Type { get; set; }

    // Reservation info (populated when desk is booked)
    public bool IsReservedByCaller { get; set; }
    public string? ReservedByFullName { get; set; }

    // Maintenance info
    public bool IsInMaintenance { get; set; }
    public string? MaintenanceReason { get; set; }
}