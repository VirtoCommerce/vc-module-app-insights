using System;
using System.Diagnostics;
using OpenTelemetry;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

/// <summary>
/// OpenTelemetry processor which excludes all SignalR requests from telemetry.
/// </summary>
public class IgnoreSignalRTelemetryProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        var urlPath = activity?.GetTagItem("url.path")?.ToString();
        if (urlPath != null && urlPath.Contains("/pushnotificationhub", StringComparison.OrdinalIgnoreCase))
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }
}
