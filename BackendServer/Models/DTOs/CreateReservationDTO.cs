namespace DeskBookingService.Models.DTOs;

public class CreateReservationDTO
{
    public required int DeskId { get; set; }
    public required List<DateOnly> ReservationDates { get; set; }
}

public class CreateReservationResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ReservationDto> CreatedReservations { get; set; } = new();
    public List<DateOnly> FailedDates { get; set; } = new();
}
