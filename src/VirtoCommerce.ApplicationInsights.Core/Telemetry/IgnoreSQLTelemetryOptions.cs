namespace VirtoCommerce.ApplicationInsights.Core.Telemetry;

public class IgnoreSqlTelemetryOptions
{
    public string[] QueryIgnoreSubstrings { get; set; } = new string[] { };
}
