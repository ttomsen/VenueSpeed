using Microsoft.Extensions.DependencyInjection;
using VenueSpeed.Core.Interfaces;
using VenueSpeed.Data.Repositories;

namespace VenueSpeed.Data.Extensions;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventBracketRepository, EventBracketRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
        services.AddScoped<IDiscoveryRepository, DiscoveryRepository>();
        return services;
    }
}
