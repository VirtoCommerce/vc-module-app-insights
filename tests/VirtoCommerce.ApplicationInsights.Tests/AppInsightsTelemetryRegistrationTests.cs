using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtoCommerce.ApplicationInsights.Data.Telemetry;
using Xunit;

namespace VirtoCommerce.ApplicationInsights.Tests;

[Trait("Category", "Unit")]
public class AppInsightsTelemetryRegistrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddAppInsightsTelemetry_HostedServicesCanBeResolved(bool enableProfiler)
    {
        // Arrange
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=e1821750-fec9-45ea-b323-06d2b14cb00e;IngestionEndpoint=https://localhost/",
            ["VirtoCommerce:ApplicationInsights:EnableProfiler"] = enableProfiler.ToString(),
        }).Build();

        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddConfiguration(configuration);
        builder.Services.AddAppInsightsTelemetry(builder.Configuration);
        using var host = builder.Build();

        // Act
        // Host.StartAsync() resolves all hosted services first: the 2.x profiler failed right here with TypeLoadException on Application Insights SDK 3.x
        var hostedServices = host.Services.GetServices<IHostedService>();

        // Assert
        Assert.NotEmpty(hostedServices);
    }
}
