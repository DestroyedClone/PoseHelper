using System;
using BepInEx;
using R2API;
using UnityEngine;
using R2API.Utils;


namespace LobbySkillPreview
{
    [BepInPlugin("com.DestroyedClone.LobbySkillPreview", "Lobby Skill Preview", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class LobbySkillPreviewPlugin : BaseUnityPlugin
    {
        
        public void Awake()
        {

        }


    }
}
