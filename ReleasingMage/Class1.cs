using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using BepInEx.Configuration;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace ReleasingMage
{
    [BepInPlugin("com.DestroyedClone.ReleasingMage", "Releasing Mage", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public static GameObject MageMasterPrefab = MasterCatalog.FindMasterPrefab("MageMaster");
        public static GameObject displayPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/LockedMage");

        public static ConfigEntry<bool> cfgCopyInventory;
        public static ConfigEntry<int> cfgLives;
        //public static ConfigEntry<int> cfgCost;

        public void Awake()
        {
            cfgCopyInventory = Config.Bind("", "Copy Inventory", true, "If set to true, then it will copy the inventory of the purchaser.");
            cfgLives = Config.Bind("", "Extra Lives", 3, "Set the value to the amount of \"Dio's Best Friend\" that the mage will get.");
            //cfgCost = Config.Bind("", "Mage Cost", 10, "Set the value to the new cost of the .");

            UnityEngine.Object.Destroy(displayPrefab.GetComponent<GameObjectUnlockableFilter>());

            EntityStates.LockedMage.UnlockingMage.onOpened += UnlockingMage_onOpened;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (scene.name == "bazaar")
            {
                UnityEngine.Object.FindObjectsOfType<GameObjectUnlockableFilter>()[0].enabled = false;
            }
        }

        [RoR2.SystemInitializer(dependencies: typeof(MasterCatalog))]
        private static void CachedMaster()
        {
            MageMasterPrefab = MasterCatalog.FindMasterPrefab("MageMonsterMaster");
        }

        private void UnlockingMage_onOpened(Interactor obj)
        {
            if (NetworkServer.active)
            {
                SummonMage(obj.gameObject, obj.transform.position);
            }
        }

        [Server]
        public CharacterMaster SummonMage(GameObject summonerBody, Vector3 spawnPosition)
        {
            if (!NetworkServer.active)
            {
                return null;
            }
            Inventory summonerInventory = summonerBody.GetComponent<CharacterBody>()?.inventory ? summonerBody.GetComponent<CharacterBody>().inventory : null;
            MasterSummon masterSummon = new MasterSummon
            {
                masterPrefab = MageMasterPrefab,
                position = spawnPosition,
                rotation = Quaternion.identity,
                summonerBodyObject = summonerBody ?? null,
                ignoreTeamMemberLimit = true,
                useAmbientLevel = new bool?(true),
                inventoryToCopy = cfgCopyInventory.Value ? summonerInventory : null,
            };
            CharacterMaster characterMaster = masterSummon.Perform();

            if (characterMaster)
            {
                DontDestroyOnLoad(characterMaster);
                if (characterMaster.inventory)
                {
                    characterMaster.inventory.GiveItem(RoR2Content.Items.ExtraLife, cfgLives.Value);
                }

                GameObject bodyObject = characterMaster.GetBodyObject();
                if (bodyObject)
                {
                    ModelLocator component = bodyObject.GetComponent<ModelLocator>();
                    if (component && component.modelTransform)
                    {
                        TemporaryOverlay temporaryOverlay = component.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                        temporaryOverlay.duration = 0.5f;
                        temporaryOverlay.animateShaderAlpha = true;
                        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        temporaryOverlay.destroyComponentOnEnd = true;
                        temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matSummonDrone");
                        temporaryOverlay.AddToCharacerModel(component.modelTransform.GetComponent<CharacterModel>());
                    }
                }
            }
            return characterMaster;
        }
    }
}
