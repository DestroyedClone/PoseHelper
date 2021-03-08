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
        public void Awake()
        {
            On.RoR2.EquipmentSlot.FixedUpdate += EquipmentSlot_FixedUpdate;
            var body = Resources.Load<GameObject>("prefabs/characterbodies/EquipmentDroneBody");
            EquipmentDroneBodyIndex = body.GetComponent<CharacterBody>().bodyIndex;
            On.RoR2.ChestRevealer.Init += GetAllowedTypes;

            //On.RoR2.CharacterAI.BaseAI.FixedUpdate += BaseAIOverride;
            On.EntityStates.GoldGat.GoldGatIdle.FixedUpdate += FixedGoldGat;
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
                    break;
                case GainArmor: //runs away in a radisu
                    break;
                case Tonic:
                    break;
                //Custom Logic
                case GoldGat: //Forced Equipment State
                    break;
                case PassiveHealing: //Target damaged ally
                    break;
                case Gateway: // Target Interactables or nearby if damaged
                    break;
                case Cleanse: //Prioritize projectiles
                    break;
                case Saw: //get close
                    break;
                case Recycle: //look at polyp
                    break;

                //Health Requirement
                case Fruit:
                    break;
                //Chase Priority
                case BurnNearby:
                case QuestVolatileBattery:
                    break;
                //(FireBallDash)

                //Valid interactables
                case Scanner:
                    if (CheckForInteractables())
                    {
                        forceActive = true;
                    }
                    break;
            }

            if (forceActive)
            {
                ActivateWhenReady(self);
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

    }
}
