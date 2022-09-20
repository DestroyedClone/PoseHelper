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
using System.Linq;
using RoR2.Orbs;
using AlternateSkills.Modules;

namespace AlternateSkills.Acrid
{
    public class AcridMain : SurvivorMain
    {
        public override string CharacterName => "Croco";
        public string TokenPrefix = "DCALTSKILLS_CROCO";

        public override void Init(ConfigFile config)
        {
            return;
        }

        public override void Hooks()
        {
            base.Hooks();
            GlobalEventManager.onServerDamageDealt += CrocoApplyRemoteDisease;
        }

        public void CrocoApplyRemoteDisease(DamageReport damageReport)
        {
            if (damageReport.damageInfo.HasModdedDamageType(DamageTypes.DTCrocoPoisonCountdown))
            {
                if (damageReport.victimBody)
                {
                    damageReport.victimBody.AddBuff(Buffs.crocoRemotePoisonDebuff);
                    damageReport.victimBody.gameObject.AddComponent<CrocoRemotePoisonInfo>().Assign(damageReport.attackerBody, damageReport.victimBody);
                }
            }
        }

        public class CrocoRemotePoisonInfo : MonoBehaviour
        {
            //Sucks for now.
            public CharacterBody owner;
            public CharacterBody victim;
            private float stopwatch = 0f;
            private float duration = 5f;
            private bool castedPoison = false;

            public void Assign(CharacterBody getOwner, CharacterBody getVictim)
            {
                owner = getOwner;
                victim = getVictim;
            }

            public void FixedUpdate()
            {
                if (castedPoison)
                    return;
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= duration)
                {
                    castedPoison = true;
                    FireDisease();
                }
            }

            public void FireDisease()
            {
                BullseyeSearch bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.searchOrigin = victim.corePosition;
                bullseyeSearch.searchDirection = Vector3.up;
                bullseyeSearch.maxDistanceFilter = EntityStates.Croco.Disease.orbRange;
                bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
                bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(base.gameObject));
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
                bullseyeSearch.RefreshCandidates();
                EffectManager.SimpleMuzzleFlash(EntityStates.Croco.Disease.muzzleflashEffectPrefab, base.gameObject, EntityStates.Croco.Disease.muzzleString, true);
                List<HurtBox> list = bullseyeSearch.GetResults().ToList<HurtBox>();
                if (list.Count > 0)
                {
                    /*Debug.LogFormat("Shooting at {0}", new object[]
                    {
                        list[0]
                    });*/
                    HurtBox target = list.FirstOrDefault<HurtBox>();
                    LightningOrb lightningOrb = new LightningOrb();
                    lightningOrb.attacker = base.gameObject;
                    lightningOrb.bouncedObjects = new List<HealthComponent>();
                    lightningOrb.lightningType = LightningOrb.LightningType.CrocoDisease;
                    lightningOrb.damageType = DamageType.PoisonOnHit;
                    lightningOrb.damageValue = owner.damage * EntityStates.Croco.Disease.damageCoefficient;
                    lightningOrb.isCrit = owner.RollCrit();
                    lightningOrb.procCoefficient = EntityStates.Croco.Disease.procCoefficient;
                    lightningOrb.bouncesRemaining = EntityStates.Croco.Disease.maxBounces;
                    lightningOrb.origin = transform.position;
                    lightningOrb.target = target;
                    lightningOrb.teamIndex = owner.teamComponent.teamIndex;
                    lightningOrb.range = EntityStates.Croco.Disease.bounceRange;
                    OrbManager.instance.AddOrb(lightningOrb);
                }
            }

        }

        public override void SetupPrimary()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESBite));
            mySkillDef.activationStateMachineName = "Mouth";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 0;
            //mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
            mySkillDef.skillName = "DCALTSKILLS_CROCO_PRIMARY_CHEW";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            primarySkillDefs.Add(mySkillDef);
            base.SetupPrimary();
        }

        public override void SetupSecondary()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESFang));
            mySkillDef.activationStateMachineName = "Mouth";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 5;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
            mySkillDef.skillName = "DCALTSKILLS_CROCO_SECONDARY_BITE";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            secondarySkillDefs.Add(mySkillDef);
            base.SetupSecondary();
        }

        public override void SetupUtility()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESKickOff));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 8;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
            mySkillDef.skillName = "DCALTSKILLS_CROCO_UTILITY_KICKOFF";
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
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESBite));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 8;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
            mySkillDef.skillName = "DCALTSKILLS_CROCO_SPECIAL_GOUGE";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            utilitySkillDefs.Add(mySkillDef);
            base.SetupSpecial();
        }
    }
}
