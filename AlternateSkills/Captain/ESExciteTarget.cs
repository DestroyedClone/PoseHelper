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
using UnityEngine.Networking;
using EntityStates.Captain;

namespace AlternateSkills.Captain
{
    public class ESExciteTarget : BaseSkillState
    {

		[SerializeField]
		public float baseDuration;
		private float duration;

		private HurtBox initialOrbTarget;

		private HuntressTracker huntressTracker;

        public override void OnEnter()
        {
            base.OnEnter();
			this.huntressTracker = base.GetComponent<HuntressTracker>();
            if (this.huntressTracker && base.isAuthority)
			{
				this.initialOrbTarget = this.huntressTracker.GetTrackingTarget();
			}
			this.duration = this.baseDuration / this.attackSpeedStat;
			base.characterBody.SetAimTimer(this.duration + 2f);
			base.PlayAnimation("Gesture, Additive", "PrepWall", "PrepWall.playbackRate", this.duration);
            if (!huntressTracker)
            {
                MainPlugin._logger.LogWarning("No tracker found for body.");
                OnExit();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            ChooseAttackType();
        }

        public void ChooseAttackType()
        {
            if (!NetworkServer.active) return;
            var targetBody = initialOrbTarget.healthComponent.body;
            if (targetBody.isPlayerControlled || targetBody.isBoss)
                return;
            var targetMaster = targetBody.master;
            if (!targetMaster || targetMaster.aiComponents.Length <= 0)
            {
                return;
            }
            var bot = targetMaster.aiComponents[0];
            if (bot.currentEnemy.characterBody != characterBody)
            {
                bot.currentEnemy.characterBody = characterBody;
                activatorSkillSlot.rechargeStopwatch = 15;
            } else {
                targetBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
                targetBody.AddBuff(RoR2Content.Buffs.PowerBuff);
                targetBody.AddBuff(RoR2Content.Buffs.AttackSpeedOnCrit);
                targetBody.AddBuff(RoR2Content.Buffs.AttackSpeedOnCrit);
                targetBody.AddBuff(RoR2Content.Buffs.AttackSpeedOnCrit);
                targetBody.inventory?.GiveItem(RoR2Content.Items.HealthDecay, 30);
            }
            
        }

        public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge > this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}
    }

}