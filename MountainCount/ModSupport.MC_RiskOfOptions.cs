using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System.Runtime.CompilerServices;
using static MountainCount.Config;

namespace MountainCount
{
    public static partial class ModSupport
    {
        public static class MC_RiskOfOptions
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void Initialize()
            {
                RiskOfOptions.ModSettingsManager.SetModDescription("Announces the amount of teleporter shrines used.", "com.DestroyedClone.MountainCount", "Mountain Count");

                ModSettingsManager.AddOption(new StringInputFieldOption(cfgPrintOnTeleporterEnd));
                ModSettingsManager.AddOption(new CheckBoxOption(cfgEditLanguage));
                ModSettingsManager.AddOption(new CheckBoxOption(cfgAddChatMessage));

                RoR2.Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
            }

            private static void Language_onCurrentLanguageChanged()
            {
                ModSettingsManager.AddOption(new CheckBoxOption(cfgExpandedInfo, new CheckBoxConfig()
                {
                    description = GetExpandedInfoDescription()
                }));
                //cant modify collection, so... first language only..
                RoR2.Language.onCurrentLanguageChanged -= Language_onCurrentLanguageChanged;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static string GetExpandedInfoDescription()
            {
                var stringBuilder = HG.StringBuilderPool.RentStringBuilder();
                void AppendResolvedTokenAddExtension(string text)
                {
                    stringBuilder.AppendLine(RoR2.Language.GetString("DEFAULT_SKIN") + ": " + RoR2.Language.GetString(text));
                    stringBuilder.AppendLine(RoR2.Language.GetString("NEW_UNLOCKABLE") + ": " + RoR2.Language.GetString(text + "_EXPANDED"));
                }

                AppendResolvedTokenAddExtension(MountainCountPlugin.shrineMountain.SayCountToken);

                if (ModSupport.modloaded_ExtraChallengeShrines)
                {
                    AppendResolvedTokenAddExtension(ModSupport.MC_ExtraChallengeShrines.shrineCrown.SayCountToken);
                    AppendResolvedTokenAddExtension(ModSupport.MC_ExtraChallengeShrines.shrineEye.SayCountToken);
                    AppendResolvedTokenAddExtension(ModSupport.MC_ExtraChallengeShrines.shrineRock.SayCountToken);
                }

                var output = stringBuilder.ToString();
                HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
                return output;
            }
        }
    }
}