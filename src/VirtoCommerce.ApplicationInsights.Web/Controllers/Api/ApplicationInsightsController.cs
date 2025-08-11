using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.ApplicationInsights.Core;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.PowerBiReports.Web.Controllers.Api
{
    [Route("api/application-insights")]
    public class ApplicationInsightsController(ISettingsManager settingsManager) : Controller
    {
        private readonly ISettingsManager _settingsManager = settingsManager;

        [HttpGet]
        [Route("redirect")]
        [Authorize(ModuleConstants.Security.Permissions.Access)]
        public ActionResult Redirect()
        {
            var redirectTo = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.ApplicationInsightsUrl);

            if (string.IsNullOrEmpty(redirectTo))
            {
                return NotFound("ApplicationInsightsUrl is not configured in the Platfortm Settings");
            }

            return Redirect(redirectTo);
        }

    }
}
