using System;
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
    public static IServiceCollection AddAppInsightsTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var aiVirtoOptionsSection = configuration.GetSection("VirtoCommerce:ApplicationInsights");
        var aiVirtoOptions = aiVirtoOptionsSection.Get<ApplicationInsightsOptions>() ?? new ApplicationInsightsOptions();

        services.AddOptions<ApplicationInsightsOptions>().Bind(aiVirtoOptionsSection);

        // Skip telemetry registration entirely when the connection string is absent
        if (!HasConnectionString(configuration))
        {
            return services;
        }

        // Sampling configuration
        services.Configure<ApplicationInsightsServiceOptions>(o =>
        {
            if (aiVirtoOptions.SamplingOptions.Processor == SamplingProcessor.Adaptive)
            {
                o.TracesPerSecond = aiVirtoOptions.SamplingOptions.Adaptive.MaxTelemetryItemsPerSecond;
            }
            else
            {
                // Fixed sampling uses SamplingRatio(0.0 - 1.0) instead of SamplingPercentage(0 - 100)
                o.SamplingRatio = (float)(aiVirtoOptions.SamplingOptions.Fixed.SamplingPercentage / 100.0);
            }
        });

        // The following line enables Application Insights telemetry collection.
        services.AddApplicationInsightsTelemetry();

        if (aiVirtoOptions.EnableProfiler)
        {
            services.AddServiceProfiler();
        }

        // OpenTelemetry activity processors replaces ITelemetryProcessor/ITelemetryInitializer from 2.x
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                // Always ignore SignalR telemetry
                tracing.AddProcessor(new IgnoreSignalRTelemetryProcessor());
                tracing.AddSource("VirtoCommerce.*");
            })
            .WithMetrics(meter =>
            {
                meter.AddMeter("VirtoCommerce.*");
            });

        // Register processors that need DI via post-configure
        if (aiVirtoOptions.IgnoreSqlTelemetryOptions != null)
        {
            services.AddSingleton<IgnoreSqlTelemetryProcessor>();
        }
        services.AddSingleton<UserTelemetryInitializer>();

        return services;
    }

    private static bool HasConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration["ApplicationInsights:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        return !string.IsNullOrEmpty(connectionString);
    }
}
