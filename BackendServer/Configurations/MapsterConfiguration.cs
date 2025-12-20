using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Mapster;

namespace DeskBookingService.Configurations;

public static class MapsterConfiguration
{
    public static void Configure()
    {
        // Configure mapping from Reservation to ReservationDto with TimeSpans
        TypeAdapterConfig<Reservation, ReservationDto>
            .NewConfig()
            .Map(dest => dest.TimeSpans, src => src.TimeSpans);

        // Configure mapping from ReservationTimeSpan to TimeSpanDto
        TypeAdapterConfig<ReservationTimeSpan, TimeSpanDto>
            .NewConfig();

        // Configure mapping from building to floorPlanDTO to include name and desks
        TypeAdapterConfig<Building, FloorPlanDto>
            .NewConfig()
            .Map(dest => dest.buildingName, src => src.Name)
            .Map(dest => dest.FloorPlanDesks, src => src.Desks);

        TypeAdapterConfig<ReservationTimeSpan, TimeSpanDto>
            .NewConfig()
            .Map(dest => dest.Status, src => TimeSpanType.Booked);

    }
}
