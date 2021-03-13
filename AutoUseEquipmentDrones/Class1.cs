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
        public int EquipmentDroneBodyIndex = -1;
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
            On.EntityStates.GoldGat.GoldGatIdle.FixedUpdate += FixedGoldGat;
            On.RoR2.ItemCatalog.Init += CacheWhitelistedItems;
            On.RoR2.EquipmentCatalog.Init += CacheWhitelistedEquipment;
            On.RoR2.PickupCatalog.Init += CachePickupIndices;
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
            var testStringArray = Recycler_Items.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (ItemCatalog.FindItemIndex(stringToTest) == ItemIndex.None) { continue; }
                    allowedItemIndices.Add(ItemCatalog.FindItemIndex(stringToTest));
                }
            }
        }

        private void CacheWhitelistedEquipment(On.RoR2.EquipmentCatalog.orig_Init orig)
        {
            orig();
            var testStringArray = Recycler_Equipment.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (EquipmentCatalog.FindEquipmentIndex(stringToTest) == EquipmentIndex.None) { continue; }
                    allowedEquipmentIndices.Add(EquipmentCatalog.FindEquipmentIndex(stringToTest));
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

        private void BaseAIOverride(On.RoR2.CharacterAI.BaseAI.orig_FixedUpdate orig, BaseAI self)
        {
            if (self.GetComponent<CharacterBody>()?.bodyIndex != EquipmentDroneBodyIndex)
            {
                orig(self);
                return;
            }
            self.enemyAttention -= Time.fixedDeltaTime;
            if (self.currentEnemy.characterBody && self.body && self.currentEnemy.characterBody.GetVisibilityLevel(self.body) < VisibilityLevel.Revealed)
            {
                self.currentEnemy.Reset();
            }
            if (self.pendingPath != null && self.pendingPath.status == PathTask.TaskStatus.Complete)
            {
                self.pathFollower.SetPath(self.pendingPath.path);
                self.pendingPath.path.Dispose();
                self.pendingPath = null;
            }
            if (self.body)
            {
                self.targetRefreshTimer -= Time.fixedDeltaTime;
                self.skillDriverUpdateTimer -= Time.fixedDeltaTime;
                if (self.skillDriverUpdateTimer <= 0f)
                {
                    if (self.skillDriverEvaluation.dominantSkillDriver)
                    {
                        self.selectedSkilldriverName = self.skillDriverEvaluation.dominantSkillDriver.customName;
                        if (self.skillDriverEvaluation.dominantSkillDriver.resetCurrentEnemyOnNextDriverSelection)
                        {
                            self.currentEnemy.Reset();
                            self.targetRefreshTimer = 0f;
                        }
                    }
                    if (!self.currentEnemy.gameObject && self.targetRefreshTimer <= 0f)
                    {
                        self.targetRefreshTimer = 0.5f;
                        HurtBox hurtBox = self.FindEnemyHurtBox(float.PositiveInfinity, self.fullVision, true);
                        if (hurtBox && hurtBox.healthComponent)
                        {
                            self.currentEnemy.gameObject = hurtBox.healthComponent.gameObject;
                            self.currentEnemy.bestHurtBox = hurtBox;
                        }
                        if (self.currentEnemy.gameObject)
                        {
                            self.enemyAttention = self.enemyAttentionDuration;
                        }
                    }
                    self.BeginSkillDriver(self.EvaluateSkillDrivers());
                }
            }
            self.PickCurrentNodeGraph();
            if (self.bodyInputBank)
            {
                bool newState = false;
                bool newState2 = false;
                if (self.skillDriverEvaluation.dominantSkillDriver)
                {
                    AISkillDriver.AimType aimType = self.skillDriverEvaluation.dominantSkillDriver.aimType;
                    if (aimType != AISkillDriver.AimType.None)
                    {
                        BaseAI.Target target = null;
                        switch (aimType)
                        {
                            case AISkillDriver.AimType.AtMoveTarget:
                                target = self.skillDriverEvaluation.target;
                                break;
                            case AISkillDriver.AimType.AtCurrentEnemy:
                                target = self.currentEnemy;
                                break;
                            case AISkillDriver.AimType.AtCurrentLeader:
                                target = self.leader;
                                break;
                        }
                        if (target != null)
                        {
                            if (target.GetBullseyePosition(out Vector3 a))
                            {
                                self.desiredAimDirection = (a - self.bodyInputBank.aimOrigin).normalized;
                            }
                            newState = (self.skillDriverEvaluation.dominantSkillDriver.shouldFireEquipment && !self.bodyInputBank.activateEquipment.down);
                        }
                        else if (self.bodyInputBank.moveVector != Vector3.zero)
                        {
                            self.desiredAimDirection = self.bodyInputBank.moveVector;
                        }
                    }
                    newState2 = self.skillDriverEvaluation.dominantSkillDriver.shouldSprint;
                }
                self.bodyInputBank.activateEquipment.PushState(newState);
                self.bodyInputBank.sprint.PushState(newState2);
                Vector3 aimDirection = self.bodyInputBank.aimDirection;
                Vector3 eulerAngles = Util.QuaternionSafeLookRotation(self.desiredAimDirection).eulerAngles;
                Vector3 eulerAngles2 = Util.QuaternionSafeLookRotation(aimDirection).eulerAngles;
                float fixedDeltaTime = Time.fixedDeltaTime;
                float x = Mathf.SmoothDampAngle(eulerAngles2.x, eulerAngles.x, ref self.aimVelocity.x, self.aimVectorDampTime, self.aimVectorMaxSpeed, fixedDeltaTime);
                float y = Mathf.SmoothDampAngle(eulerAngles2.y, eulerAngles.y, ref self.aimVelocity.y, self.aimVectorDampTime, self.aimVectorMaxSpeed, fixedDeltaTime);
                float z = Mathf.SmoothDampAngle(eulerAngles2.z, eulerAngles.z, ref self.aimVelocity.z, self.aimVectorDampTime, self.aimVectorMaxSpeed, fixedDeltaTime);
                self.bodyInputBank.aimDirection = Quaternion.Euler(x, y, z) * Vector3.forward;
                self.hasAimConfirmation = (Vector3.Dot(self.bodyInputBank.aimDirection, self.desiredAimDirection) >= 0.95f);
            }
            self.debugEnemyHurtBox = self.currentEnemy.bestHurtBox;
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

            switch (self.equipmentIndex)
            {
                // Enemy On Map
                // If there are enemies alive, use.
                case CommandMissile:
                case Meteor:
                    forceActive = CheckForEnemies(enemyTeamIndex);
                    break;
                // Priority Target
                // Prioritizes a certain enemy rather than firing blindly
                // Overrides BaseAI
                case Blackhole:
                case BFG:
                case Lightning:
                case CrippleWard:
                    break;
                // Evade or Aggro
                // Attempts to draw enemy attention
                case Jetpack: // spam jump
                    ForceJump(baseAI);
                    break;
                case GainArmor: //runs away in a radisu
                    break;
                case Tonic:
                    break;
                //Custom Logic
                case GoldGat: //Forced Equipment State and money
                    uint num2 = (uint)((float)GoldGatFire.baseMoneyCostPerBullet * (1f + (TeamManager.instance.GetTeamLevel(self.characterBody.master.teamIndex) - 1f) * 0.25f));
                    self.characterBody.master.money = num2*60;
                    break;
                case PassiveHealing: //Target damaged ally
                    break;
                case Gateway: // Target Interactables or nearby if damaged
                    break;
                case Cleanse: //Activate if debuffed
                    forceActive = CheckForDebuffs(self.characterBody);
                    break;
                case Saw: //get close
                    break;
                case Recycle: //look at polyp
                    //self.UpdateTargets();
                    GenericPickupController pickupController = self.currentTarget.pickupController;
                    if (!pickupController || pickupController.Recycled)
                    {
                        break;
                    }
                    PickupIndex initialPickupIndex = pickupController.pickupIndex;
                    if (allowedPickupIndices.Contains(initialPickupIndex))
                    {
                        forceActive = true;
                    }
                    break;

                //Health Requirement
                case Fruit:
                    forceActive = self.healthComponent?.health <= self.healthComponent?.fullHealth * 0.5f;
                    break;
                //Chase Priority
                case BurnNearby:
                case QuestVolatileBattery:
                    break;
                //(FireBallDash)

                //Valid interactables
                case Scanner:
                    forceActive = CheckForInteractables();
                    break;
            }

            if (forceActive)
            {
                ForceEquipmentUse(baseAI);
            }
        }

        private bool CheckForEnemies(TeamIndex teamIndex)
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
            BuffIndex buffIndex = BuffIndex.Slow50;
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
