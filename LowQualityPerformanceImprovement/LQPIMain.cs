using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using Path = System.IO.Path;
using UnityEngine.AddressableAssets;

namespace LowQualityPerformanceImprovement
{
    [BepInPlugin("com.DestroyedClone.LowQualityPerformanceImprovement", "Low Quality Performance Improvement", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class LQPIMain : BaseUnityPlugin
    {
        //1. Pickup display simplified, no spawn effects
        //2. Billboard sprites in stead of showing the model.
        public static GameObject pickupDroplet = Resources.Load<GameObject>("prefabs/networkedobjects/pickupdroplet");
        public static GameObject spriteObject;

        public static LQPIMain instance;

        public void Awake()
        {
            Debug.Log($"Cursor.lockstate = {Cursor.lockState}");
            Cursor.lockState = CursorLockMode.None;
        }

        public void Start()
        {
            instance = this;
            //SetupSpriteObject();
            //On.RoR2.PickupCatalog.Init += PickupCatalog_Init;
            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            //UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            ModifyPrefabs();
        }

        private void ModifyPrefabs()
        {
            var wispBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Wisp/WispBody.prefab").WaitForCompletion();
            wispBody.transform.Find("Model Base/mdlWisp1Mouth/WispArmature/ROOT/Base/Fire").gameObject.SetActive(false);
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            if (loadSceneMode == UnityEngine.SceneManagement.LoadSceneMode.Single)
            {
                if (RoR2.SceneCatalog.GetSceneDefFromScene(scene).sceneType == SceneType.Stage)
                {
                    Methods.PrintSceneCollisi2ons();
                    Chat.AddMessage("printed");
                }
            }
        }

        [ConCommand(commandName = "show_collideables", flags = ConVarFlags.None, helpText = "")]
        private static void DiscoverScene(ConCommandArgs args)
        {
           Methods.PrintSceneCollisi2ons();
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
        }

        private void PickupCatalog_Init(On.RoR2.PickupCatalog.orig_Init orig)
        {
            orig();
            foreach (var pickupDef in PickupCatalog.allPickups)
            {
                bool isItem = pickupDef.itemIndex >= 0;
                bool isEquipment = pickupDef.equipmentIndex >= 0;
                if (pickupDef.coinValue > 0) continue;
                if (!isItem && !isEquipment) continue;
                if (!pickupDef.displayPrefab) continue;
                var displayPrefab = pickupDef.displayPrefab;
                foreach (var child in displayPrefab.GetComponentsInChildren<Transform>().ToList())
                {
                    Destroy(child);
                }
            }
        }

        private void SetupSpriteObject()
        {
            var textPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/BearProc"), "PickupSpriteDisplay");
            var indicatorObject = Instantiate(Resources.Load<GameObject>("prefabs/positionindicators/poipositionindicator").transform.Find("PositionIndicator/BoingyScaler/AlwaysVisible/Default").gameObject,
                textPrefab.transform.Find("TextCamScaler"));

            Destroy(textPrefab.GetComponent<DestroyOnTimer>());
            Destroy(textPrefab.transform.Find("Fluff").gameObject);
            var com = textPrefab.AddComponent<PickupSpriteDisplay>();
            //var tmp = textPrefab.transform.Find("TextCamScaler/TextRiser/TextMeshPro").GetComponent<TextMeshPro>();
            var sr = textPrefab.transform.Find("TextCamScaler/TextRiser/TextMeshPro").gameObject.AddComponent<SpriteRenderer>();
        }

        private class PickupSpriteDisplay : MonoBehaviour
        {

        }

        private void PickupDropletController_Start(On.RoR2.PickupDropletController.orig_Start orig, PickupDropletController self)
        {
            PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickupIndex);
            GameObject gameObject = pickupDef?.dropletDisplayPrefab;
            if (gameObject)
            {
                UnityEngine.Object.Instantiate<GameObject>(gameObject, base.transform);
            }
        }


    }
}