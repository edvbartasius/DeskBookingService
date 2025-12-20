namespace DeskBookingService.Models;

public enum ReservationStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2, // Act as a soft delete in DB
}

public enum DeskStatus
{
    Open,         // Open for reservation
    Reserved,     // Already reserved (by anyone)
    Maintenance   // Under maintenance
}