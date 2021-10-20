using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.RoR2Content.Equipment;
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
using static BetterEquipmentDroneUse.Methods;
using EntityStates.AI;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

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

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static Dictionary<EquipmentIndex, DroneMode> DroneModeDictionary = new Dictionary<EquipmentIndex, DroneMode>();

        public void Awake()
        {
            _logger = Logger;

            Recycler_Items = Config.Bind("Recycler", "Item IDS", "Tooth,Seed,Icicle,GhostOnKill,BounceNearby,MonstersOnShrineUse", "Enter the IDs of the item you want equipment drones to recycle." +
    "\nSeparated by commas (ex: AffixRed,Meteor,Fruit)");
            Recycler_Equipment = Config.Bind("Recycler", "Equipment IDS", "Meteor,CritOnUse,GoldGat,Scanner,Gateway", "Enter the IDs of the equipment you want equipment drones to recycle." +
    "\nSeparated by commas (ex: AffixRed,Meteor,Fruit)");


            var body = Resources.Load<GameObject>("prefabs/characterbodies/EquipmentDroneBody");

            //On.RoR2.CharacterAI.BaseAI.FixedUpdate += BaseAIOverride;

            //On.RoR2.CharacterAI.BaseAI.UpdateBodyAim += BaseAI_UpdateBodyAim;
            On.RoR2.CharacterAI.BaseAI.UpdateBodyInputs += Conditional_ForceEquipmentUse;
            On.RoR2.EquipmentSlot.OnStartServer += GiveComponent;
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            On.RoR2.EquipmentCatalog.Init += EquipmentCatalog_Init;
        }

        private void EquipmentCatalog_Init(On.RoR2.EquipmentCatalog.orig_Init orig)
        {
            orig();
            SetupDictionary();
        }

        public static void SetupDictionary()
        {
            DroneModeDictionary = new Dictionary<EquipmentIndex, DroneMode>()
            {
                [RoR2Content.Equipment.CommandMissile.equipmentIndex] = DroneMode.EnemyOnMap,
                [RoR2Content.Equipment.Meteor.equipmentIndex] = DroneMode.EnemyOnMap,

                [RoR2Content.Equipment.Blackhole.equipmentIndex] = DroneMode.PriorityTarget,
                [RoR2Content.Equipment.BFG.equipmentIndex] = DroneMode.PriorityTarget,
                [RoR2Content.Equipment.Lightning.equipmentIndex] = DroneMode.PriorityTarget,
                [RoR2Content.Equipment.CrippleWard.equipmentIndex] = DroneMode.PriorityTarget,

                [RoR2Content.Equipment.Jetpack.equipmentIndex] = DroneMode.Evade,
                [RoR2Content.Equipment.GainArmor.equipmentIndex] = DroneMode.Evade,
                [RoR2Content.Equipment.Tonic.equipmentIndex] = DroneMode.Evade,

                [RoR2Content.Equipment.GoldGat.equipmentIndex] = DroneMode.GoldGat,

                [RoR2Content.Equipment.PassiveHealing.equipmentIndex] = DroneMode.PassiveHealing,

                [RoR2Content.Equipment.Gateway.equipmentIndex] = DroneMode.Gateway,

                [RoR2Content.Equipment.Cleanse.equipmentIndex] = DroneMode.Cleanse,

                [RoR2Content.Equipment.Saw.equipmentIndex] = DroneMode.Saw,

                [RoR2Content.Equipment.Recycle.equipmentIndex] = DroneMode.Recycle,

                [RoR2Content.Equipment.Fruit.equipmentIndex] = DroneMode.Fruit,

                [RoR2Content.Equipment.BurnNearby.equipmentIndex] = DroneMode.Snuggle,
                [RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex] = DroneMode.Snuggle,

                [RoR2Content.Equipment.Scanner.equipmentIndex] = DroneMode.Scan,
            };
        }

        #region Cache
        [RoR2.SystemInitializer(dependencies: typeof(RoR2.PickupCatalog))]
        private static void CachePickupIndices()
        {
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

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.ItemCatalog))]
        private void CacheWhitelistedItems()
        {
            Debug.Log("Caching whitelisted items for Recycler.");
            var testStringArray = Recycler_Items.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (ItemCatalog.FindItemIndex(stringToTest) == ItemIndex.None) { continue; }
                    allowedItemIndices.Add(ItemCatalog.FindItemIndex(stringToTest));
                    Debug.Log("Adding whitelisted item: " + stringToTest);
                }
            }
            Debug.Log(allowedItemIndices);
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.EquipmentCatalog))]
        private void CacheWhitelistedEquipment()
        {
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

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.ChestRevealer))]
        private static void CacheAllowedChestRevealerTypes()
        {
            allowedTypesToScan = ChestRevealer.typesToCheck;
        }
        #endregion

        private void GiveComponent(On.RoR2.EquipmentSlot.orig_OnStartServer orig, EquipmentSlot self)
        {
            orig(self);
            if (!self || !self.characterBody || !self.characterBody.master) return;
            switch (self.characterBody.baseNameToken)
            {
                case "EQUIPMENTDRONE_BODY_NAME":
                    var baseAI = self.characterBody.master.gameObject.GetComponent<BaseAI>();
                    if (!baseAI)
                    {
                        return;
                    }

                    var component = baseAI.gameObject.GetComponent<BEDUComponent>();
                    if (!component)
                    {
                        component = baseAI.gameObject.AddComponent<BEDUComponent>();
                        component.baseAI = baseAI;
                        component.equipmentSlot = self;
                    }
                    break;
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
                    self.bodyInputBank.sprint.PushState(true); //self.bodyInputs.pressSprint
                    self.bodyInputBank.activateEquipment.PushState(useEquipment);
                    self.bodyInputBank.moveVector = self.bodyInputs.moveVector;
                }
            } else
            {
                orig(self);
            }
        }
        public enum DroneMode
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
            [Tooltip("A reference to the component's BaseAI.")]
            public BaseAI baseAI = null;
            [Tooltip("The team that is opposite to the baseAI's team..")]
            public TeamIndex enemyTeamIndex = TeamIndex.None;
            [Tooltip("A reference to the body's equipmentSlot.")]
            public EquipmentSlot equipmentSlot;
            [Tooltip("The current EquipmentIndex.")]
            public EquipmentIndex equipmentIndex;
            [Tooltip("The current mode of the drone.")]
            public DroneMode droneMode = DroneMode.None;
            [Tooltip("Whether or not to fire the equipment.")]
            bool equipmentReady = false;

            [Tooltip("Whether or not the drone may freely use their equipment.")]
            public bool freeUse = false;
            [Tooltip("Whether or not the drone is forced to use their equipment.")]
            public bool useEquipment = false;

            [Tooltip("Speaking for debugging.")]
            bool hasSpoken = false;

            void Start()
            {
                enemyTeamIndex = baseAI.body.teamComponent.teamIndex == TeamIndex.Player ? TeamIndex.Monster : TeamIndex.Player;

                if (!equipmentSlot)
                {
                    if (baseAI.body && baseAI.body.inventory)
                        equipmentSlot = baseAI.body.equipmentSlot;
                }
                equipmentIndex = baseAI.master.inventory.currentEquipmentIndex;


                EvaluateDroneMode();
                _logger.LogMessage($"Chosen Drone Mode: {droneMode}");
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
                _logger.LogMessage($"Trying out EquipmentIndex {equipmentIndex}");
                if (DroneModeDictionary.TryGetValue(equipmentIndex, out DroneMode newDroneMode))
                {
                    droneMode = newDroneMode;
                } else
                {
                    droneMode = DroneMode.None;
                }
            }
            void ForceTarget(GameObject target)
            {
                baseAI.currentEnemy.gameObject = target;
                _logger.LogMessage($"{baseAI.body.GetDisplayName()} has switched targets to {baseAI.currentEnemy.gameObject.name}");
            }

            void FixedUpdate()
            {
                bool forceActive = false;
                freeUse = false;
                useEquipment = false;
                equipmentReady = equipmentSlot && equipmentSlot.stock > 0;
                if (!equipmentReady)
                {
                    hasSpoken = false;
                    return;
                }

                switch (droneMode)
                {
                    // If there are enemies on the map, but not necessarily any priority targets. //
                    case DroneMode.EnemyOnMap:
                        _logger.LogMessage("Checking for enemies on the stage.");
                        if (CheckForAliveOnTeam(enemyTeamIndex))
                        {
                            //DroneSay("There's enemies alive!");
                            _logger.LogMessage("Enemies on the stage found!");
                            forceActive = true;
                        }
                        break;
                    // If there is a high-value target, then it will prioritize that target before firing. //
                    // Priority: isBoss, isElite
                    // TODO: internal cooldown of 30 seconds, before allowing freeuse to prevent wasted fires before a priority enemy appears //
                    case DroneMode.PriorityTarget:
                        var priorityTarget = GetPriorityTarget(baseAI.master.teamIndex);
                        if (priorityTarget)
                        {
                            _logger.LogMessage($"Priority Target Found: {priorityTarget.name}");
                            ForceTarget(priorityTarget);
                            freeUse = true;
                        }
                        break;
                    // Attempts to evade, using its skills //
                    case DroneMode.Evade:
                        freeUse = true;
                        break;
                    // 
                    case DroneMode.GoldGat:
                        baseAI.master.money = uint.MaxValue;
                        break;
                    /* Priority Listing:
                     * Heal allied players in order of most hurt.
                     * TODO: Unless about to die (<10% health), then heal until 50%
                    */
                    case DroneMode.PassiveHealing:
                        var ally = GetMostHurtTeam(baseAI.body.teamComponent.teamIndex);
                        ForceTarget(ally);

                        forceActive = true;
                        break;
                    // If there are any debuffs, then use. //
                    case DroneMode.Cleanse:
                        if (CheckForDebuffs(baseAI.body))
                        {
                            DroneSay("I'm filthy! Cleaning!!");
                            forceActive = true;
                        }
                        break;
                    // Ideally we'd want to get in close, but I'd have to work on it //
                    case DroneMode.Saw:
                        freeUse = true;
                        break;
                    case DroneMode.Recycle:
                        GenericPickupController pickupController = equipmentSlot.currentTarget.pickupController;
                        if (pickupController && !pickupController.Recycled)
                        {
                            _logger.LogMessage($"Equipment Drone is currently looking at {PickupCatalog.GetPickupDef(pickupController.pickupIndex).internalName}");
                            PickupIndex initialPickupIndex = pickupController.pickupIndex;
                            if (allowedPickupIndices.Contains(initialPickupIndex))
                            {
                                //DroneSay("Bad Item/Equipment!!");
                                forceActive = true;
                            }
                        }
                        break;
                    case DroneMode.Fruit:
                        if (baseAI.body.healthComponent.health <= baseAI.body.healthComponent.fullHealth * 0.5f)
                        {
                            //DroneSay("I'm low health! Gonna heal!");
                            _logger.LogMessage($"Drone attempting to heal at {baseAI.body.healthComponent.combinedHealthFraction} health");
                            forceActive = true;
                        }
                        //forceActive = self.healthComponent?.health <= self.healthComponent?.fullHealth * 0.5f;
                        break;
                    // Tries to get as close as possible to the enemy //
                    case DroneMode.Snuggle:
                        freeUse = true;
                        break;
                    case DroneMode.Scan:
                        if (CheckForValidInteractables())
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
