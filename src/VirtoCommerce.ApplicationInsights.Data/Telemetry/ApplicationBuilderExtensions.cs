using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure AppInsights sampling accordingly to
    /// https://docs.microsoft.com/en-us/azure/azure-monitor/app/sampling#configure-sampling-settings
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAppInsightsTelemetry(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>() ??
            throw new Exception("TelemetryConfiguration is not initialized. Please make sure that another module doesn't override AppInsightsTelemetry.");

        var samplingOptions = app.ApplicationServices.GetRequiredService<IOptions<ApplicationInsightsOptions>>().Value.SamplingOptions;
        var builder = configuration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
        if (samplingOptions.Processor == SamplingProcessor.Adaptive)
        {
            // Using adaptive rate sampling
            builder.Use(x =>
                new AdaptiveSamplingTelemetryProcessor(samplingOptions.Adaptive, null, x)
                {
                    ExcludedTypes = samplingOptions.ExcludedTypes,
                    IncludedTypes = samplingOptions.IncludedTypes
                }
            );
        }
        else
        {
            // Using fixed rate sampling
            builder.Use(x =>
                new SamplingTelemetryProcessor(x)
                {
                    SamplingPercentage = samplingOptions.Fixed.SamplingPercentage,
                    ExcludedTypes = samplingOptions.ExcludedTypes,
                    IncludedTypes = samplingOptions.IncludedTypes
                }
            );
        }

        builder.Build();

        var telemetryProcessorsLogInfo = new Dictionary<string, ITelemetryProcessor>();
        foreach (var processor in configuration.DefaultTelemetrySink.TelemetryProcessors)
        {
            telemetryProcessorsLogInfo.Add(processor.GetType().Name, processor);
        }

        if (Log.Logger != null)
        {
            var telemetryProcessors = JsonConvert.SerializeObject(telemetryProcessorsLogInfo, Formatting.Indented);
            Log.Information($"ApplicationInsights telemetry processors list and settings:{Environment.NewLine}{telemetryProcessors}{Environment.NewLine}");
        }

        return app;
    }
}
