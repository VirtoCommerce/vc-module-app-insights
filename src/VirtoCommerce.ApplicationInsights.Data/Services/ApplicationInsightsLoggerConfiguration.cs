using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using VirtoCommerce.Platform.Core.Logger;

namespace VirtoCommerce.ApplicationInsights.Data.Services
{
    public class ApplicationInsightsLoggerConfiguration : ILoggerConfigurationService
    {
        private readonly TelemetryConfiguration _configuration;

        public ApplicationInsightsLoggerConfiguration(TelemetryConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.WriteTo.ApplicationInsights(telemetryConfiguration: _configuration,
            telemetryConverter: TelemetryConverter.Traces,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);
        }
    }
}
