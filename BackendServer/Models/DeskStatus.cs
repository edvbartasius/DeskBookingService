namespace DeskBookingService.Models;

public enum DeskStatus
{
    Open,         // Open for reservation
    Reserved,     // Already reserved (by anyone)
    Maintenance   // Under maintenance
}