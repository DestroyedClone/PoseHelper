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
        public BodyIndex EquipmentDroneBodyIndex = BodyIndex.None;
        public Type[] allowedTypesToScan = new Type[] { };
        public static ConfigEntry<string> Recycler_Items { get; set; }
        public static ConfigEntry<string> Recycler_Equipment { get; set; }

        public List<ItemIndex> allowedItemIndices = new List<ItemIndex>();
        public List<EquipmentIndex> allowedEquipmentIndices = new List<EquipmentIndex>();
        public List<PickupIndex> allowedPickupIndices = new List<PickupIndex>();
        public void Awake()
        {
            Recycler_Items = Config.Bind("Recycler", "Item IDS", "Tooth,Seed,Icicle,GhostOnKill,BounceNearby,MonstersOnShrineUse", "Enter the IDs of the item you want equipment drones to recycle." +
    "\nSeparated by commas (ex: AffixRed,Meteor,Fruit)");
            Recycler_Equipment = Config.Bind("Recycler", "Equipment IDS", "Meteor,CritOnUse,GoldGat,Scanner,Gateway", "Enter the IDs of the equipment you want equipment drones to recycle." +
    "\nSeparated by commas (ex: AffixRed,Meteor,Fruit)");


            On.RoR2.EquipmentSlot.FixedUpdate += EquipmentSlot_FixedUpdate;
            var body = Resources.Load<GameObject>("prefabs/characterbodies/EquipmentDroneBody");
            EquipmentDroneBodyIndex = body.GetComponent<CharacterBody>().bodyIndex;
            On.RoR2.ChestRevealer.Init += GetAllowedTypes;

            //On.RoR2.CharacterAI.BaseAI.FixedUpdate += BaseAIOverride;
            //On.EntityStates.GoldGat.GoldGatIdle.FixedUpdate += FixedGoldGat;
            On.RoR2.ItemCatalog.Init += CacheWhitelistedItems;
            On.RoR2.EquipmentCatalog.Init += CacheWhitelistedEquipment;
            On.RoR2.PickupCatalog.Init += CachePickupIndices;
            //On.RoR2.CharacterAI.BaseAI.UpdateBodyAim += BaseAI_UpdateBodyAim;
        }

        private void BaseAI_UpdateBodyAim(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyAim orig, BaseAI self, float deltaTime)
        {
            if (self.body.bodyIndex != EquipmentDroneBodyIndex && self.body.equipmentSlot.equipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex)
            {
                orig(self, deltaTime);
                return;
            }
            self.hasAimConfirmation = false;
            if (self.bodyInputBank)
            {
                Vector3 aimDirection = self.bodyInputBank.aimDirection;
                Vector3 desiredAimDirection = self.bodyInputs.desiredAimDirection;
                if (desiredAimDirection != Vector3.zero)
                {


                    Quaternion target = Util.QuaternionSafeLookRotation(desiredAimDirection);
                    Vector3 vector = Util.SmoothDampQuaternion(Util.QuaternionSafeLookRotation(aimDirection), target, ref self.aimVelocity, self.aimVectorDampTime, self.aimVectorMaxSpeed, deltaTime) * Vector3.forward;
                    self.bodyInputBank.aimDirection = vector;
                    self.hasAimConfirmation = (Vector3.Dot(vector, desiredAimDirection) >= 0.95f);
                }
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


        private void EquipmentSlot_FixedUpdate(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, EquipmentSlot self)
        {
            orig(self);
            if (self.characterBody || self.characterBody.bodyIndex != EquipmentDroneBodyIndex) return;
            var baseAI = self.gameObject.GetComponent<BaseAI>();
            if (!baseAI) return;
            TeamIndex enemyTeamIndex = self.teamComponent.teamIndex == TeamIndex.Player ? TeamIndex.Monster : TeamIndex.Player;
            bool forceActive = false;

            bool match(EquipmentDef equipmentDef)
            {
                return self.equipmentIndex == equipmentDef.equipmentIndex;
            }

            // Enemy On Map
            // If there are enemies alive, use.
            if (match(RoR2Content.Equipment.CommandMissile) || match(RoR2Content.Equipment.Meteor))
            {
                forceActive = CheckForAlive(enemyTeamIndex);
            }
            // Priority Target
            // Prioritizes a certain enemy rather than firing blindly
            // Overrides BaseAI
            else if (match(RoR2Content.Equipment.Blackhole) || match(RoR2Content.Equipment.BFG) || match(RoR2Content.Equipment.Lightning) || match(RoR2Content.Equipment.CrippleWard))
            {

            }

            // Evade or Aggro
            // Attempts to draw enemy attention
            else if (match(RoR2Content.Equipment.Jetpack)) //Spam Jump
            {

            }
            else if (match(RoR2Content.Equipment.GainArmor)) //Kite
            {

            }
            else if (match(RoR2Content.Equipment.Tonic)) //Kite
            {

            }
            // Custom
            else if (match(RoR2Content.Equipment.GoldGat))
            {
                uint num2 = (uint)((float)GoldGatFire.baseMoneyCostPerBullet * (1f + (TeamManager.instance.GetTeamLevel(self.characterBody.master.teamIndex) - 1f) * 0.25f));
                self.characterBody.master.money = num2 * 60;
            }
            else if (match(RoR2Content.Equipment.PassiveHealing)) //Target damaged ally, or self 
            {

            }
            else if (match(RoR2Content.Equipment.Gateway)) // Target Interactables or nearby if damaged
            {

            }
            else if (match(RoR2Content.Equipment.Cleanse))
            {
                forceActive = CheckForDebuffs(self.characterBody);
            }
            else if (match(RoR2Content.Equipment.Saw))//get close
            {

            }
            else if (match(RoR2Content.Equipment.Recycle))
            {
                GenericPickupController pickupController = self.currentTarget.pickupController;
                if (!pickupController || pickupController.Recycled)
                {
                    //break
                }
                PickupIndex initialPickupIndex = pickupController.pickupIndex;
                if (allowedPickupIndices.Contains(initialPickupIndex))
                {
                    forceActive = true;
                }
                //break;
            }
            else if (match(RoR2Content.Equipment.Fruit))
            {
                forceActive = self.healthComponent?.health <= self.healthComponent?.fullHealth * 0.5f;
            }
            // chase
            // fireball dash already done
            else if (match(RoR2Content.Equipment.BurnNearby) || match(RoR2Content.Equipment.QuestVolatileBattery))
            {

            }
            // valid interactables
            else if (match(RoR2Content.Equipment.Scanner))
            {
                forceActive = CheckForInteractables();
            }

            if (forceActive)
            {
                ForceEquipmentUse(baseAI);
            }
        }

        private bool CheckForAlive(TeamIndex teamIndex)
        {
            ReadOnlyCollection<TeamComponent> teamComponents = TeamComponent.GetTeamMembers(teamIndex);
            return teamComponents.Count > 0;
        }



        private void ActivateWhenReady(EquipmentSlot equipmentSlot)
        {
            //equipmentslot L299
            bool isEquipmentActivationAllowed = equipmentSlot.characterBody.isEquipmentActivationAllowed;
            if (isEquipmentActivationAllowed && equipmentSlot.hasEffectiveAuthority)
            {
                if (NetworkServer.active)
                {
                    equipmentSlot.ExecuteIfReady();
                    return;
                }
                equipmentSlot.CallCmdExecuteIfReady();
            }
        }

        private bool CheckForInteractables()
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

        private void ForceEquipmentUse(BaseAI baseAI)
        {
            baseAI.bodyInputBank.activateEquipment.PushState(true);
        }

        private void ForceJump(BaseAI baseAI)
        {
            baseAI.bodyInputBank.jump.PushState(true);
        }

        private bool CheckForDebuffs(CharacterBody characterBody) //try hooking addbuff instead?
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
    }
}
