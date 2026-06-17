using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Serilog;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure AppInsights telemetry with OpenTelemetry processors.
    /// In Application Insights 3.0, custom processors replace ITelemetryProcessor/ITelemetryInitializer.
    /// See https://github.com/microsoft/ApplicationInsights-dotnet/blob/main/BreakingChanges.md
    /// </summary>
    public static IApplicationBuilder UseAppInsightsTelemetry(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();

        if (configuration == null)
        {
            // No connection string configured -> Application Insights telemetry was not registered
            // (see AddAppInsightsTelemetry). The module is installed but telemetry is disabled,
            // so there is nothing to wire up here. This is a supported, non-error scenario.
            Log.Information("ApplicationInsights connection string is not configured, telemetry is disabled.");
            return app;
        }

        var aiOptions = app.ApplicationServices.GetRequiredService<IOptions<ApplicationInsightsOptions>>();
        var samplingOptions = aiOptions.Value.SamplingOptions;

        // Register DI-dependent OpenTelemetry processors via TracerProvider
        var tracerProvider = app.ApplicationServices.GetService<TracerProvider>();
        if (tracerProvider != null)
        {
            // SQL filtering processor (if configured)
            var sqlProcessor = app.ApplicationServices.GetService<IgnoreSqlTelemetryProcessor>();
            if (sqlProcessor != null)
            {
                tracerProvider.AddProcessor(sqlProcessor);
            }

            // User identity and cloud role enrichment
            var userProcessor = app.ApplicationServices.GetRequiredService<UserTelemetryInitializer>();
            tracerProvider.AddProcessor(userProcessor);
        }

        if (Log.Logger != null)
        {
            var processorType = samplingOptions.Processor == SamplingProcessor.Adaptive
                ? "Rate-limited (TracesPerSecond)"
                : "Fixed-ratio (SamplingRatio)";
            Log.Information("ApplicationInsights sampling configured: {ProcessorType}", processorType);
        }

        return app;
    }
}
