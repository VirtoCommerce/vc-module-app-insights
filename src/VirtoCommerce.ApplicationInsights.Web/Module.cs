using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.ApplicationInsights.Core;
using VirtoCommerce.ApplicationInsights.Data.Telemetry;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
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

        appBuilder.UseAppInsightsTelemetry();

        // Extract InstrumentationKey from ConnectionString (InstrumentationKey property removed in 3.x)
        // The key is still needed by VueJS frontend applications
        var configuration = appBuilder.ApplicationServices.GetService<TelemetryConfiguration>();
        if (configuration?.ConnectionString != null)
        {
            var instrumentationKey = GetInstrumentationKey(configuration);

            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                ModuleConstants.Settings.General.InstrumentationKey.DefaultValue = instrumentationKey;
            }
        }

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

        // Register store level settings
        settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.StoreLevelSettings, "Store");

        // Register permissions
        var permissionsRegistrar = serviceProvider.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions
            .Select(x => new Permission { ModuleId = ModuleInfo.Id, GroupName = "ApplicationInsights", Name = x })
            .ToArray());
    }

    private static string GetInstrumentationKey(TelemetryConfiguration configuration)
    {
        return configuration.ConnectionString
                        .Split(';')
                        .Select(part => part.Split('=', 2))
                        .Where(kv => kv.Length == 2 && kv[0].Equals("InstrumentationKey", System.StringComparison.OrdinalIgnoreCase))
                        .Select(kv => kv[1])
                        .FirstOrDefault();
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
