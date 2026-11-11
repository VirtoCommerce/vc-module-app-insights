using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.ApplicationInsights.Core.Telemetry;
using VirtoCommerce.ApplicationInsights.Data.Telemetry;
using Xunit;

namespace VirtoCommerce.ApplicationInsights.Tests;

[Trait("Category", "Unit")]
public class IgnoreRequestTelemetryProcessorTests
{
    [Theory]
    [InlineData("/pushnotificationhub/negotiate", null, false)]
    [InlineData("/PushNotificationHub", null, false)]
    [InlineData("/graphql", null, false)]
    [InlineData("/graphql", "GraphQL", true)]
    [InlineData("/api/catalog/products", null, true)]
    public void OnEnd_AppliesRules(string urlPath, string graphQlTag, bool expectedRecorded)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddIgnoreRequestTelemetry("/pushnotificationhub");
        services.AddIgnoreRequestTelemetry("/graphql", exceptWhenTag: "graphql.type");
        using var serviceProvider = services.BuildServiceProvider();

        var processor = new IgnoreRequestTelemetryProcessor(serviceProvider.GetRequiredService<IOptions<IgnoreRequestTelemetryOptions>>());

        using var activity = new Activity("test");
        activity.ActivityTraceFlags = ActivityTraceFlags.Recorded;
        activity.SetTag("url.path", urlPath);
        if (graphQlTag != null)
        {
            activity.SetTag("graphql.type", graphQlTag);
        }

        // Act
        processor.OnEnd(activity);

        // Assert
        Assert.Equal(expectedRecorded, activity.Recorded);
    }

    [Fact]
    public void OnEnd_NoUrlPath_KeepsActivity()
    {
        // Arrange
        var options = Options.Create(new IgnoreRequestTelemetryOptions());
        options.Value.Rules.Add(new IgnoreRequestTelemetryRule { UrlPath = "/graphql" });
        var processor = new IgnoreRequestTelemetryProcessor(options);

        using var activity = new Activity("test");
        activity.ActivityTraceFlags = ActivityTraceFlags.Recorded;

        // Act
        processor.OnEnd(activity);

        // Assert
        Assert.True(activity.Recorded);
    }
}
