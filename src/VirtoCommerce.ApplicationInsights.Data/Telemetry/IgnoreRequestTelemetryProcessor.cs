using System.Diagnostics;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

/// <summary>
/// OpenTelemetry processor which excludes request activities matching the rules.
/// </summary>
public class IgnoreRequestTelemetryProcessor : BaseProcessor<Activity>
{
    private readonly IOptions<IgnoreRequestTelemetryOptions> _options;

    public IgnoreRequestTelemetryProcessor(IOptions<IgnoreRequestTelemetryOptions> options)
    {
        _options = options;
    }

    public override void OnEnd(Activity activity)
    {
        if (activity == null)
        {
            return;
        }

        var urlPath = activity.GetTagItem("url.path")?.ToString();
        if (urlPath.IsNullOrEmpty())
        {
            return;
        }

        foreach (var rule in _options.Value.Rules)
        {
            if (!rule.UrlPath.IsNullOrEmpty() &&
                urlPath.ContainsIgnoreCase(rule.UrlPath) &&
                (rule.ExceptWhenTag.IsNullOrEmpty() || activity.GetTagItem(rule.ExceptWhenTag) == null))
            {
                // Clearing the 'Recorded' flag prevents the activity from being exported
                activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
                return;
            }
        }
    }
}
