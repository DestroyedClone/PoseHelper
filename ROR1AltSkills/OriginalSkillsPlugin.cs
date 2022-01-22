using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ROR1AltSkills
{
    [BepInPlugin("com.DestroyedClone.OriginalSkills", "Original Skills", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI), nameof(ProjectileAPI), nameof(DamageAPI), nameof(BuffAPI), nameof(DotAPI))]
    public class OriginalSkillsPlugin : BaseUnityPlugin
    {
        internal static string modkeyword = "DC_ORIGSKILLS_KEYWORD_IDENTIFIER";
        public static ConfigFile _config;
        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _config = Config;
            _logger = Logger;
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.Language))]
        public static void SetupLanguage()
        {
            LanguageAPI.Add(modkeyword, $"[ Original Skills Mod ]");
        }

        [RoR2.SystemInitializer(dependencies: new Type[] { typeof(RoR2.SurvivorCatalog) })]
        public static void AssemblySetup() //credit to bubbet for base code
        {
            var survivorMainType = typeof(SurvivorMain);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract)
                {
                    if (survivorMainType.IsAssignableFrom(type))
                    {
                        var objectInitializer = (SurvivorMain)Activator.CreateInstance(type);
                        objectInitializer.Init(_config);
                    }
                }
            }
        }
    }
}