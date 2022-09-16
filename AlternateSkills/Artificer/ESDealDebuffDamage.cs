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

namespace AlternateSkills
{
    public class ESDealDebuffDamage : BaseSkillState
    {

		[SerializeField]
		public float baseDuration;
		private float duration;

		private HurtBox initialOrbTarget;

		private HuntressTracker huntressTracker;

        float damageCoefficient = 0.75f;

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
            ShatterEnemy();
        }

        public void ShatterEnemy()
        {
            if (!NetworkServer.active) return;
            var targetBody = initialOrbTarget.healthComponent.body;
            var enemyDebuffs = MainPlugin.ReturnBuffs(targetBody, true, false);
            var damageInfo = new DamageInfo()
            {
                damage = damageCoefficient * enemyDebuffs.Length * damageStat,
                crit = false,
                attacker = characterBody.gameObject,
                inflictor = characterBody.gameObject,
                position = targetBody.corePosition,
                force = Vector3.zero,
                procChainMask = default,
                damageType = DamageType.Generic,
                damageColorIndex = DamageColorIndex.Default,
            };
            targetBody.healthComponent.TakeDamage(damageInfo);

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