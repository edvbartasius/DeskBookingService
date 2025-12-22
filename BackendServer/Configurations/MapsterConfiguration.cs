using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Mapster;

namespace DeskBookingService.Configurations;

public static class MapsterConfiguration
{
    public static void Configure()
    {
        // Configure mapping from Building to FloorPlanDto to include name and desks
        TypeAdapterConfig<Building, FloorPlanDto>
            .NewConfig()
            .Map(dest => dest.buildingName, src => src.Name)
            .Map(dest => dest.FloorPlanDesks, src => src.Desks);

        // Configure mapping from Desk to DeskDto with custom status logic and reservation info
        TypeAdapterConfig<Desk, DeskDto>
            .NewConfig()
            .Map(dest => dest.Status, src =>
                src.IsInMaintenance ? DeskStatus.Maintenance :
                src.Reservations.Any() ? DeskStatus.Reserved :
                DeskStatus.Open)
            .AfterMapping((src, dest) =>
            {
                var activeReservation = src.Reservations.FirstOrDefault();
                if (activeReservation?.User != null)
                {
                    dest.ReservedByFullName = $"{activeReservation.User.Name} {activeReservation.User.Surname}";
                }
            });
    }
}
