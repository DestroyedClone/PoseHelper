using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.Runtime.CompilerServices;

namespace ShareYourMoney
{
    public static class ModCompat
    {
        public static void Initialize()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                RiskOfOptionsCompat();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RiskOfOptionsCompat()
        {
            RiskOfOptions.ModSettingsManager.SetModDescription("Allows players to drop their dosh.", "com.DestroyedClone.DoshDrop", "Dosh Drop");

            ModSettingsManager.AddOption(new KeyBindOption(DoshDropPlugin.cfgCDropKey));
            ModSettingsManager.AddOption(new CheckBoxOption(DoshDropPlugin.cfgCLanguageSwap));

            ModSettingsManager.AddOption(new StepSliderOption(DoshDropPlugin.cfgSPercentToDrop, new StepSliderConfig()
            {
                min = 0.01f,
                max = 1f,
                increment = 0.01f
            }));
            //ModSettingsManager.AddOption(new CheckBoxOption(DoshDropPlugin.cfgSPerformanceMode));
            ModSettingsManager.AddOption(new CheckBoxOption(DoshDropPlugin.cfgSPreventModUseOnStageEnd));
            ModSettingsManager.AddOption(new CheckBoxOption(DoshDropPlugin.cfgSRefundOnStageEnd));

            DoshDropPlugin.cfgCLanguageSwap.SettingChanged += CfgCLanguageSwap_SettingChanged;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void CfgCLanguageSwap_SettingChanged(object sender, EventArgs e)
        {
            DoshDropPlugin.SetupLanguage();
        }
    }
}