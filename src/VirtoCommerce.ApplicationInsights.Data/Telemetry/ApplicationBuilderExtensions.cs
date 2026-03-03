using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Serilog;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;
using VirtoCommerce.Platform.Core.Modularity.Exceptions;

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
            throw new ModuleInitializeException("TelemetryConfiguration is not initialized. Please make sure that another module doesn't override AppInsightsTelemetry.");
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
