using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLogFlake.Models.Options;
using NLogFlake.Services;

namespace NLogFlake;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddLogFlake(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<LogFlakeOptions>(configuration.GetSection(LogFlakeOptions.SectionName))
            .AddOptionsWithValidateOnStart<LogFlakeOptions, LogFlakeOptionsValidator>();

        services.AddHttpClient();

        services.AddScoped<ICorrelationService, CorrelationService>();

        services.AddSingleton<ILogFlake, LogFlake>();
        services.AddSingleton<ILogFlakeService, LogFlakeService>();

        return services;
    }
}
