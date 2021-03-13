using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace VersusPlayerBoss
{
    [BepInPlugin("com.DestroyedClone.RadarEffectToggle", "Versus Player Boss", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class VPBPlugin : BaseUnityPlugin
    {

    }
}
