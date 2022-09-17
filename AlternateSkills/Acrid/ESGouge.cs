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
using EntityStates.Croco;
using AlternateSkills.Modules;

namespace AlternateSkills.Acrid
{
    public class ESGouge : BasicMeleeAttack
    {
        
		public int step;

		[SerializeField]
		public float bloom;

		private string animationStateName;

		private float durationBeforeInterruptable;

		private bool hasGrantedBuff;
		public override void OnEnter()
		{
			base.OnEnter();
				this.damageCoefficient = Slash.comboFinisherDamageCoefficient;
				this.swingEffectPrefab = Slash.comboFinisherSwingEffectPrefab;
				this.hitPauseDuration = Slash.comboFinisherhitPauseDuration;
				this.bloom = Slash.comboFinisherBloom;
			base.characterDirection.forward = base.GetAimRay().direction;
			this.durationBeforeInterruptable = Slash.baseDurationBeforeInterruptable / this.attackSpeedStat;
		}

        public override void PlayAnimation()
		{
            this.animationStateName = "Slash1";
            string soundString = Slash.slash1Sound;
			float duration = Mathf.Max(this.duration, 0.2f);
			base.PlayCrossfade("Gesture, Additive", this.animationStateName, "Slash.playbackRate", duration, 0.05f);
			base.PlayCrossfade("Gesture, Override", this.animationStateName, "Slash.playbackRate", duration, 0.05f);
			Util.PlaySound(soundString, base.gameObject);
		}

		public override void OnMeleeHitAuthority()
		{
			base.OnMeleeHitAuthority();
			base.characterBody.AddSpreadBloom(this.bloom);
			if (!this.hasGrantedBuff)
			{
				this.hasGrantedBuff = true;
				base.characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.CrocoRegen.buffIndex, 0.5f);
			}
		}
        
        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
		{
			base.AuthorityModifyOverlapAttack(overlapAttack);
			overlapAttack.damageType = DamageType.Generic;
			overlapAttack.AddModdedDamageType(DamageTypes.DTCrocoPoisonCountdown);
		}
        
		public override void BeginMeleeAttackEffect()
		{
			this.swingEffectMuzzleString = this.animationStateName;
			base.AddRecoil(-0.1f * Slash.recoilAmplitude, 0.1f * Slash.recoilAmplitude, -1f * Slash.recoilAmplitude, 1f * Slash.recoilAmplitude);
			base.BeginMeleeAttackEffect();
		}
        
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			if (base.fixedAge >= this.durationBeforeInterruptable)
			{
				return InterruptPriority.Skill;
			}
			return InterruptPriority.PrioritySkill;
		}

    }

}