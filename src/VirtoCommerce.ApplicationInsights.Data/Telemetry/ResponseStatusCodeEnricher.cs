using System;
using System.Diagnostics;
using System.Globalization;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

/// <summary>
/// Replaces OpenTelemetry's 'http.response.status_code' with 'virtocommerce.response.status_code' value to change the the actual HTTP response.
/// </summary>
public static class ResponseStatusCodeEnricher
{
    private const string ResponseStatusCodeTag = "http.response.status_code";

    public static void Enrich(Activity activity)
    {
        var value = activity.GetTagItem(TelemetryTags.ResponseStatusCodeOverride);
        if (value == null)
        {
            return;
        }

        if (int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.None, CultureInfo.InvariantCulture, out var statusCode))
        {
            activity.SetTag(ResponseStatusCodeTag, statusCode);
        }

        // remove the tag so it doesn't end up in custom dimensions
        activity.SetTag(TelemetryTags.ResponseStatusCodeOverride, null);
    }
}
