using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using BepInEx.Configuration;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace KKA_AddOn
{
    [BepInPlugin("com.DestroyedClone.KKA_AddOn", "KingKombatArena AddOn", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class KKAAPlugin : BaseUnityPlugin
    {
        /* 1. Custom scenes like LobbyAppearanceImprovements
         * 2. Reduce size of billboarded particle effects due to blocking the screen
         * 3. 
         * 
         * 
         */

        public static ConfigEntry<float> particleSizeReduction;

        public void Awake()
        {
            particleSizeReduction = Config.Bind("Visuals", "Particle Effect Size Multiplier", 0.5f, "Certain particle effects will get reduced in size." +
                "\nIncluding: ");
        }
    }
}
