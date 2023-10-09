using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace HealthbarImmune
{
    public static class ModSupport_RiskOfOptions
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Initialize()
        {
            RiskOfOptions.ModSettingsManager.SetModDescription("Makes the healthbar look like how it does in ROR1 when immune", "com.DestroyedClone.HealthbarImmune", "Healthbar Immune");

            ModSettingsManager.AddOption(new StringInputFieldOption(HealthbarImmunePlugin.cfgCharacterBlacklist, new InputFieldConfig()
            {
                submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit
            }));
            HealthbarImmunePlugin.cfgCharacterBlacklist.SettingChanged += CfgCharacterBlacklist_SettingChanged;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void CfgCharacterBlacklist_SettingChanged(object sender, EventArgs e)
        {
            string replacement = HealthbarImmunePlugin.cfgCharacterBlacklist.Value.Trim();
            HealthbarImmunePlugin.cfgCharacterBlacklist.Value = replacement;
            HealthbarImmunePlugin.SetupDictionary();
        }
    }
}
