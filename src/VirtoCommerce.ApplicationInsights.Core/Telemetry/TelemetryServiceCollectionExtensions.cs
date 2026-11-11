using Microsoft.Extensions.DependencyInjection;

namespace VirtoCommerce.ApplicationInsights.Core.Telemetry;

public static class TelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddIgnoreRequestTelemetry(this IServiceCollection services, string urlPath, string exceptWhenTag = null)
    {
        services.Configure<IgnoreRequestTelemetryOptions>(options => options.Rules.Add(new IgnoreRequestTelemetryRule
        {
            UrlPath = urlPath,
            ExceptWhenTag = exceptWhenTag,
        }));

        return services;
    }
}
