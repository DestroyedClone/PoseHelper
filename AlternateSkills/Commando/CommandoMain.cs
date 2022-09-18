using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using AlternateSkills.Modules;

namespace AlternateSkills.Commando
{
    public class CommandoMain : SurvivorMain
    {
        public override string CharacterName => "Commando";
        public string TokenPrefix = "DCALTSKILLS_COMMANDO";

        public class CommandoSquadController : MonoBehaviour
        {
            public CharacterBody owner;
            private float stopwatch = 0;
            public float frequency = 6;
            public bool shouldHaveBuff = false;
            public SphereSearch sphereSearch = new SphereSearch();

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= frequency)
                {
                    stopwatch = 0;
                    CheckForNearbyAllies();
                    EvaluateBuff();
                }
            }

            public void CheckForNearbyAllies()
            {
                sphereSearch.origin = owner.corePosition;
                sphereSearch.radius = 5;
                sphereSearch.RefreshCandidates();
                var friendlyTeamMask = TeamMask.none;
                friendlyTeamMask.AddTeam(owner.teamComponent.teamIndex);
                sphereSearch.FilterCandidatesByHurtBoxTeam(friendlyTeamMask);
                List<Collider> colliders = new List<Collider>();
                sphereSearch.searchData.GetColliders(colliders);
                shouldHaveBuff = colliders.Count > 1;
            }
            public void EvaluateBuff()
            {
                bool ownerHasBuff = owner.HasBuff(Buffs.commandoSquadronBuff);
                if (shouldHaveBuff)
                {
                    if (!ownerHasBuff)
                    {
                        owner.AddBuff(Buffs.commandoSquadronBuff);
                    }
                } else {
                    if (ownerHasBuff)
                    {
                        owner.RemoveBuff(Buffs.commandoSquadronBuff);
                    }
                }
            }
        }

        public override void Hooks()
        {
            base.Hooks();
            On.RoR2.BulletAttack.Fire += CommandoReinforceCopyBullet;
        }

        public void CommandoReinforceCopyBullet(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self)
        {
            orig(self);
            if (self.owner)
            {
                var comp = self.owner.GetComponent<CommandoReinforcementComponent>();
                if (comp)
                {
                    comp.FireBullet(self);
                }
            }
        }

        public override void SetupPassive()
        {
            SurvivorSkillLocator.passiveSkill.skillNameToken = TokenPrefix+"_PASSIVE_NAME";
            SurvivorSkillLocator.passiveSkill.skillDescriptionToken = TokenPrefix+"_PASSIVE_DESC";
            BodyPrefab.AddComponent<CommandoSquadController>().owner = BodyPrefab.GetComponent<CharacterBody>();
            BodyPrefab.AddComponent<CommandoReinforcementComponent>().casterBody = BodyPrefab.GetComponent<CharacterBody>();
            base.SetupPassive();
        }
        
        public override void SetupUtility()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESEDFRun));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 0;
            //mySkillDef.icon = SurvivorSkillLocator.utility.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_UTILITY";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            utilitySkillDefs.Add(mySkillDef);
            base.SetupUtility();
        }

        public override void SetupSpecial()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESReinforcement));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 20;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.special.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_SPECIAL";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            utilitySkillDefs.Add(mySkillDef);
            base.SetupSpecial();
        }
    }
    public class CommandoReinforcementComponent : MonoBehaviour
    {
        public CharacterBody casterBody;
        public List<CharacterBody> targetedBodies = new List<CharacterBody>();

        float stopwatch = 0;
        float reinforcementDuration = 7;
        static BuffDef reinforcementVisualBuff = Buffs.commandoReinforcingVisualBuff;

        public bool AddTarget(CharacterBody characterBody)
        {
            if (!targetedBodies.Contains(characterBody))
            {
                targetedBodies.Add(characterBody);
                casterBody.AddBuff(reinforcementVisualBuff);
                return true;
            }
            return false;
        }

        public BulletAttack CopyBulletAttack(BulletAttack bulletAttack)
        {
            BulletAttack copy = new BulletAttack();
                copy._aimVector = bulletAttack._aimVector;
                copy._maxDistance = bulletAttack.maxDistance;
                copy.aimVector = bulletAttack.aimVector;
                copy.bulletCount = bulletAttack.bulletCount;
                copy.damage = bulletAttack.damage;
                copy.damageColorIndex = bulletAttack.damageColorIndex;
                copy.damageType = bulletAttack.damageType;
                copy.falloffModel = bulletAttack.falloffModel;
                copy.filterCallback = bulletAttack.filterCallback;
                copy.force = bulletAttack.force;
                copy.hitCallback = bulletAttack.hitCallback;
                copy.HitEffectNormal = bulletAttack.HitEffectNormal;
                copy.hitEffectPrefab = bulletAttack.hitEffectPrefab;
                copy.hitMask = bulletAttack.hitMask;
                copy.isCrit = bulletAttack.isCrit;
                copy.maxDistance = bulletAttack.maxDistance;
                copy.maxSpread = bulletAttack.maxSpread;
                copy.minSpread = bulletAttack.minSpread;
                copy.modifyOutgoingDamageCallback = bulletAttack.modifyOutgoingDamageCallback;
                copy.muzzleName = bulletAttack.muzzleName;
                copy.owner = bulletAttack.owner;
                copy.procChainMask = bulletAttack.procChainMask;
                copy.procCoefficient = bulletAttack.procCoefficient;
                copy.queryTriggerInteraction = bulletAttack.queryTriggerInteraction;
                copy.radius = bulletAttack.radius;
                copy.smartCollision = bulletAttack.smartCollision;
                copy.sniper = bulletAttack.sniper;
                copy.spreadPitchScale = bulletAttack.spreadPitchScale;
                copy.spreadYawScale = bulletAttack.spreadYawScale;
                copy.stopperMask = bulletAttack.stopperMask;
                copy.tracerEffectPrefab = bulletAttack.tracerEffectPrefab;
                //copy.weapon = bulletAttack.weapon;
            return copy;
        }

        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch > reinforcementDuration)
            {
                if (targetedBodies.Count > 0)
                {
                    casterBody.RemoveBuff(reinforcementVisualBuff);
                    targetedBodies.RemoveAt(0);
                }
                stopwatch = 0;
            }
        }

        public void FireBullet(BulletAttack bulletAttack)
        {
            if (targetedBodies.Count == 0)
                return;
            
            var attackCopy = CopyBulletAttack(bulletAttack);
            foreach (var target in targetedBodies)
            {
                if (!target)
                    continue;
                attackCopy.origin = target.corePosition;
                bulletAttack.Fire();
            }
        }
    }
}
