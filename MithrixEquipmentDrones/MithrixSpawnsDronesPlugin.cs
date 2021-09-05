using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MithrixEquipmentDrones
{
    [BepInPlugin("com.DestroyedClone.MithrixEquipmentDrones", "Mithrix Spawns Equipment Drones", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class MithrixSpawnsDronesPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.ReturnStolenItemsOnGettingHit.Awake += ReturnStolenItemsOnGettingHit_Awake;
        }

        private void ReturnStolenItemsOnGettingHit_Awake(On.RoR2.ReturnStolenItemsOnGettingHit.orig_Awake orig, ReturnStolenItemsOnGettingHit self)
        {
            orig(self);
            var a = self.gameObject.AddComponent<MithrixSpawnsDronesActivator>();
            a.returnStolenItems = self;
        }

        public class MithrixSpawnsDronesActivator : MonoBehaviour
        {
            public ReturnStolenItemsOnGettingHit returnStolenItems;
            private ItemStealController itemStealController;
            private GameObject masterPrefab = null;

            public void Start()
            {
                if (!itemStealController)
                    itemStealController = returnStolenItems.itemStealController;
                itemStealController.onStealFinishClient += ItemStealController_onStealFinishClient;

                masterPrefab = MasterCatalog.FindMasterPrefab("EquipmentDroneMaster");
            }

            public void OnDestroy()
            {
                itemStealController.onStealFinishClient -= ItemStealController_onStealFinishClient;
            }

            private void ItemStealController_onStealFinishClient()
            {
                if (NetworkServer.active)
                {
                    SpawnDronesForEachPlayer();
                }
            }

            [Server]
            private void SpawnDronesForEachPlayer()
            {
                int participatingPlayerCount = Run.instance.participatingPlayerCount != 0 ? Run.instance.participatingPlayerCount : 1;
                float angle = 360f / participatingPlayerCount;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                var nextPosition = gameObject.transform.position + Vector3.up * 8f + Vector3.forward * 4f;

                int i = 0;

                foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
                {
                    if (playerCharacterMasterController && playerCharacterMasterController.body && playerCharacterMasterController.body.healthComponent.alive)
                    {
                        EquipmentIndex equipmentIndex = EquipmentIndex.None;
                        if (playerCharacterMasterController.body.inventory && playerCharacterMasterController.body.inventory.currentEquipmentState.equipmentIndex != EquipmentIndex.None)
                            equipmentIndex = playerCharacterMasterController.body.inventory.GetEquipmentIndex();
                        SummonDrone(gameObject, nextPosition, equipmentIndex);
                        i++;
                        nextPosition = rotation * nextPosition;
                    }
                }
            }

            [Server]
            public CharacterMaster SummonDrone(GameObject bodyPrefab, Vector3 spawnPosition, EquipmentIndex equipmentIndex = EquipmentIndex.None)
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'RoR2.CharacterMaster RoR2.MithrixSpawnsDronesActivator::SummonMaster(UnityEngine.GameObject)' called on client");
                    return null;
                }
                MasterSummon masterSummon = new MasterSummon
                {
                    masterPrefab = masterPrefab,
                    position = spawnPosition,
                    rotation = Quaternion.identity,
                    summonerBodyObject = bodyPrefab,
                    ignoreTeamMemberLimit = true,
                    useAmbientLevel = new bool?(true),
                };
                CharacterMaster characterMaster = masterSummon.Perform();

                if (characterMaster)
                {
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
                        if (equipmentIndex != EquipmentIndex.None)
                        {
                            var inventory = characterMaster.inventory;
                            if (inventory) inventory.SetEquipmentIndex(equipmentIndex);
                        }
                    }
                }
                return characterMaster;
            }
        }
    }
}