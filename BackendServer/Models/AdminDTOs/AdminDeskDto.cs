namespace DeskBookingService.Models.AdminDTOs;

public class AdminDeskDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public DeskStatus Status { get; set; }
    public int BuildingId { get; set; }
    public string? BuildingName { get; set; }
}
