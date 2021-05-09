using System;
using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.Merc;

namespace AlternateSkills.Mercenary
{
    public class FallingLight : BaseSkillState
    {
		protected Animator animator;
		protected float duration;
		protected bool hasSwung;
		protected float hitPauseTimer;
		protected bool isInHitPause;
		protected OverlapAttack overlapAttack;
		protected BaseState.HitStopCachedState hitStopCachedState;

		public override void OnEnter()
		{
			base.OnEnter();
			this.animator = base.GetModelAnimator();
			this.duration = Uppercut.baseDuration / this.attackSpeedStat;
			this.overlapAttack = base.InitMeleeOverlap(Uppercut.baseDamageCoefficient, Uppercut.hitEffectPrefab, base.GetModelTransform(), Uppercut.hitboxString);
			this.overlapAttack.forceVector = Vector3.down * Uppercut.upwardForceStrength;
			if (base.characterDirection && base.inputBank)
			{
				base.characterDirection.forward = base.inputBank.aimDirection;
			}
			Util.PlaySound(Uppercut.enterSoundString, base.gameObject);
			this.PlayAnim();
		}

		// Token: 0x06003ECC RID: 16076 RVA: 0x000F8093 File Offset: 0x000F6293
		protected virtual void PlayAnim()
		{
			base.PlayCrossfade("FullBody, Override", "Uppercut", "Uppercut.playbackRate", this.duration, 0.1f);
		}

		// Token: 0x06003ECD RID: 16077 RVA: 0x000F80B5 File Offset: 0x000F62B5
		public override void OnExit()
		{
			base.OnExit();
			base.PlayAnimation("FullBody, Override", "UppercutExit");
		}

		// Token: 0x06003ECE RID: 16078 RVA: 0x000F80D0 File Offset: 0x000F62D0
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.hitPauseTimer -= Time.fixedDeltaTime;
			if (base.isAuthority)
			{
				if (this.animator.GetFloat("Sword.active") > 0.2f && !this.hasSwung)
				{
					this.hasSwung = true;
					base.characterMotor.Motor.ForceUnground();
					Util.PlayAttackSpeedSound(Uppercut.attackSoundString, base.gameObject, Uppercut.slashPitch);
					EffectManager.SimpleMuzzleFlash(Uppercut.swingEffectPrefab, base.gameObject, Uppercut.slashChildName, true);
				}
				if (base.FireMeleeOverlap(this.overlapAttack, this.animator, "Sword.active", 0f, false))
				{
					Util.PlaySound(Uppercut.hitSoundString, base.gameObject);
					if (!this.isInHitPause)
					{
						this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Uppercut.playbackRate");
						this.hitPauseTimer = Uppercut.hitPauseDuration / this.attackSpeedStat;
						this.isInHitPause = true;
					}
				}
				if (this.hitPauseTimer <= 0f && this.isInHitPause)
				{
					base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
					base.characterMotor.Motor.ForceUnground();
					this.isInHitPause = false;
				}
				if (!this.isInHitPause)
				{
					if (base.characterMotor && base.characterDirection)
					{
						Vector3 velocity = base.characterDirection.forward * this.moveSpeedStat * Mathf.Lerp(Uppercut.moveSpeedBonusCoefficient, 0f, base.age / this.duration);
						velocity.y = -Uppercut.yVelocityCurve.Evaluate(base.fixedAge / this.duration) *2f ;
						base.characterMotor.velocity = velocity;
					}
				}
				else
				{
					base.fixedAge -= Time.fixedDeltaTime;
					base.characterMotor.velocity = Vector3.zero;
					this.hitPauseTimer -= Time.fixedDeltaTime;
					this.animator.SetFloat("Uppercut.playbackRate", 0f);
				}
				if (base.fixedAge >= this.duration)
				{
					if (this.hasSwung)
					{
						this.hasSwung = true;
						this.overlapAttack.Fire(null);
					}
					this.outer.SetNextStateToMain();
				}
			}
		}
	}
}
