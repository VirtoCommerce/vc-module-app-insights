using System.Diagnostics;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

/// <summary>
/// OpenTelemetry processor which excludes dependency SQL queries matching configured substrings.
/// </summary>
public class IgnoreSqlTelemetryProcessor : BaseProcessor<Activity>
{
    private readonly ApplicationInsightsOptions _options;

    /// <summary>
    /// Just for looking at processor options from outside (logging, etc...)
    /// </summary>
    public IgnoreSqlTelemetryOptions Options => _options.IgnoreSqlTelemetryOptions;

    public IgnoreSqlTelemetryProcessor(IOptions<ApplicationInsightsOptions> options)
    {
        _options = options.Value;
    }

    public override void OnEnd(Activity activity)
    {
        if (_options.IgnoreSqlTelemetryOptions == null)
        {
            return;
        }

        var dbSystem = activity?.GetTagItem("db.system")?.ToString();
        if (dbSystem is not ("mssql" or "microsoft.sql_server"))
        {
            return;
        }

        var statement = activity.GetTagItem("db.statement")?.ToString();
        if (!statement.IsNullOrEmpty())
        {
            foreach (var substring in _options.IgnoreSqlTelemetryOptions.QueryIgnoreSubstrings)
            {
                if (statement.Contains(substring))
                {
                    activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
                    return;
                }
            }
        }
    }
}
