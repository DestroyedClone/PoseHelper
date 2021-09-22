using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using R2API;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Audio;
using EntityStates.Croco;

namespace ROR1AltSkills.Acrid
{
    public class PoisonBite : ModifiedBasicMeleeAttack
	{
		public PoisonBite()
        {
			hitBoxGroupName = "Slash";
			hitEffectPrefab = Resources.Load<GameObject>("OmniImpactVFXSlashSyringe");
			procCoefficient = 1f;
			pushAwayForce = 0;
			forceVector = Vector3.zero;
			hitPauseDuration = 0.1f;
			swingEffectPrefab = Resources.Load<GameObject>("CrocoBiteEffect");
			swingEffectMuzzleString = "MouthMuzzle";
			mecanimHitboxActiveParameter = "Bite.hitBoxActive";
			shorthopVelocityFromHit = 6;
			beginStateSoundString = "Play_acrid_m2_bite_shoot";
			beginSwingSoundString = "";
			forceForwardVelocity = false;
			//forwardVelocityCurve;
			scaleHitPauseDurationAndVelocityWithAttackSpeed = false;
			ignoreAttackSpeed = false;
        }
		// Token: 0x04003DAF RID: 15791
		public static float recoilAmplitude = Bite.recoilAmplitude;

		// Token: 0x04003DB0 RID: 15792
		public static float baseDurationBeforeInterruptable = Bite.baseDurationBeforeInterruptable;

		// Token: 0x04003DB1 RID: 15793
		[SerializeField]
		public float bloom = 0;

		// Token: 0x04003DB2 RID: 15794
		public static string biteSound = Bite.biteSound;

		// Token: 0x04003DB3 RID: 15795
		//private string animationStateName;

		// Token: 0x04003DB4 RID: 15796
		private float durationBeforeInterruptable;

		// Token: 0x04003DB5 RID: 15797
		private CrocoDamageTypeController crocoDamageTypeController;

		// Token: 0x04003DB6 RID: 15798
		private bool hasGrantedBuff;
		protected override bool allowExitFire
		{
			get
			{
				return base.characterBody && !base.characterBody.isSprinting;
			}
		}

		// Token: 0x06004458 RID: 17496 RVA: 0x001138FC File Offset: 0x00111AFC
		public override void OnEnter()
		{
			base.OnEnter();
			base.characterDirection.forward = base.GetAimRay().direction;
			this.durationBeforeInterruptable = Bite.baseDurationBeforeInterruptable / this.attackSpeedStat;
			this.crocoDamageTypeController = base.GetComponent<CrocoDamageTypeController>();
		}

		// Token: 0x06004459 RID: 17497 RVA: 0x000F87FA File Offset: 0x000F69FA
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x0600445A RID: 17498 RVA: 0x00113948 File Offset: 0x00111B48
		protected override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
		{
			base.AuthorityModifyOverlapAttack(overlapAttack);
			overlapAttack.damageType = (DamageType.BonusToLowHealth);
			overlapAttack.AddModdedDamageType(AcridMain.OriginalPoisonOnHit);
		}

		// Token: 0x0600445B RID: 17499 RVA: 0x00113988 File Offset: 0x00111B88
		protected override void PlayAnimation()
		{
			float duration = Mathf.Max(this.duration, 0.2f);
			base.PlayCrossfade("Gesture, Additive", "Bite", "Bite.playbackRate", duration, 0.05f);
			base.PlayCrossfade("Gesture, Override", "Bite", "Bite.playbackRate", duration, 0.05f);
			Util.PlaySound(Bite.biteSound, base.gameObject);
		}

		// Token: 0x0600445C RID: 17500 RVA: 0x001139F0 File Offset: 0x00111BF0
		protected override void OnMeleeHitAuthority()
		{
			base.OnMeleeHitAuthority();
			base.characterBody.AddSpreadBloom(this.bloom);
			if (!this.hasGrantedBuff)
			{
				this.hasGrantedBuff = true;
				base.characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.CrocoRegen.buffIndex, 0.5f);
			}
		}

		// Token: 0x0600445D RID: 17501 RVA: 0x00113A3D File Offset: 0x00111C3D
		public override void BeginMeleeAttackEffect()
		{
			base.AddRecoil(0.9f * Bite.recoilAmplitude, 1.1f * Bite.recoilAmplitude, -0.1f * Bite.recoilAmplitude, 0.1f * Bite.recoilAmplitude);
			base.BeginMeleeAttackEffect();
		}

		// Token: 0x0600445E RID: 17502 RVA: 0x00113A77 File Offset: 0x00111C77
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			if (base.fixedAge >= this.durationBeforeInterruptable)
			{
				return InterruptPriority.Skill;
			}
			return InterruptPriority.Pain;
		}
	}
}
