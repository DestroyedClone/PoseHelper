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

namespace AlternateSkills.Commando
{
	public class ESReinforcement : BaseSkillState
    {
        // 1. Target an ally
        // 1b. Just do proximity for now
        // 2. Add a component to them
        // 3. Exit.
        [SerializeField]
		public float baseDuration = 1f;
		private float duration;

		private HurtBox initialOrbTarget;

		private AllyTracker allyTracker;

        private CommandoReinforcementComponent commandoReinforcementComponent;

        public override void OnEnter()
        {
            base.OnEnter();
			this.allyTracker = base.GetComponent<AllyTracker>();
            if (this.allyTracker && base.isAuthority)
			{
				this.initialOrbTarget = this.allyTracker.GetTrackingTarget();
			}
            commandoReinforcementComponent = base.GetComponent<CommandoReinforcementComponent>();
			this.duration = this.baseDuration / this.attackSpeedStat;
			base.characterBody.SetAimTimer(this.duration + 2f);
			base.PlayAnimation("Gesture, Additive", "PrepWall", "PrepWall.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            ReinforceAlly();
        }

        public void ReinforceAlly()
        {
            if (!NetworkServer.active) return;
            var targetBody = initialOrbTarget.healthComponent.body;
            if (targetBody)
            {
                if (commandoReinforcementComponent.AddTarget(targetBody))
                {

                } else {
                    skillLocator.utility.rechargeStopwatch = 0;
                }
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