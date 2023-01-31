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
using JetBrains.Annotations;
using AlternateSkills.Modules;

namespace AlternateSkills.Merc
{
    public class MercenaryMain : SurvivorMain
    {
        public override string CharacterName => "Merc";
        public string TokenPrefix = "DCALTSKILLS_MERC";
        public const float adrenalineDuration = 1f;
        public static BuffDef adrenalineBuff => Buffs.mercAdrenalineBuff;

        public override void SetupPassive()
        {
            base.SetupPassive();
            BodyPrefab.GetComponent<CharacterBody>().baseJumpCount = 1;
            SurvivorSkillLocator.passiveSkill.skillNameToken = TokenPrefix+"_PASSIVE_NAME";
            SurvivorSkillLocator.passiveSkill.skillDescriptionToken = TokenPrefix+"_PASSIVE_DESC";

            
            //On.EntityStates.Merc.Weapon.GroundLight2.OnMeleeHitAuthority += MercAdrenPrimary;
            On.EntityStates.Merc.Weapon.GroundLight2.OnEnter += MercAdrenGroundLight;
            On.EntityStates.Merc.WhirlwindBase.OnEnter += MercAdrenWhirlwind;
            On.EntityStates.Merc.Uppercut.OnEnter += MercAdrenUppercut;
            On.EntityStates.Merc.FocusedAssaultDash.OnMeleeHitAuthority += MercAdrenDash;
            
        }

        public override void Hooks()
        {
            base.Hooks();
            R2API.RecalculateStatsAPI.GetStatCoefficients += MercBuffs;
            On.RoR2.Skills.SkillDef.CanExecute += MercPeaceNoAttack;

            //Passive Activations
            BodyPrefab.AddComponent<DCMercEchoComponent>().owner = BodyPrefab.GetComponent<CharacterBody>();
        }

        #region Adrenaline
        public void MercAdrenGroundLight(On.EntityStates.Merc.Weapon.GroundLight2.orig_OnEnter orig, EntityStates.Merc.Weapon.GroundLight2 self)
        {
            orig(self);
            RollForAdrenaline(self.characterBody);
            TrackEcho(self.characterBody, "", self.damageCoefficient * self.damageStat);
        }
        public void MercAdrenPrimary(On.EntityStates.Merc.Weapon.GroundLight2.orig_OnMeleeHitAuthority orig, EntityStates.Merc.Weapon.GroundLight2 self)
        {
            orig(self);
            RollForAdrenaline(self.characterBody);
            TrackEcho(self.characterBody, "", self.damageCoefficient * self.damageStat);
        }
        public void MercAdrenWhirlwind(On.EntityStates.Merc.WhirlwindBase.orig_OnEnter orig, EntityStates.Merc.WhirlwindBase self)
        {
            orig(self);
            RollForAdrenaline(self.characterBody);
            TrackEcho(self.characterBody, "", self.baseDamageCoefficient * self.damageStat);
        }
        public void MercAdrenUppercut(On.EntityStates.Merc.Uppercut.orig_OnEnter orig, EntityStates.Merc.Uppercut self)
        {
            orig(self);
            RollForAdrenaline(self.characterBody);
            TrackEcho(self.characterBody, "", EntityStates.Merc.Uppercut.baseDamageCoefficient * self.damageStat);
        }
        public void MercAdrenDash(On.EntityStates.Merc.FocusedAssaultDash.orig_OnMeleeHitAuthority orig, EntityStates.Merc.FocusedAssaultDash self)
        {
            orig(self);
            RollForAdrenaline(self.characterBody);
            TrackEcho(self.characterBody, "", self.damageCoefficient * self.damageStat);
        }
        #endregion

        public static void TrackEcho(CharacterBody characterBody, string muzzle, float damage)
        {
            var component = characterBody.GetComponent<DCMercEchoComponent>();
            if (component)
            {
                component.Track(characterBody.corePosition, muzzle, damage, false);
            }
        }

        public bool MercPeaceNoAttack(On.RoR2.Skills.SkillDef.orig_CanExecute orig, SkillDef self, GenericSkill skillSlot) {
            if (skillSlot.characterBody && skillSlot.characterBody.HasBuff(Buffs.mercPeaceBuff))
            {
                return false;
            }
            return orig(self, skillSlot);
        }

