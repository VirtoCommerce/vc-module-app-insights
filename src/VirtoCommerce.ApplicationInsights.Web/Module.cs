using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.ApplicationInsights.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }

    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        // Register services
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        // Nothing to do here
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
