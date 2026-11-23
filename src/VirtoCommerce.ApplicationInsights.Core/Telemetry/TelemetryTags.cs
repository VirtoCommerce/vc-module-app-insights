namespace VirtoCommerce.ApplicationInsights.Core.Telemetry;

public static class TelemetryTags
{
    /// <summary>
    /// Set this tag on the server request activity to report a response code other than the actual HTTP status code.
    /// </summary>
    public const string ResponseStatusCodeOverride = "virtocommerce.response.status_code";
}
