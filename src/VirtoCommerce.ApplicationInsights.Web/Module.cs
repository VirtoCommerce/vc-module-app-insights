using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtoCommerce.ApplicationInsights.Core;
using VirtoCommerce.ApplicationInsights.Data.Telemetry;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;


namespace VirtoCommerce.ApplicationInsights.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }

    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddAppInsightsTelemetry(Configuration);
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var serviceProvider = appBuilder.ApplicationServices;

        var enviroment = appBuilder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        if (enviroment.IsDevelopment())
        {
#if DEBUG
            TelemetryDebugWriter.IsTracingDisabled = true;
#endif
        }

        appBuilder.UseAppInsightsTelemetry();

        // Resolve Default InstrumentationKey from TelemetryConfiguration
        var configuration = appBuilder.ApplicationServices.GetService<TelemetryConfiguration>();
        if (configuration != null)
        {
            ModuleConstants.Settings.General.InstrumentationKey.DefaultValue = configuration.InstrumentationKey;
        }

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

        // Register store level settings
        settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.StoreLevelSettings, "Store");
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
