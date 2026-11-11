using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using VirtoCommerce.ApplicationInsights.Data.Telemetry;
using Xunit;

namespace VirtoCommerce.ApplicationInsights.Tests;

[Trait("Category", "Unit")]
public class RequestUrlTelemetryTests
{
    [Fact]
    public async Task ServerActivity_HasPublicUrl_BehindProxy()
    {
        // Arrange
        var exportedActivities = new List<Activity>();

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=e1821750-fec9-45ea-b323-06d2b14cb00e;IngestionEndpoint=https://localhost/",
            ["VirtoCommerce:ApplicationInsights:SamplingOptions:Processor"] = "Fixed",
            ["VirtoCommerce:ApplicationInsights:SamplingOptions:Fixed:SamplingPercentage"] = "100",
        });
        builder.Services.AddAppInsightsTelemetry(builder.Configuration);
        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddInMemoryExporter(exportedActivities));
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        });

        await using var app = builder.Build();
        app.UseForwardedHeaders();
        app.Run(context =>
        {
            // Same as GraphQLHttpMiddlewareWithLogs in xAPI module
            Activity.Current?.SetTag("http.route", "graphql/GetMenu");
            return Task.CompletedTask;
        });
        await app.StartAsync(TestContext.Current.CancellationToken);

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{app.Urls.First()}/graphql");
        request.Headers.Add("X-Forwarded-Proto", "https");
        request.Headers.Add("X-Forwarded-Host", "shop.example.com");

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        await app.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var activity = Assert.Single(exportedActivities, x => x.Kind == ActivityKind.Server);
        Assert.Equal("https", activity.GetTagItem("url.scheme"));
        Assert.Equal("shop.example.com", activity.GetTagItem("server.address"));
        Assert.Null(activity.GetTagItem("server.port"));
        Assert.Equal("/graphql", activity.GetTagItem("url.path"));
        Assert.Equal("graphql/GetMenu", activity.GetTagItem("http.route"));
    }
}
