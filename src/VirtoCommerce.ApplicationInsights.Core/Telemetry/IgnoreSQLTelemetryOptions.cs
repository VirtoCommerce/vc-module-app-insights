using System;

namespace VirtoCommerce.ApplicationInsights.Core.Telemetry;

public class IgnoreSqlTelemetryOptions
{
    public string[] QueryIgnoreSubstrings { get; set; } = Array.Empty<string>();
}
