namespace DeskBookingService.Models.DTOs;

public class DeskStatusDto
{
    public int DeskId { get; set; }
    public DateOnly Date { get; set; }
    public DeskStatus Status { get; set; }

    // If status is Booked, include reserver info
    public string? ReservedByFirstName { get; set; }
    public string? ReservedByLastName { get; set; }
    public string? ReservedByUserId { get; set; }

    // If status is Unavailable, optionally include reason
    public string? UnavailableReason { get; set; }
}

public class DeskAvailabilityDto
{
    public int DeskId { get; set; }
    public string? Description { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public DeskType Type { get; set; }

    // Status for each date in the requested range
    public List<DeskStatusDto> StatusByDate { get; set; } = new();
}
