namespace DeskBookingService.Models.DTOs;

class FloorPlanDto
{
    public string? buildingName { get; set; }
    public required int FloorPlanWidth { get; set; }
    public required int FloorPlanHeight { get; set; }
    public ICollection<DeskDto>? FloorPlanDesks { get; set; }
}