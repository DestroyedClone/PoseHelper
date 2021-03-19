﻿using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace StageVariantsPlus
{
    [BepInPlugin("com.DestroyedClone.StageVariantsPlus", "Stage Variants Plus", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class SVPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.SceneDirector.Start += ChooseSceneToModify;
        }

        private void ChooseSceneToModify(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "goolake")
            {
                if (Util.CheckRoll(100))
                {
                    ModifyGooLake();
                }
            }
        }

        private void ModifyGooLake()
        {
            var miscprops = GameObject.Find("HOLDER: Misc Props");
            var gooplane = miscprops.transform.Find("GooPlane");
            var gooplanehigh = miscprops.transform.Find("GooPlane, High");
            var debuffArea = gooplane.transform.Find("DEBUFF ZONE: Plane");
            var SFX = gooplane.transform.Find("SFX");

            var debuffClone = Instantiate<GameObject>(debuffArea.gameObject, gooplanehigh);
            var SFXClone = Instantiate<GameObject>(SFX.gameObject, gooplanehigh);

            var debuffZone = debuffClone.GetComponent<DebuffZone>();
            var debuffZonePlus = debuffClone.AddComponent<DebuffZonePlus>();
            debuffZonePlus.buffApplicationEffectPrefab = debuffZone.buffApplicationEffectPrefab;
            debuffZonePlus.buffApplicationSoundString = debuffZone.buffApplicationSoundString;
            debuffZonePlus.buffDuration = debuffZone.buffDuration;
            debuffZonePlus.buffType = debuffZone.buffType;
            debuffZonePlus.orbResetListFrequency = debuffZonePlus.buffDuration;
            UnityEngine.Object.Destroy(debuffZone);
        }

    }
}