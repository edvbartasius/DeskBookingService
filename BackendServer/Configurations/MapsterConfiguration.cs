using DeskBookingService.Models;
using DeskBookingService.Models.AdminDTOs;
using Mapster;

namespace DeskBookingService.Configurations;

public static class MapsterConfiguration
{
    public static void Configure()
    {
        // Configure mapping from Desk to AdminDeskDto
        TypeAdapterConfig<Desk, AdminDeskDto>
            .NewConfig()
            .Map(dest => dest.BuildingName, src => src.Building != null ? src.Building.Name : null);

        // You can add more custom mappings here as needed
        // For example, if you want to map User or Desk names in Reservations:
        // TypeAdapterConfig<Reservation, AdminReservationDto>
        //     .NewConfig()
        //     .Map(dest => dest.UserName, src => src.User != null ? src.User.Name : null);
    }
}
