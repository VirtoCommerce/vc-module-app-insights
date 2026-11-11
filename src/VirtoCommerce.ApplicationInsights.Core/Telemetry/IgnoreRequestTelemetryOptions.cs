using System.Collections.Generic;

namespace VirtoCommerce.ApplicationInsights.Core.Telemetry;

public class IgnoreRequestTelemetryOptions
{
    public IList<IgnoreRequestTelemetryRule> Rules { get; set; } = [];
}

public class IgnoreRequestTelemetryRule
{
    /// <summary>
    /// Activity is ignored when its 'url.path' tag contains this value
    /// </summary>
    public string UrlPath { get; set; }

    /// <summary>
    /// Optional. If activity has this tag then it's not ignored
    /// </summary>
    public string ExceptWhenTag { get; set; }
}
