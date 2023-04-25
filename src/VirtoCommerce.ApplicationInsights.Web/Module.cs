using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtoCommerce.ApplicationInsights.Data.Services;
using VirtoCommerce.ApplicationInsights.Data.Telemetry;
using VirtoCommerce.Platform.Core.Logger;
using VirtoCommerce.Platform.Core.Modularity;


namespace VirtoCommerce.ApplicationInsights.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }

    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddAppInsightsTelemetry(Configuration);

        serviceCollection.AddTransient<ILoggerConfigurationService, ApplicationInsightsLoggerConfiguration>();
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var enviroment = appBuilder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        if (enviroment.IsDevelopment())
        {
#if DEBUG
            TelemetryDebugWriter.IsTracingDisabled = true;
#endif
        }

        appBuilder.UseAppInsightsTelemetry();
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
