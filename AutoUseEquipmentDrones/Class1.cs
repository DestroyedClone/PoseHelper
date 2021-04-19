using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.EquipmentIndex;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EntityStates;
using JetBrains.Annotations;
using RoR2.Navigation;
using UnityEngine.AI;
using EntityStates.GoldGat;
using System.Security;
using System.Security.Permissions;

using EntityStates.AI;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AutoUseEquipmentDrones
{
    [BepInPlugin("com.DestroyedClone.BetterEquipmentDroneUse", "Better Equipment Drone Use", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class AUEDPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<string> TargetPriority { get; set; }
        public static Type[] allowedTypesToScan = new Type[] { };
        public static ConfigEntry<string> Recycler_Items { get; set; }
        public static ConfigEntry<string> Recycler_Equipment { get; set; }

        public static List<ItemIndex> allowedItemIndices = new List<ItemIndex>();
        public static List<EquipmentIndex> allowedEquipmentIndices = new List<EquipmentIndex>();
        public static List<PickupIndex> allowedPickupIndices = new List<PickupIndex>();

        public void Awake()
        {
            Recycler_Items = Config.Bind("Recycler", "Item IDS", "Tooth,Seed,Icicle,GhostOnKill,BounceNearby,MonstersOnShrineUse", "Enter the IDs of the item you want equipment drones to recycle." +
    "\nSeparated by commas (ex: AffixRed,Meteor,Fruit)");
            Recycler_Equipment = Config.Bind("Recycler", "Equipment IDS", "Meteor,CritOnUse,GoldGat,Scanner,Gateway", "Enter the IDs of the equipment you want equipment drones to recycle." +
    "\nSeparated by commas (ex: AffixRed,Meteor,Fruit)");


            var body = Resources.Load<GameObject>("prefabs/characterbodies/EquipmentDroneBody");
            On.RoR2.ChestRevealer.Init += GetAllowedTypes;

            //On.RoR2.CharacterAI.BaseAI.FixedUpdate += BaseAIOverride;
            On.RoR2.ItemCatalog.Init += CacheWhitelistedItems;
            On.RoR2.EquipmentCatalog.Init += CacheWhitelistedEquipment;
            On.RoR2.PickupCatalog.Init += CachePickupIndices;

            //On.RoR2.CharacterAI.BaseAI.UpdateBodyAim += BaseAI_UpdateBodyAim;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs += Conditional_ForceEquipmentUse;
            On.RoR2.EquipmentSlot.Awake += GiveComponent;
        }

        private void GiveComponent(On.RoR2.EquipmentSlot.orig_Awake orig, EquipmentSlot self)
        {
            orig(self);
            switch (self.characterBody.baseNameToken)
            {
                case "EQUIPMENTDRONE_BODY_NAME":
                    var baseAI = self.characterBody.masterObject.GetComponent<BaseAI>();
                    if (!baseAI)
                    {
                        Debug.Log("No BaseAI!");
                        return;
                    }

                    var component = baseAI.gameObject.GetComponent<BEDUComponent>();
                    if (!component)
                    {
                        component = baseAI.gameObject.AddComponent<BEDUComponent>();
                        component.baseAI = baseAI;
                        Chat.AddMessage("Adding drone component!");
                    }
                    break;
                default:
                    return;
            }
        }

        private void Conditional_ForceEquipmentUse(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyInputs orig, BaseAI self)
        {
            var component = self.gameObject.GetComponent<BEDUComponent>();
            if (component)
            {
                BaseAIState baseAIState;
                if ((baseAIState = (self.stateMachine.state as BaseAIState)) != null)
                {
                    self.bodyInputs = baseAIState.GenerateBodyInputs(self.bodyInputs);
                }
                if (self.bodyInputBank)
                {
                    bool useEquipment = component.useEquipment || component.freeUse;

                    self.bodyInputBank.skill1.PushState(self.bodyInputs.pressSkill1);
                    self.bodyInputBank.skill2.PushState(self.bodyInputs.pressSkill2);
                    self.bodyInputBank.skill3.PushState(self.bodyInputs.pressSkill3);
                    self.bodyInputBank.skill4.PushState(self.bodyInputs.pressSkill4);
                    self.bodyInputBank.jump.PushState(self.bodyInputs.pressJump);
                    self.bodyInputBank.sprint.PushState(self.bodyInputs.pressSprint);
                    self.bodyInputBank.activateEquipment.PushState(useEquipment);
                    self.bodyInputBank.moveVector = self.bodyInputs.moveVector;
                }
            } else
            {
                orig(self);
            }
        }

        private void CachePickupIndices(On.RoR2.PickupCatalog.orig_Init orig)
        {
            orig();
            foreach (var itemIndex in allowedItemIndices)
            {
                if (PickupCatalog.FindPickupIndex(itemIndex) != PickupIndex.none)
                    allowedPickupIndices.Add(PickupCatalog.FindPickupIndex(itemIndex));
            }
            foreach (var equipmentIndex in allowedEquipmentIndices)
            {
                if (PickupCatalog.FindPickupIndex(equipmentIndex) != PickupIndex.none)
                    allowedPickupIndices.Add(PickupCatalog.FindPickupIndex(equipmentIndex));
            }
        }

        private void CacheWhitelistedItems(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();
            Debug.Log("Caching whitelisted items for Recycler.");
            var testStringArray = Recycler_Items.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (ItemCatalog.FindItemIndex(stringToTest) == ItemIndex.None) { continue; }
                    allowedItemIndices.Add(ItemCatalog.FindItemIndex(stringToTest));
                    Debug.Log("Adding whitelisted item: "+ stringToTest);
                }
            }
        }

        private void CacheWhitelistedEquipment(On.RoR2.EquipmentCatalog.orig_Init orig)
        {
            orig();
            Debug.Log("Caching whitelisted EQUIPMENT for Recycler.");
            var testStringArray = Recycler_Equipment.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (EquipmentCatalog.FindEquipmentIndex(stringToTest) == EquipmentIndex.None) { continue; }
                    allowedEquipmentIndices.Add(EquipmentCatalog.FindEquipmentIndex(stringToTest));
                    Debug.Log("Adding whitelisted equipment: " + stringToTest);
                }
            }
        }

        private void FixedGoldGat(On.EntityStates.GoldGat.GoldGatIdle.orig_FixedUpdate orig, EntityStates.GoldGat.GoldGatIdle self)
        {
            self.FixedUpdate();
            self.gunAnimator?.SetFloat("Crank.playbackRate", 0f, 1f, Time.fixedDeltaTime);
            if (self.isAuthority && self.shouldFire && self.bodyMaster.money > 0U && self.bodyEquipmentSlot.stock > 0)
            {
                self.outer.SetNextState(new GoldGatFire
                {
                    shouldFire = self.shouldFire
                });
                return;
            }
        }


        private void GetAllowedTypes(On.RoR2.ChestRevealer.orig_Init orig)
        {
            orig();
            allowedTypesToScan = ChestRevealer.typesToCheck;
        }


        public static bool CheckForAlive(TeamIndex teamIndex)
        {
            ReadOnlyCollection<TeamComponent> teamComponents = TeamComponent.GetTeamMembers(teamIndex);
            return teamComponents.Count > 0;
        }

        public static bool CheckForInteractables()
        {
            ///Type[] validInteractables = new Type[] {  };
            foreach (var valid in allowedTypesToScan)
            {
                InstanceTracker.FindInstancesEnumerable(valid);
                if (((IInteractable)valid).ShouldShowOnScanner())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckForDebuffs(CharacterBody characterBody) //try hooking addbuff instead?
        {
            BuffIndex buffIndex = (BuffIndex)0;
            BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
            while (buffIndex < buffCount)
            {
                BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                if (buffDef.isDebuff)
                {
                    if (characterBody.HasBuff(buffIndex))
                    {
                        return true;
                    }
                }
                buffIndex++;
            }
            return false;
        }

        enum DroneMode
        {
            None,
            EnemyOnMap,
            PriorityTarget,
            Evade,
            GoldGat,
            PassiveHealing,
            Gateway,
            Cleanse,
            Saw,
            Recycle,
            Fruit,
            Snuggle,
            Scan
        }

        public class BEDUComponent : MonoBehaviour
        {
            public BaseAI baseAI = null;
            bool isNetwork = false;
            public TeamIndex enemyTeamIndex = TeamIndex.None;
            readonly EquipmentSlot equipmentSlot;
            EquipmentIndex equipmentIndex;
            DroneMode droneMode = DroneMode.None;
            bool equipmentReady = false;

            public bool freeUse = false;
            public bool useEquipment = false;

            bool hasSpoken = false;

            void Awake()
            {
                enemyTeamIndex = baseAI.body.teamComponent.teamIndex == TeamIndex.Player ? TeamIndex.Monster : TeamIndex.Player;
                isNetwork = NetworkServer.active;
                equipmentIndex = equipmentSlot.equipmentIndex;

                EvaluateDroneMode();
            }
            void DroneSay(string msg)
            {
                if (!hasSpoken)
                {
                    Chat.AddMessage(Run.instance.NetworkfixedTime + " <style=cIsUtility> Drone: " + msg + "</style>");
                    hasSpoken = true;
                }
            }
            void EvaluateDroneMode()
            {
                bool match(EquipmentDef equipmentDef)
                {
                    return equipmentIndex == equipmentDef.equipmentIndex;
                }

                if (match(RoR2Content.Equipment.CommandMissile) || match(RoR2Content.Equipment.Meteor))
                    droneMode = DroneMode.EnemyOnMap;
                else if (match(RoR2Content.Equipment.Blackhole) || match(RoR2Content.Equipment.BFG) || match(RoR2Content.Equipment.Lightning) || match(RoR2Content.Equipment.CrippleWard))
                    droneMode = DroneMode.PriorityTarget;
                else if (match(RoR2Content.Equipment.Jetpack) || match(RoR2Content.Equipment.GainArmor) || match(RoR2Content.Equipment.Tonic)) //Spam Jump
                    droneMode = DroneMode.Evade;
                else if (match(RoR2Content.Equipment.GoldGat))
                    droneMode = DroneMode.GoldGat;
                else if (match(RoR2Content.Equipment.PassiveHealing))
                    droneMode = DroneMode.PassiveHealing;
                else if (match(RoR2Content.Equipment.Gateway))
                    droneMode = DroneMode.Gateway;
                else if (match(RoR2Content.Equipment.Cleanse))
                    droneMode = DroneMode.Cleanse;
                else if (match(RoR2Content.Equipment.Saw))
                    droneMode = DroneMode.Saw;
                else if (match(RoR2Content.Equipment.Recycle))
                    droneMode = DroneMode.Recycle;
                else if (match(RoR2Content.Equipment.Fruit))
                    droneMode = DroneMode.Fruit;
                else if (match(RoR2Content.Equipment.BurnNearby) || match(RoR2Content.Equipment.QuestVolatileBattery))
                    droneMode = DroneMode.Snuggle;
                else if (match(RoR2Content.Equipment.Scanner))
                    droneMode = DroneMode.Scan;
            }

            void FixedUpdate()
            {
                bool forceActive = false;
                freeUse = false;
                equipmentReady = equipmentSlot.stock > 0;
                if (!equipmentReady)
                {
                    hasSpoken = false;
                    return;
                }

                switch (droneMode)
                {
                    case DroneMode.EnemyOnMap:
                        if (CheckForAlive(enemyTeamIndex))
                        {
                            DroneSay("There's enemies alive!");
                            forceActive = true;
                        }
                        break;
                    case DroneMode.PriorityTarget:
                        break;
                    case DroneMode.Evade:
                        freeUse = true;
                        break;
                    case DroneMode.GoldGat:
                        uint num2 = (uint)((float)GoldGatFire.baseMoneyCostPerBullet * (1f + (TeamManager.instance.GetTeamLevel(baseAI.master.teamIndex) - 1f) * 0.25f));
                        baseAI.master.money = num2 * 60;
                        break;
                    case DroneMode.PassiveHealing:
                        break;
                    case DroneMode.Cleanse:
                        if (CheckForDebuffs(baseAI.body))
                        {
                            DroneSay("I'm filthy! Cleaning!!");
                            forceActive = true;
                        }
                        break;
                    case DroneMode.Saw:
                        freeUse = true;
                        break;
                    case DroneMode.Recycle:
                        GenericPickupController pickupController = equipmentSlot.currentTarget.pickupController;
                        if (pickupController && !pickupController.Recycled)
                        {
                            PickupIndex initialPickupIndex = pickupController.pickupIndex;
                            if (allowedPickupIndices.Contains(initialPickupIndex))
                            {
                                DroneSay("Bad Item/Equipment!!");
                                forceActive = true;
                            }
                        }
                        break;
                    case DroneMode.Fruit:
                        if (baseAI.body.healthComponent.health <= baseAI.body.healthComponent.fullHealth * 0.5f)
                        {
                            DroneSay("I'm low health! Gonna heal!");
                            forceActive = true;
                        }
                        //forceActive = self.healthComponent?.health <= self.healthComponent?.fullHealth * 0.5f;
                        break;
                    case DroneMode.Snuggle:
                        freeUse = true;
                        break;
                    case DroneMode.Scan:
                        if (CheckForInteractables())
                        {
                            forceActive = true;
                            DroneSay("There's still stuff to buy!");
                        }
                        break;
                    default:
                        freeUse = true;
                        break;
                }
                if (forceActive) Debug.Log("attempting to use equipment");
                    useEquipment = forceActive && equipmentReady;
            }
        }
    }
}
