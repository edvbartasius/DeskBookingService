namespace DeskBookingService.Models.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; }
}
