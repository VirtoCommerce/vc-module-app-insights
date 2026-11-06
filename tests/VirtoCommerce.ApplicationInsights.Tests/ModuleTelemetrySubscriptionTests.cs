using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using VirtoCommerce.ApplicationInsights.Data.Telemetry;
using Xunit;

namespace VirtoCommerce.ApplicationInsights.Tests;

[Trait("Category", "Unit")]
public class ModuleTelemetrySubscriptionTests
{
    [Fact]
    public async Task OnlyConfiguredSourcesAndMeters_AreSubscribed()
    {
        // Arrange
        var prefix = $"VirtoCommerce.Test{Guid.NewGuid():N}";
        var optedIn = $"{prefix}.OptedIn";
        var notOptedIn = $"{prefix}.NotOptedIn";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=e1821750-fec9-45ea-b323-06d2b14cb00e;IngestionEndpoint=https://localhost/",
                ["OpenTelemetry:Sources:0"] = optedIn,
                ["OpenTelemetry:Sources:1"] = "",
                ["OpenTelemetry:Meters:0"] = optedIn,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddAppInsightsTelemetry(configuration);

        await using var serviceProvider = services.BuildServiceProvider();

        // Act
        _ = serviceProvider.GetRequiredService<TracerProvider>();
        _ = serviceProvider.GetRequiredService<MeterProvider>();

        using var optedInSource = new ActivitySource(optedIn);
        using var notOptedInSource = new ActivitySource(notOptedIn);
        using var optedInMeter = new Meter(optedIn);
        using var notOptedInMeter = new Meter(notOptedIn);
        var optedInCounter = optedInMeter.CreateCounter<long>("count");
        var notOptedInCounter = notOptedInMeter.CreateCounter<long>("count");

        // Assert
        Assert.True(optedInSource.HasListeners());
        Assert.False(notOptedInSource.HasListeners());
        Assert.True(optedInCounter.Enabled);
        Assert.False(notOptedInCounter.Enabled);
    }
}
