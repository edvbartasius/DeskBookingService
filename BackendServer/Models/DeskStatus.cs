namespace DeskBookingService.Models;

public enum ReservationStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2, // Act as a soft delete in DB
}