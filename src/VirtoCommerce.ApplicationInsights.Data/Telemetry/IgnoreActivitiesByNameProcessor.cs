using System.Diagnostics;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

/// <summary>
/// OpenTelemetry processor that suppresses activities whose <see cref="Activity.DisplayName"/>
/// matches one of the names configured via <see cref="IgnoreActivitiesByNameOptions"/>.
/// Clearing the <see cref="ActivityTraceFlags.Recorded"/> flag prevents the activity
/// from being exported (e.g. to Application Insights) while still allowing it
/// to participate in in-process correlation.
/// </summary>
public class IgnoreActivitiesByNameProcessor : BaseProcessor<Activity>
{
    private readonly IgnoreActivitiesByNameOptions _options;

    public IgnoreActivitiesByNameProcessor(IOptions<IgnoreActivitiesByNameOptions> options)
    {
        _options = options.Value;
    }

    public override void OnEnd(Activity activity)
    {
        if (_options.IgnoredDisplayNames.Contains(activity.DisplayName))
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }
}
