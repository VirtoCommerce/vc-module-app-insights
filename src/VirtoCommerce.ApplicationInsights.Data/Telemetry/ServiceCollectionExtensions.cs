using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;


namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Charge AppInsights options and add the telemetry
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddAppInsightsTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var aiVirtoOptionsSection = configuration.GetSection("VirtoCommerce:ApplicationInsights");
        var aiVirtoOptions = aiVirtoOptionsSection.Get<ApplicationInsightsOptions>() ?? new ApplicationInsightsOptions();

        // Configure sampling via ApplicationInsightsServiceOptions (Application Insights 3.0).
        // In 3.0, adaptive sampling is replaced by rate-limited sampling (TracesPerSecond),
        // and fixed sampling uses SamplingRatio (0.0-1.0) instead of SamplingPercentage (0-100).
        // See https://learn.microsoft.com/en-us/azure/azure-monitor/app/migrate-to-opentelemetry
        services.Configure<ApplicationInsightsServiceOptions>(o =>
        {
            if (aiVirtoOptions.SamplingOptions.Processor == SamplingProcessor.Adaptive)
            {
                o.TracesPerSecond = aiVirtoOptions.SamplingOptions.Adaptive.MaxTelemetryItemsPerSecond;
            }
            else
            {
                o.SamplingRatio = (float)(aiVirtoOptions.SamplingOptions.Fixed.SamplingPercentage / 100.0);
            }
        });

        // The following line enables Application Insights telemetry collection.
        services.AddApplicationInsightsTelemetry();

        if (aiVirtoOptions.EnableProfiler)
        {
            services.AddServiceProfiler();
        }

        // Charge ApplicationInsights options to enable custom configuration
        services.AddOptions<ApplicationInsightsOptions>().Bind(aiVirtoOptionsSection);

        // Register OpenTelemetry activity processors
        // (replacing ITelemetryProcessor/ITelemetryInitializer from 2.x)
        // See https://github.com/microsoft/ApplicationInsights-dotnet/blob/main/BreakingChanges.md
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                // Always ignore SignalR telemetry
                tracing.AddProcessor(new IgnoreSignalRTelemetryProcessor());

                // Configurable activity ignore
                tracing.AddProcessor<IgnoreActivitiesByNameProcessor>();
            });

        // Register processors that need DI via post-configure
        if (aiVirtoOptions.IgnoreSqlTelemetryOptions != null)
        {
            services.AddSingleton<IgnoreSqlTelemetryProcessor>();
        }
        services.AddSingleton<UserTelemetryInitializer>();

        return services;
    }
}
