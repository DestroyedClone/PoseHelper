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
using EntityStates.Mage;

namespace AlternateSkills.Mage
{
    public class ESShareBuffs : BaseSkillState
    {

		[SerializeField]
		public float baseDuration = 1;
		private float duration;

		private HurtBox initialOrbTarget;

		private HuntressTracker huntressTracker;

        float sharedBuffDuration = 15;
        float sharedDebuffDuration = 15;

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
        }

        public override void OnExit()
        {
            base.OnExit();
            ExchangeBuffs();
        }

        public void ExchangeBuffs()
        {
            if (!NetworkServer.active) return;
            var targetBody = initialOrbTarget.healthComponent.body;
            var ownerDebuffs = MainPlugin.ReturnBuffs(characterBody, true, false);
            var enemyBuffs = MainPlugin.ReturnBuffs(targetBody, false, true);

            foreach (var sin in ownerDebuffs)
            {
                if (!sin.isCooldown)
                    targetBody.AddTimedBuff(sin, sharedDebuffDuration);
            }
            foreach (var bless in enemyBuffs)
            {
                if (!bless.isCooldown)
                    characterBody.AddTimedBuff(bless, sharedBuffDuration);
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