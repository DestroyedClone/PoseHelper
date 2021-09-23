using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2.UI;
using BepInEx.Configuration;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MithrixEquipmentDrones
{
    [BepInPlugin("com.DestroyedClone.MithrixSpawnsEquipmentDrones", "Mithrix Spawns Equipment Drones", "1.0.1")]
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
            List<EquipmentIndex> equipmentIndexes = new List<EquipmentIndex>();

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

            private void GetEquipmentDefs()
            {
                equipmentIndexes.Clear();
                foreach (var pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.master && pcmc.master.inventory && !pcmc.master.IsDeadAndOutOfLivesServer())
                    {
                        foreach (var equipmentState in pcmc.master.inventory.equipmentStateSlots)
                        {
                            if (equipmentState.equipmentDef != null)
                            {
                                equipmentIndexes.Add(equipmentState.equipmentIndex);
                            }
                        }
                    }
                }
                if (equipmentIndexes.Count == 0)
                    equipmentIndexes.Add(RoR2Content.Equipment.Fruit.equipmentIndex);
            }

            private void AdjustHealth(CharacterMaster characterMaster)
            {
                float num = 1f;
                float num2 = 1f;
                num += Run.instance.difficultyCoefficient / 2.5f;
                num2 += Run.instance.difficultyCoefficient / 30f;
                int num3 = Mathf.Max(1, Run.instance.livingPlayerCount);
                num *= Mathf.Pow((float)num3, 0.5f);
                Debug.LogFormat("Scripted Combat Encounter (Equipment Drone): currentBoostHpCoefficient={0}, currentBoostDamageCoefficient={1}", new object[]
                {
                        num,
                        num2
                });
                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt((num - 1f) * 10f));
                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((num2 - 1f) * 10f));
                characterMaster.inventory.GiveItem(RoR2Content.Items.Hoof, 10);
            }

            [Server]
            private void SpawnDronesForEachPlayer()
            {
                GetEquipmentDefs();
                int participatingPlayerCount = equipmentIndexes.Count;
                float angle = 360f / participatingPlayerCount;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                var nextPosition = gameObject.transform.position + Vector3.up * 8f + Vector3.forward * 8f;

                int i = 0;

                RoR2.UI.HUDBossHealthBarController bossHealthBarController = UnityEngine.Object.FindObjectOfType<HUDBossHealthBarController>();

                foreach (var equipmentIndex in equipmentIndexes)
                {
                    var drone = SummonDrone(gameObject, nextPosition, equipmentIndex);
                    AdjustHealth(drone);
                    bossHealthBarController.currentBossGroup.AddBossMemory(drone);
                    i++;
                    nextPosition = rotation * nextPosition;
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
                        var inventory = characterMaster.inventory;
                        if (inventory) inventory.SetEquipmentIndex(equipmentIndex);
                    }
                }
                return characterMaster;
            }
        }
    }
}