namespace VirtoCommerce.ApplicationInsights.Core.Telemetry;

/// <summary>
/// Settings for adaptive (rate-limited) sampling.
/// In Application Insights 3.0, adaptive sampling is replaced by rate-limited sampling (TracesPerSecond).
/// </summary>
public class AdaptiveSamplingSettings
{
    /// <summary>
    /// Maximum number of telemetry items allowed per second.
    /// Maps to ApplicationInsightsServiceOptions.TracesPerSecond in Application Insights 3.0.
    /// </summary>
    public double MaxTelemetryItemsPerSecond { get; set; } = 5;
}
