using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API.Utils;
using System;
using R2API;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Reflection;
using R2API.ContentManagement;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

//dotnet build --configuration Release
namespace AlternateSkills
{
    [BepInPlugin(MODUID, "DCAltSkills", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), 
    nameof(LanguageAPI), 
    nameof(DamageAPI), 
    nameof(ContentAddition), 
    nameof(RecalculateStatsAPI),
    nameof(DeployableAPI))]

    public class MainPlugin : BaseUnityPlugin
    {
        public static ConfigFile _config;
        internal static BepInEx.Logging.ManualLogSource _logger;
        public const string MODUID = "com.DestroyedClone.DCAltSkills";
        public static bool hasRun = false;
        
        public static R2API.ScriptableObjects.R2APISerializableContentPack ContentPack { get; private set; }

        public void Awake()
        {
            _config = Config;
            _logger = Logger;

            
            ContentPack = R2API.ContentManagement.R2APIContentManager.ReserveSerializableContentPack();
            Modules.Buffs.RegisterBuffs();
            Modules.DamageTypes.RegisterDamageTypes();
            //new Modules.ContentPacks().Initialize();
            On.RoR2.SurvivorCatalog.Init += RunAssemblysetup;
        }

        public void RunAssemblysetup(On.RoR2.SurvivorCatalog.orig_Init orig)
        {
            orig();
            AssemblySetup();
        }

        [RoR2.SystemInitializer(dependencies: new Type[] { typeof(RoR2.SurvivorCatalog) })]
        public static void AssemblySetup() //credit to bubbet for base code
        {
            if (hasRun)
                {
                    _logger.LogWarning("Setup already ran.");
                    return;
                }
            _logger.LogMessage("Running AssemblySetup()");
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
            hasRun = true;
        }

        public static BuffDef[] ReturnBuffs(CharacterBody characterBody, bool returnDebuffs, bool returnBuffs, bool returnCooldownType = false)
        {
            List<BuffDef> buffDefs = new List<BuffDef>();
            BuffIndex buffIndex = (BuffIndex)0;
            BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
            while (buffIndex < buffCount)
            {
                BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                if (characterBody.HasBuff(buffDef))
                {
                    if ((buffDef.isDebuff && returnDebuffs) || (!buffDef.isDebuff && returnBuffs))
                    {
                        buffDefs.Add(buffDef);
                    }
                }
                buffIndex++;
            }
            return buffDefs.ToArray();
        }


        // Aetherium: https://github.com/KomradeSpectre/AetheriumMod/blob/6f35f9d8c57f4b7fa14375f620518e7c904c8287/Aetherium/Items/AccursedPotion.cs#L344-L358
        public static void AddBuffAndDot(BuffDef buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != RoR2.DotController.DotIndex.None)
                {
                    RoR2.DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    body.AddTimedBuffAuthority(buff.buffIndex, duration);
                }
            }
        }
    }
}
