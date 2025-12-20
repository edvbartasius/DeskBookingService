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
    }
}
