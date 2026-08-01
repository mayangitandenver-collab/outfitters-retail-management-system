using Microsoft.Extensions.DependencyInjection;

namespace Outfitters.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
