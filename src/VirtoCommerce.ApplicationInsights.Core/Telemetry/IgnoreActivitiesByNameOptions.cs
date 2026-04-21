using System;
using System.Collections.Generic;

namespace VirtoCommerce.ApplicationInsights.Core.Telemetry;

public class IgnoreActivitiesByNameOptions
{
    public HashSet<string> IgnoredDisplayNames { get; } = new(StringComparer.OrdinalIgnoreCase);
}
