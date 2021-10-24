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
using RoR2.DirectionalSearch;
using RoR2.Orbs;
using RoR2.Projectile;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace BetterEquipmentDroneUse
{
    [BepInPlugin("com.DestroyedClone.BetterEquipmentDroneUse", "Better Equipment Drone Use", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public static ConfigEntry<string> TargetPriority { get; set; }
        public static Type[] allowedTypesToScan = new Type[] { };
        public static ConfigEntry<string> Recycler_Items { get; set; }
        public static ConfigEntry<string> Recycler_Equipment { get; set; }

        public static List<ItemIndex> allowedItemIndices = new List<ItemIndex>();
        public static List<EquipmentIndex> allowedEquipmentIndices = new List<EquipmentIndex>();
        public static List<PickupIndex> allowedPickupIndices = new List<PickupIndex>();

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static BodyIndex EquipmentDroneBodyIndex;

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
            On.RoR2.EquipmentSlot.FindPickupController += EquipmentSlot_FindPickupController;
        }

        private GenericPickupController EquipmentSlot_FindPickupController(On.RoR2.EquipmentSlot.orig_FindPickupController orig, EquipmentSlot self, Ray aimRay, float maxAngle, float maxDistance, bool requireLoS, bool requireTransmutable)
        {
            if (self.characterBody && self.characterBody.bodyIndex == EquipmentDroneBodyIndex)
            {
                if (self.pickupSearch == null)
                {
                    self.pickupSearch = new PickupSearch();
                }
                aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, base.gameObject, out float num);
                self.pickupSearch.searchOrigin = aimRay.origin;
                self.pickupSearch.searchDirection = aimRay.direction;
                self.pickupSearch.minAngleFilter = 0f;
                self.pickupSearch.maxAngleFilter = 360;
                self.pickupSearch.minDistanceFilter = 0f;
                self.pickupSearch.maxDistanceFilter = maxDistance + num;
                self.pickupSearch.filterByDistinctEntity = false;
                self.pickupSearch.filterByLoS = requireLoS;
                self.pickupSearch.sortMode = SortMode.DistanceAndAngle;
                self.pickupSearch.requireTransmutable = requireTransmutable;


                foreach (var pickup in self.pickupSearch.candidateInfoList)
                {
                    if (pickup.source && !pickup.source.Recycled)
                    {
                        if (allowedPickupIndices.Contains(pickup.source.pickupIndex))
                        {
                            return pickup.source;
                        }
                    }
                }
                return self.pickupSearch.SearchCandidatesForSingleTarget(InstanceTracker.GetInstancesList<GenericPickupController>());
            }
            var original = orig(self, aimRay, maxAngle, maxDistance, requireLoS, requireTransmutable);

            return original;
        }

        private void GiveComponent(On.RoR2.EquipmentSlot.orig_OnStartServer orig, EquipmentSlot self)
        {
            orig(self);
            if (!self || !self.characterBody || !self.characterBody.master) return;
            if (self.characterBody.bodyIndex == EquipmentDroneBodyIndex)
            {
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
                    bool freeUseEquipment = component.freeUse && self.bodyInputs.pressActivateEquipment;
                    bool useEquipment = component.useEquipment || freeUseEquipment;
                    bool shouldJump = self.bodyInputs.pressJump || component.droneMode == DroneMode.Evade;

                    self.bodyInputBank.skill1.PushState(self.bodyInputs.pressSkill1);
                    self.bodyInputBank.skill2.PushState(self.bodyInputs.pressSkill2);
                    self.bodyInputBank.skill3.PushState(self.bodyInputs.pressSkill3);
                    self.bodyInputBank.skill4.PushState(self.bodyInputs.pressSkill4);
                    self.bodyInputBank.jump.PushState(shouldJump);
                    self.bodyInputBank.sprint.PushState(true); //self.bodyInputs.pressSprint

                    if (component.droneMode == DroneMode.Recycle)
                    {
                        GenericPickupController pickupController = self.body.equipmentSlot.currentTarget.pickupController;
                        if (pickupController && !pickupController.Recycled)
                        {
                            //_logger.LogMessage($"Equipment Drone is currently looking at {PickupCatalog.GetPickupDef(pickupController.pickupIndex).internalName}");
                            PickupIndex initialPickupIndex = pickupController.pickupIndex;
                            if (allowedPickupIndices.Contains(initialPickupIndex))
                            {
                                useEquipment = true;
                            }
                        }
                    }
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
            Scan,
            FuelArray
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

            [Tooltip("Position used to force the drone to look in a particular direction.")]
            public Vector3 newPositionToLook = Vector3.zero;

            void Start()
            {
                enemyTeamIndex = baseAI.master.teamIndex == TeamIndex.Player ? TeamIndex.Monster : TeamIndex.Player;
                
                if (!equipmentSlot)
                {
                    if (baseAI.body && baseAI.body.inventory)
                        equipmentSlot = baseAI.body.equipmentSlot;
                }
                equipmentIndex = baseAI.master.inventory.currentEquipmentIndex;

                EvaluateDroneMode(equipmentIndex);
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
            void ForceTarget(GameObject target)
            {
                baseAI.currentEnemy.gameObject = target;
                //_logger.LogMessage($"{baseAI.body.GetDisplayName()} has switched targets to {baseAI.currentEnemy.gameObject.name}");
            }

            void LookAtPosition(Vector3 position)
            {
                baseAI.bodyInputBank.aimDirection = baseAI.bodyInputBank.transform.position - position;
            }

            void FixedUpdate()
            {
                if (!baseAI)
                    return;

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
                    case DroneMode.EnemyOnMap:
                        //_logger.LogMessage("Checking for enemies on the stage.");
                        if (CheckForAliveOnTeam(enemyTeamIndex))
                        {
                            //DroneSay("There's enemies alive!");
                            //_logger.LogMessage("Enemies on the stage found!");
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
                            //_logger.LogMessage($"Priority Target Found: {priorityTarget.name}");
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
                        freeUse = true;
                        /*
                         * Why is this disabled?
                         * The cooldown of the blast shower (10s) gets reduced down to 20.589% of its value
                         * 2.0589.
                         * If the cooldown were longer then it could be justified to check for debuffs.
                         * But this way is cheaper.
                        if (CheckForDebuffs(baseAI.body))
                        {
                            forceActive = true;
                        }*/
                        break;
                    // Ideally we'd want to get in close, but I'd have to work on it //
                    case DroneMode.Saw:
                        freeUse = true;
                        break;
                    case DroneMode.Recycle:
                        freeUse = false;
                        break;
                        /*
                         * 
                         * Moving it to the main events due to the delay in use causing the wrong pickup to be affected.
                        GenericPickupController pickupController = equipmentSlot.currentTarget.pickupController;
                        if (pickupController && !pickupController.Recycled)
                        {
                            _logger.LogMessage($"Equipment Drone is currently looking at {PickupCatalog.GetPickupDef(pickupController.pickupIndex).internalName}");
                            PickupIndex initialPickupIndex = pickupController.pickupIndex;
                            if (allowedPickupIndices.Contains(initialPickupIndex))
                            {
                                //DroneSay("Bad Item/Equipment!!");
                                LookAtPosition(pickupController.transform.position);
                                forceActive = true;
                            }
                        }*/
                    case DroneMode.Fruit:
                        // CD: 45 -> ~10s
                        // It's fine to spam it.
                        freeUse = true;
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
                //if (forceActive) Debug.Log("attempting to use equipment");
                    useEquipment = forceActive && equipmentReady;
            }
        }
    }
}
