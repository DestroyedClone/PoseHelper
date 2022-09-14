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
using System.Linq;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MithrixEquipmentDrones
{
    [BepInPlugin("com.DestroyedClone.MithrixSpawnsEquipmentDrones", "Mithrix Spawns Equipment Drones", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class MithrixStealsEquipment : BaseUnityPlugin
    {
        public static ConfigEntry<string> bannedEquipment;
        public static EquipmentDef[] bannedEquipmentDefs = new EquipmentDef[0];
        public static EquipmentDef defaultItemDef = null;
        public void Awake()
        {
            bannedEquipment = Config.Bind("", "Banned Items", "", "Add any items you want to ban, separate with commas. Leave empty to disable." +
                "\nEx: \"Meteor,Lightning,DeathProjectile\"");
            On.RoR2.ReturnStolenItemsOnGettingHit.Awake += ReturnStolenItemsOnGettingHit_Awake;
        }

        public static void GetEquipmentDefs()
        {
            if (!bannedEquipment.Value.IsNullOrWhiteSpace())
            {
                string[] subs = bannedEquipment.Value.Split(',');
                List<EquipmentDef> equipmentDefs = new List<EquipmentDef>();
                foreach (var sub in subs)
                {
                    var equipmentIndex = EquipmentCatalog.FindEquipmentIndex(sub);
                    if (equipmentIndex != EquipmentIndex.None)
                    {
                        equipmentDefs.Add(EquipmentCatalog.GetEquipmentDef(equipmentIndex));
                    }
                }
                bannedEquipmentDefs = equipmentDefs.ToArray();
            }
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
            List<EquipmentIndex> equipmentIndexes = new List<EquipmentIndex>();
            public CharacterMaster mithrixMaster;
            Inventory lendeeInventory;
            EquipmentSlot equipmentSlot;

            public void Start()
            {
                if (!itemStealController)
                    itemStealController = returnStolenItems.itemStealController;
                lendeeInventory = itemStealController.lendeeInventory;
                itemStealController.onStealFinishClient += ItemStealController_onStealFinishClient;
                mithrixMaster = lendeeInventory.GetComponent<CharacterMaster>();
            }

            public void OnDestroy()
            {
                itemStealController.onStealFinishClient -= ItemStealController_onStealFinishClient;
            }

            public void FixedUpdate()
            {
                if (!mithrixMaster)
                {
                    mithrixMaster = lendeeInventory.GetComponent<CharacterMaster>();
                }
                if (!equipmentSlot)
                {
                    equipmentSlot = mithrixMaster.GetBody().equipmentSlot;
                }
                if (equipmentSlot && equipmentSlot.hasEffectiveAuthority && equipmentSlot.characterBody.isEquipmentActivationAllowed)
                    foreach (var equipmentState in lendeeInventory.equipmentStateSlots)
                    {
                        if (equipmentState.equipmentDef)
                        {
                            Execute(equipmentState);
                        }
                    }
            }

            [Server]
            private void Execute(EquipmentState equipmentState)
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Void RoR2.EquipmentSlot::Execute()' called on client");
                    return;
                }
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentState.equipmentIndex);
                if (equipmentDef != null && equipmentSlot.subcooldownTimer <= 0f && equipmentSlot.PerformEquipmentAction(equipmentDef))
                {
                    equipmentSlot.OnEquipmentExecuted();
                }
            }

            private void ItemStealController_onStealFinishClient()
            {
                if (NetworkServer.active)
                {
                    GetEquipmentDefs();
                    SetEquipments();
                }
            }
            
            private void SetEquipments()
            {
                if (equipmentIndexes.Count <= 0)
                    return;

                //var currentEquipmentIndex = itemStealController.lendeeInventory.currentEquipmentIndex;
                //var currentEqpDef = EquipmentCatalog.GetEquipmentDef(currentEquipmentIndex);
                /*if (currentEqpDef && currentEqpDef.passiveBuffDef)
                {
                    //itemStealController.lendeeInventory.GetComponent<CharacterMaster>().GetBody()?.AddBuff(currentEqpDef.passiveBuffDef);
                }*/
                uint slot = 1;
                foreach (var equipmentIndex in equipmentIndexes)
                {
                    lendeeInventory.SetEquipmentIndexForSlot(equipmentIndex, slot);
                    slot++;
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
                            if (equipmentState.equipmentDef != null && !bannedEquipmentDefs.Contains(equipmentState.equipmentDef))
                            {
                                Chat.AddMessage($"Stealing {pcmc.GetDisplayName()}'s {Language.GetString(equipmentState.equipmentDef.nameToken)}");
                                equipmentIndexes.Add(equipmentState.equipmentIndex);
                            }
                        }
                    }
                }
            }
        }
    }
}