        public void MercBuffs(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Buffs.mercAdrenalineBuff))
            {
                args.attackSpeedMultAdd += 3f;
            }
            if (sender.HasBuff(Buffs.mercPeaceBuff))
            {
                args.moveSpeedReductionMultAdd += 0.9f;
                args.jumpPowerMultAdd = 0;
                sender.isSprinting = false;
            }
        }

        public static void RollForAdrenaline(CharacterBody characterBody)
        {
            //
            if (!characterBody.HasBuff(adrenalineBuff))
            {
                if (Util.CheckRoll(10))
                    characterBody.AddTimedBuff(adrenalineBuff, adrenalineDuration);
            } else {
                if (Util.CheckRoll(1))
                    characterBody.AddTimedBuff(adrenalineBuff, adrenalineDuration);
            }
        }

        public override void SetupPrimary()
        {
            return;
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESAuralAttack));
            mySkillDef.activationStateMachineName = "Weapon";
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
            mySkillDef.skillName = TokenPrefix+"_PRIMARY";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            primarySkillDefs.Add(mySkillDef);
            base.SetupPrimary();
        }
        
        public override void SetupSecondary()
        {
            var mySkillDef = ScriptableObject.CreateInstance<DCMercSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESActiveEcho));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 5;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.secondary.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_SECONDARY";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            secondarySkillDefs.Add(mySkillDef);
            base.SetupSecondary();
        }
        
        public override void SetupUtility()
        {
            return;
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESWavedash));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 6;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
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
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESChargeSlash));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 6;
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
            specialSkillDefs.Add(mySkillDef);
            base.SetupSpecial();
        }

        public class DCPeace_Module : MonoBehaviour, IOnTakeDamageServerReceiver
        {
            public CharacterBody owner;

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.attackerBody)
                {
                    TeleportHelper.TeleportBody(owner, damageReport.attackerBody.footPosition);
                    owner.skillLocator.primary.ExecuteIfReady();
                    owner.RemoveBuff(Buffs.mercPeaceBuff);
                    Destroy(this);
                }
            }
        }
        public class DCMercEchoComponent : MonoBehaviour
        {
            public CharacterBody owner;
            private Vector3 positionOfLastSlash;
            private string slashSound;
            public bool canSlash = false;
            public float damage = 0;
            public bool wasCrit = false;

            public void ConsumeSlash()
            {
                if (!canSlash) return;
                BlastAttack fakeAttack = new BlastAttack()
                {
                    attacker = owner.gameObject,
                    inflictor = owner.gameObject,
                    procCoefficient = 0f,
                    procChainMask = default,
                    //impactEffect = null,
                    losType = BlastAttack.LoSType.None,
                    damageType = DamageType.ApplyMercExpose,
                    baseForce = 0,
                    baseDamage = damage * 0.5f,
                    falloffModel = BlastAttack.FalloffModel.None,
                    radius = 5f,
                    position = positionOfLastSlash,
                    attackerFiltering = AttackerFiltering.Default,
                    teamIndex = owner.teamComponent.teamIndex,
                    crit = wasCrit
                };
                var result = fakeAttack.Fire();
			    Util.PlaySound(slashSound, owner.gameObject);
                canSlash = false;
            }

            public void Track(Vector3 position, string slashSound, float damage, bool crit)
            {
                positionOfLastSlash = position;
                this.slashSound = slashSound;
                canSlash = true;
                this.damage = damage;
                wasCrit = crit;
            }
        }

        //based of huntress's
        public class DCMercSkillDef : SkillDef
        {
            public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
            {
                return new DCMercSkillDef.InstanceData
                {
                    echoComponent = skillSlot.GetComponent<DCMercEchoComponent>()
                };
            }

            private static bool CanSlash([NotNull] GenericSkill skillSlot)
            {
                DCMercEchoComponent echoComponent = ((DCMercSkillDef.InstanceData)skillSlot.skillInstanceData).echoComponent;
                return (echoComponent != null) ? echoComponent.canSlash : false;
            }

            public override bool CanExecute([NotNull] GenericSkill skillSlot)
            {
			    return DCMercSkillDef.CanSlash(skillSlot) && base.CanExecute(skillSlot);
            }
            
            protected class InstanceData : SkillDef.BaseSkillInstanceData
            {
                public DCMercEchoComponent echoComponent;
            }
        }
    }
}
