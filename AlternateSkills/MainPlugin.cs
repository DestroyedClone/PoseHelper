using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace AlternateSkills
{
    [BepInPlugin("com.DestroyedClone.AlternateSkills", "Alternate Skills", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]

    public class MainPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Buffs.RegisterBuffs();
        }

    }
}
