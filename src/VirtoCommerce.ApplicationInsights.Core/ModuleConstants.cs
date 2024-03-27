using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ApplicationInsights.Core;

public static class ModuleConstants
{
    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor EnableTracking { get; } = new SettingDescriptor
            {
                Name = "ApplicationInsights.EnableTracking",
                GroupName = "Application Insights",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
                IsPublic = true,
            };

            public static SettingDescriptor InstrumentationKey { get; } = new SettingDescriptor
            {
                Name = "ApplicationInsights.InstrumentationKey",
                GroupName = "Application Insights",
                ValueType = SettingValueType.ShortText,
                IsPublic = true,
            };

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    yield return EnableTracking;
                    yield return InstrumentationKey;
                }
            }

        }

        public static IEnumerable<SettingDescriptor> StoreLevelSettings
        {
            get
            {
                yield return General.EnableTracking;
                yield return General.InstrumentationKey;
            }
        }

    }
}
