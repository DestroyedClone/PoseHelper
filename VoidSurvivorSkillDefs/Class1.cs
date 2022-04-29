using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using System.Security;
using System.Security.Permissions;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;
using System;
using BepInEx.Logging;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace VoidFiendSkillTokens
{
    [BepInPlugin("com.DestroyedClone.VoidFiendSkillTokens", "Void Fiend Skill Tokens", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class VFSTMain : BaseUnityPlugin
    {
        public static bool cfgCustomTokens = false;

        //public static SkillDef drownSkillDef;
        public static SkillDef corruptedDrownSkillDef;

        //public static SkillDef floodSkillDef;
        public static SkillDef corruptedFloodSkillDef;

        //public static SkillDef trespassSkillDef;
        public static SkillDef corruptedTrespassSkillDef;

        //public static SkillDef suppressSkillDef;
        public static SkillDef corruptedSuppressSkillDef;

        internal static BepInEx.Logging.ManualLogSource _logger;
        public void Awake()
        {
            _logger = Logger;
            cfgCustomTokens = Config.Bind<bool>("", "Use custom tokens", false, "").Value;
            RoR2Application.onLoad += CreateVoidSurvivorTokens;
            RoR2Application.onLoad += ModifySkillDefs;
        }

        public static void CreateVoidSurvivorTokens()
        {
            foreach (var language in Language.GetAllLanguages())
            {
                _logger.LogMessage($"Language name:{language.name} selfName:{language.selfName}");
                foreach (string slot in new string[] {"PRIMARY", "SECONDARY", "UTILITY", "SPECIAL" })
                {
                    LanguageAPI.Add($"VOIDSURVIVOR_{slot}_UPGRADE_DESCRIPTION",
                        Language.GetString($"VOIDSURVIVOR_{slot}_DESCRIPTION") + Environment.NewLine
                        + Language.GetString($"VOIDSURVIVOR_{slot}_UPRADE_TOOLTIP", language.name));
                }
            }
        }

        public static void ModifySkillDefs()
        {
            ModifyPrimary();
            ModifySecondary();
            ModifyUtility();
            ModifySpecial();
        }

        public static void ModifySpecial()
        {
            corruptedSuppressSkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/CrushHealth.asset").WaitForCompletion();
            corruptedSuppressSkillDef.skillDescriptionToken = "VOIDSURVIVOR_SPECIAL_CORRUPTED_DESCRIPTION";
        }

        public static void ModifyPrimary()
        {
            corruptedDrownSkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/FireCorruptBeam.asset").WaitForCompletion();
            corruptedDrownSkillDef.skillDescriptionToken = "VOIDSURVIVOR_PRIMARY_CORRUPTED_DESCRIPTION";
        }
        public static void ModifySecondary()
        {
            corruptedFloodSkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/FireCorruptDisk.asset").WaitForCompletion();
            corruptedFloodSkillDef.skillDescriptionToken = "VOIDSURVIVOR_SECONDARY_CORRUPTED_DESCRIPTION";
        }
        public static void ModifyUtility()
        {
            corruptedTrespassSkillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/VoidBlinkDown.asset").WaitForCompletion();
            corruptedTrespassSkillDef.skillDescriptionToken = "VOIDSURVIVOR_UTILITY_CORRUPTED_DESCRIPTION";
        }
    }
}