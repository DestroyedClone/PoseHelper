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
            On.RoR2.EquipmentSlot.OnStartServer += GiveComponent;
        }

        private void GiveComponent(On.RoR2.EquipmentSlot.orig_OnStartServer orig, EquipmentSlot self)
        {
            orig(self);
            if (!self) return;
            if (!self.characterBody) return;
            if (!self.characterBody.master) return;
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
            Debug.Log(allowedItemIndices);
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

        public static GameObject GetMostHurtAlly(TeamIndex teamIndex)
        {
            ReadOnlyCollection<TeamComponent> teamComponents = TeamComponent.GetTeamMembers(teamIndex);
            Dictionary<TeamComponent, float> keyValuePairs = new Dictionary<TeamComponent, float>();
            foreach (var ally in teamComponents)
            {
                if (ally.body?.healthComponent)
                {
                    keyValuePairs.Add(ally, ally.body.healthComponent.health / ally.body.healthComponent.fullHealth);
                }
            }
            // https://stackoverflow.com/questions/23734686/c-sharp-dictionary-get-the-key-of-the-min-value
            var min = keyValuePairs.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
            return null;
        }


        public static bool CheckInteractables()
        {
            Type[] array = ChestRevealer.typesToCheck;
            for (int i = 0; i < array.Length; i++)
            {
                foreach (MonoBehaviour monoBehaviour in InstanceTracker.FindInstancesEnumerable(array[i]))
                {
                    if (((IInteractable)monoBehaviour).ShouldShowOnScanner())
                    {
                        Debug.Log("interactable check 1");
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckForInteractables()
        {
            ///Type[] validInteractables = new Type[] {  };
            foreach (var valid in allowedTypesToScan)
            {
                InstanceTracker.FindInstancesEnumerable(valid);
                if (((IInteractable)valid).ShouldShowOnScanner())
                {
                    Debug.Log("interactable check 2");
                    return true;
                }
            }
            return false;
        }

        public static bool CheckForDebuffs(CharacterBody characterBody)
        {
            BuffIndex buffIndex = 0;
            BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
            while (buffIndex < buffCount)
            {
                BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                if (buffDef.isDebuff && characterBody.HasBuff(buffIndex))
                {
                    if (characterBody.HasBuff(buffIndex))
                    {
                        Debug.Log("debuffs!");
                        return true;
                    }
                }
                buffIndex++;
            }
            Debug.Log("no debuffs?!");
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
            [Tooltip("A reference to the component's BaseAI.")]
            public BaseAI baseAI = null;
            [Tooltip("The team that is opposite to the baseAI's team..")]
            public TeamIndex enemyTeamIndex = TeamIndex.None;
            [Tooltip("A reference to the body's equipmentSlot.")]
            public EquipmentSlot equipmentSlot;
            [Tooltip("The current EquipmentIndex.")]
            EquipmentIndex equipmentIndex;
            [Tooltip("The current mode of the drone.")]
            DroneMode droneMode = DroneMode.None;
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
            void TargetAlly(GameObject ally)
            {
                baseAI.currentEnemy.gameObject = ally;
            }

            void FixedUpdate()
            {
                bool forceActive = false;
                freeUse = false;
                useEquipment = false;
                equipmentReady = equipmentSlot.stock > 0;
                if (!equipmentReady)
                {
                    hasSpoken = false;
                    return;
                }

                switch (droneMode)
                {
                    // If there are enemies on the map, but not necessarily any priority targets. //
                    case DroneMode.EnemyOnMap:
                        if (CheckForAlive(enemyTeamIndex))
                        {
                            DroneSay("There's enemies alive!");
                            forceActive = true;
                        }
                        break;
                    // If there is a high-value target, then it will prioritize that target before firing. //
                    // internal cooldown of 30 seconds, before allowing freeuse to prevent wasted fires before a priority enemy appears //
                    case DroneMode.PriorityTarget:
                        freeUse = true;
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
                     * Unless about to die (<10% health), then heal until 50%
                    */
                    case DroneMode.PassiveHealing:
                        var ally = GetMostHurtAlly(baseAI.body.teamComponent.teamIndex);
                        TargetAlly(ally);

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
                    // Tries to get as close as possible to the enemy //
                    case DroneMode.Snuggle:
                        freeUse = true;
                        break;
                    case DroneMode.Scan:
                        if (CheckForInteractables() || CheckInteractables())
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
