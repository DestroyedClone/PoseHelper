using BepInEx;
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
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class SVPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> GooLakeFilled { get; set; }
        public static GameObject LandInGoo;

        public void Awake()
        {
            GooLakeFilled = Config.Bind("Abandoned Aqueduct", "Filled Goo Lake Chance", 50f, "Fills a channel of the map with goo that inflicts the ClayGoo debuff.");

            On.RoR2.SceneDirector.Start += ChooseSceneToModify;
            On.RoR2.EffectCatalog.Init += MakeGooEnd;
        }

        private void MakeGooEnd(On.RoR2.EffectCatalog.orig_Init orig)
        {
            orig();
            LandInGoo = Resources.Load<GameObject>("prefabs/effects/impacteffects/LandInGoo");
            var destroyOnTimer = LandInGoo.gameObject.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 2f;
        }

        private void ChooseSceneToModify(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "goolake":
                    if (Util.CheckRoll(GooLakeFilled.Value))
                    {
                        FillGooLake();
                    }
                    break;
            }
        }

        private void FillGooLake()
        {
            var miscprops = GameObject.Find("HOLDER: Misc Props");
            var gooplane = miscprops.transform.Find("GooPlane");
            var gooplanehigh = miscprops.transform.Find("GooPlane, High");
            var debuffArea = gooplane.transform.Find("DEBUFF ZONE: Plane");
            var SFX = gooplane.transform.Find("SFX");

            var debuffClone = Instantiate<GameObject>(debuffArea.gameObject, gooplanehigh);
            var SFXClone = Instantiate<GameObject>(SFX.gameObject, gooplanehigh);

            debuffClone.transform.localScale = new Vector3(10f, 1f, 10f);
            var debuffZone = debuffClone.GetComponent<DebuffZone>();
            var debuffZonePlus = debuffClone.AddComponent<DebuffZonePlus>();
            debuffZonePlus.buffApplicationEffectPrefab = debuffZone.buffApplicationEffectPrefab;
            debuffZonePlus.buffApplicationSoundString = debuffZone.buffApplicationSoundString;
            debuffZonePlus.buffDuration = debuffZone.buffDuration;
            debuffZonePlus.buffType = debuffZone.buffType;
            debuffZonePlus.orbResetListFrequency = debuffZonePlus.buffDuration;
            UnityEngine.Object.Destroy(debuffZone);

            gooplanehigh.gameObject.SetActive(true);
        }

    }
}
