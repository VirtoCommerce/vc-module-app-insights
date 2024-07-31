using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

public class HeaderTelemetryInitializer(IHttpContextAccessor httpContextAccessor) : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        var context = httpContextAccessor.HttpContext;

        if (telemetry is not RequestTelemetry requestTelemetry ||
            context?.Request.Path.Value != "/signin-oidc")
        {
            return;
        }

        AddHeaders(requestTelemetry, context.Request.Headers, "Request-Header-");
        AddHeaders(requestTelemetry, context.Response.Headers, "Response-Header-");
    }

    private static void AddHeaders(RequestTelemetry telemetry, IHeaderDictionary headers, string prefix)
    {
        foreach (var (key, value) in headers.OrderBy(x => x.Key))
        {
            telemetry.Properties[$"{prefix}{key}"] = value.ToString();
        }
    }
}
