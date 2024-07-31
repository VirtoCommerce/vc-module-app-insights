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

        foreach (var header in context.Request.Headers)
        {
            requestTelemetry.Properties[$"Header-{header.Key}"] = header.Value.ToString();
        }
    }
}
