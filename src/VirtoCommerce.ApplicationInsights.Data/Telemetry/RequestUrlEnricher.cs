using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

/// <summary>
/// OpenTelemetry captures 'url.scheme' on request start, before UseForwardedHeaders() applies X-Forwarded-Proto/X-Forwarded-Host
/// Behind a proxy the request URL would be reported as http://...
/// This reports the public request URL (scheme/host from X-Forwarded headers), as in Application Insights SDK 2.x.
/// </summary>
public static class RequestUrlEnricher
{
    public static void Enrich(Activity activity, HttpRequest request)
    {
        activity.SetTag("url.scheme", request.Scheme);
        activity.SetTag("server.address", request.Host.Host);
        // null removes the tag, so a stale internal port doesn't end up in the public URL
        activity.SetTag("server.port", request.Host.Port);
    }
}
