using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;

namespace VirtoCommerce.ApplicationInsights.Data.Telemetry;

/// <summary>
/// An OpenTelemetry processor that enriches activities with user identity and cloud role context.
/// </summary>
public class UserTelemetryInitializer : BaseProcessor<Activity>
{
    private readonly ApplicationInsightsOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTelemetryInitializer(IHttpContextAccessor httpContextAccessor, IOptions<ApplicationInsightsOptions> options)
    {
        _options = options?.Value ?? new ApplicationInsightsOptions();
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnEnd(Activity activity)
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

        if (userId != null)
        {
            activity?.SetTag("enduser.id", userId);
        }
        if (!string.IsNullOrEmpty(_options.RoleName))
        {
            activity?.SetTag("cloud.role_name", _options.RoleName);
        }
        if (!string.IsNullOrEmpty(_options.RoleInstance))
        {
            activity?.SetTag("cloud.role_instance", _options.RoleInstance);
        }
    }
}
