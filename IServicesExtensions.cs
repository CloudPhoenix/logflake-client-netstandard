using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLogFlake.Models.Options;
using NLogFlake.Services;

namespace NLogFlake;

public static class IServicesExtensions
{
    public static IServiceCollection AddLogFlake(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<LogFlakeOptions>(configuration.GetSection(LogFlakeOptions.SectionName))
            .AddOptionsWithValidateOnStart<LogFlakeOptions, LogFlakeOptionsValidator>();

        _ = services.Configure<LogFlakeSettingsOptions>(configuration.GetSection(LogFlakeSettingsOptions.SectionName))
            .AddOptionsWithValidateOnStart<LogFlakeSettingsOptions, LogFlakeSettingsOptionsValidator>();

        services.AddHttpClient();

        services.AddScoped<ICorrelationService, CorrelationService>();

        services.AddSingleton<ILogFlake, LogFlake>();
        services.AddSingleton<ILogFlakeService, LogFlakeService>();

        return services;
    }
}
