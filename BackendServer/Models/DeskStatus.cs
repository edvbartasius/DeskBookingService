namespace DeskBookingService.Models;

public enum ReservationStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2, // Act as a soft delete in DB
}

public enum DeskStatus
{
    Available,    // Desk is free and can be booked
    Booked,       // Desk is currently reserved
    Unavailable   // Outside operating hours, desk doesn't exist or in maintenance
}