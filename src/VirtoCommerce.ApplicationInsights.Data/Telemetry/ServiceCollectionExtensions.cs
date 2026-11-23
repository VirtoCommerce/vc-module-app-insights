using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Monitor.OpenTelemetry.Profiler;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AspNetCore;
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
        var connectionString = GetConnectionString(configuration);
        if (string.IsNullOrEmpty(connectionString))
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

        // The following line enables Application Insights telemetry collection
        services.AddApplicationInsightsTelemetry();

        if (aiVirtoOptions.EnableProfiler)
        {
            // OpenTelemetry-based profiler, compatible with Application Insights SDK 3.x
            services.AddAzureMonitorProfiler();
        }

        // Report the public request URL (scheme/host from X-Forwarded headers)
        services.PostConfigureAll<AspNetCoreTraceInstrumentationOptions>(options =>
        {
            var enrich = options.EnrichWithHttpResponse;
            options.EnrichWithHttpResponse = (activity, response) =>
            {
                enrich?.Invoke(activity, response);
                RequestUrlEnricher.Enrich(activity, response.HttpContext.Request);
            };
        });

        // Always ignore SignalR telemetry
        services.AddIgnoreRequestTelemetry("/pushnotificationhub");

        // OpenTelemetry activity processors replaces ITelemetryProcessor/ITelemetryInitializer from 2.x
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                // Rules are added by this and other modules via AddIgnoreRequestTelemetry()
                tracing.AddProcessor(sp => new IgnoreRequestTelemetryProcessor(sp.GetRequiredService<IOptions<IgnoreRequestTelemetryOptions>>()));

                // Module ActivitySources are opt-in per deployment via "OpenTelemetry:Sources" (shared with the OpenTelemetry module)
                foreach (var source in GetConfigurationList(configuration, "OpenTelemetry:Sources"))
                {
                    tracing.AddSource(source);
                }
            })
            .WithMetrics(metrics =>
            {
                // Module Meters are opt-in per deployment via "OpenTelemetry:Meters" (shared with the OpenTelemetry module)
                foreach (var meter in GetConfigurationList(configuration, "OpenTelemetry:Meters"))
                {
                    metrics.AddMeter(meter);
                }
            });

        // Register processors that need DI via post-configure
        if (aiVirtoOptions.IgnoreSqlTelemetryOptions != null)
        {
            services.AddSingleton<IgnoreSqlTelemetryProcessor>();
        }
        services.AddSingleton<UserTelemetryInitializer>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        return configuration["ApplicationInsights:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
    }

    private static IEnumerable<string> GetConfigurationList(IConfiguration configuration, string key)
    {
        return configuration.GetSection(key)
            .GetChildren()
            // AddSource/AddMeter throw on an empty name, a stray empty config entry must not fail module init
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => x.Value);
    }
}
