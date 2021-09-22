using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using R2API;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Audio;


namespace ROR1AltSkills
{
    public class ModifiedBasicMeleeAttack : BaseState
	{
		[SerializeField]
		public float baseDuration = 1f;
		[SerializeField]
		public float damageCoefficient = 1f;
		[SerializeField]
		public string hitBoxGroupName = "";
		[SerializeField]
		public GameObject hitEffectPrefab;
		[SerializeField]
		public float procCoefficient = 1f;
		[SerializeField]
		public float pushAwayForce = 0f;
		[SerializeField]
		public Vector3 forceVector = Vector3.zero;
		[SerializeField]
		public float hitPauseDuration = 0.5f;
		[SerializeField]
		public GameObject swingEffectPrefab = null;
		[SerializeField]
		public string swingEffectMuzzleString = "";
		[SerializeField]
		public string mecanimHitboxActiveParameter = "";
		[SerializeField]
		public float shorthopVelocityFromHit = 0f;
		[SerializeField]
		public string beginStateSoundString = "";
		[SerializeField]
		public string beginSwingSoundString = "";
		[SerializeField]
		public NetworkSoundEventDef impactSound;
		[SerializeField]
		public bool forceForwardVelocity = false;
		[SerializeField]
		public AnimationCurve forwardVelocityCurve;
		[SerializeField]
		public bool scaleHitPauseDurationAndVelocityWithAttackSpeed = true;
		[SerializeField]
		public bool ignoreAttackSpeed = false;

		protected float duration = 2f;
		protected HitBoxGroup hitBoxGroup;
		protected Animator animator;
		public OverlapAttack overlapAttack;
		protected bool authorityHitThisFixedUpdate;
		protected float hitPauseTimer;
		protected Vector3 storedHitPauseVelocity;
		private Run.FixedTimeStamp meleeAttackStartTime = Run.FixedTimeStamp.positiveInfinity;
		private GameObject swingEffectInstance;
		private int meleeAttackTicks;
		protected List<HurtBox> hitResults = new List<HurtBox>();


		private bool forceFire;

		protected bool authorityInHitPause
		{
			get
			{
				return this.hitPauseTimer > 0f;
			}
		}

		private bool meleeAttackHasBegun
		{
			get
			{
				return this.meleeAttackStartTime.hasPassed;
			}
		}

		protected bool authorityHasFiredAtAll
		{
			get
			{
				return this.meleeAttackTicks > 0;
			}
		}

		private protected bool isCritAuthority { private get; set; }


		protected virtual bool allowExitFire
		{
			get
			{
				return true;
			}
		}


		public virtual string GetHitBoxGroupName()
		{
			return this.hitBoxGroupName;
		}


		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = this.CalcDuration();
			if (this.duration <= Time.fixedDeltaTime * 2f)
			{
				this.forceFire = true;
			}
			base.StartAimMode(2f, false);
			Util.PlaySound(this.beginStateSoundString, base.gameObject);
			this.animator = base.GetModelAnimator();
			if (base.isAuthority)
			{
				this.isCritAuthority = base.RollCrit();
				this.hitBoxGroup = base.FindHitBoxGroup(this.GetHitBoxGroupName());
				if (this.hitBoxGroup)
				{
					OverlapAttack overlapAttack = new OverlapAttack();
					overlapAttack.attacker = base.gameObject;
					overlapAttack.damage = this.damageCoefficient * this.damageStat;
					overlapAttack.damageColorIndex = DamageColorIndex.Default;
					overlapAttack.damageType = DamageType.Generic;
					overlapAttack.forceVector = this.forceVector;
					overlapAttack.hitBoxGroup = this.hitBoxGroup;
					overlapAttack.hitEffectPrefab = this.hitEffectPrefab;
					NetworkSoundEventDef networkSoundEventDef = this.impactSound;
					overlapAttack.impactSound = ((networkSoundEventDef != null) ? networkSoundEventDef.index : NetworkSoundEventIndex.Invalid);
					overlapAttack.inflictor = base.gameObject;
					overlapAttack.isCrit = this.isCritAuthority;
					overlapAttack.procChainMask = default;
					overlapAttack.pushAwayForce = this.pushAwayForce;
					overlapAttack.procCoefficient = this.procCoefficient;
					overlapAttack.teamIndex = base.GetTeam();
					this.overlapAttack = overlapAttack;
				}
			}
			this.PlayAnimation();
		}


		protected virtual float CalcDuration()
		{
			if (this.ignoreAttackSpeed)
			{
				return this.baseDuration;
			}
			return this.baseDuration / this.attackSpeedStat;
		}

		// Token: 0x060039AB RID: 14763 RVA: 0x00004381 File Offset: 0x00002581
		protected virtual void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
		{
		}

		// Token: 0x060039AC RID: 14764 RVA: 0x000E2878 File Offset: 0x000E0A78
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (string.IsNullOrEmpty(this.mecanimHitboxActiveParameter))
			{
				this.BeginMeleeAttackEffect();
			}
			else if (this.animator.GetFloat(this.mecanimHitboxActiveParameter) > 0.5f)
			{
				this.BeginMeleeAttackEffect();
			}
			if (base.isAuthority)
			{
				this.AuthorityFixedUpdate();
			}
		}

		// Token: 0x060039AD RID: 14765 RVA: 0x000E28CC File Offset: 0x000E0ACC
		protected void AuthorityTriggerHitPause()
		{
			if (base.characterMotor)
			{
				this.storedHitPauseVelocity += base.characterMotor.velocity;
				base.characterMotor.velocity = Vector3.zero;
			}
			if (this.animator)
			{
				this.animator.speed = 0f;
			}
			if (this.swingEffectInstance)
			{
				ScaleParticleSystemDuration component = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
				if (component)
				{
					component.newDuration = 20f;
				}
			}
			this.hitPauseTimer = (this.scaleHitPauseDurationAndVelocityWithAttackSpeed ? (this.hitPauseDuration / this.attackSpeedStat) : this.hitPauseDuration);
		}

		// Token: 0x060039AE RID: 14766 RVA: 0x000E2980 File Offset: 0x000E0B80
		public virtual void BeginMeleeAttackEffect()
		{
			if (this.meleeAttackStartTime != Run.FixedTimeStamp.positiveInfinity)
			{
				return;
			}
			this.meleeAttackStartTime = Run.FixedTimeStamp.now;
			Util.PlaySound(this.beginSwingSoundString, base.gameObject);
			if (this.swingEffectPrefab)
			{
				Transform transform = base.FindModelChild(this.swingEffectMuzzleString);
				if (transform)
				{
					this.swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(this.swingEffectPrefab, transform);
					ScaleParticleSystemDuration component = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
					if (component)
					{
						component.newDuration = component.initialDuration;
					}
				}
			}
		}

		// Token: 0x060039AF RID: 14767 RVA: 0x000E2A14 File Offset: 0x000E0C14
		protected virtual void AuthorityExitHitPause()
		{
			this.hitPauseTimer = 0f;
			this.storedHitPauseVelocity.y = Mathf.Max(this.storedHitPauseVelocity.y, this.scaleHitPauseDurationAndVelocityWithAttackSpeed ? (this.shorthopVelocityFromHit / Mathf.Sqrt(this.attackSpeedStat)) : this.shorthopVelocityFromHit);
			if (base.characterMotor)
			{
				base.characterMotor.velocity = this.storedHitPauseVelocity;
			}
			this.storedHitPauseVelocity = Vector3.zero;
			if (this.animator)
			{
				this.animator.speed = 1f;
			}
			if (this.swingEffectInstance)
			{
				ScaleParticleSystemDuration component = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
				if (component)
				{
					component.newDuration = component.initialDuration;
				}
			}
		}

		// Token: 0x060039B0 RID: 14768 RVA: 0x00004381 File Offset: 0x00002581
		protected virtual void PlayAnimation()
		{
		}

		// Token: 0x060039B1 RID: 14769 RVA: 0x00004381 File Offset: 0x00002581
		protected virtual void OnMeleeHitAuthority()
		{
		}

		// Token: 0x060039B2 RID: 14770 RVA: 0x000E2ADC File Offset: 0x000E0CDC
		private void AuthorityFireAttack()
		{
			if (this.overlapAttack != null)
            {
				this.AuthorityModifyOverlapAttack(this.overlapAttack);
				this.hitResults.Clear();
				this.authorityHitThisFixedUpdate = this.overlapAttack.Fire(this.hitResults);
				this.meleeAttackTicks++;
				if (this.authorityHitThisFixedUpdate)
				{
					this.AuthorityTriggerHitPause();
					this.OnMeleeHitAuthority();
				}
			} else
            {
				Chat.AddMessage("Why");
            }
		}

		// Token: 0x060039B3 RID: 14771 RVA: 0x000E2B3C File Offset: 0x000E0D3C
		protected virtual void AuthorityFixedUpdate()
		{
			if (this.authorityInHitPause)
			{
				this.hitPauseTimer -= Time.fixedDeltaTime;
				if (base.characterMotor)
				{
					base.characterMotor.velocity = Vector3.zero;
				}
				base.fixedAge -= Time.fixedDeltaTime;
				if (!this.authorityInHitPause)
				{
					this.AuthorityExitHitPause();
				}
			}
			else if (this.forceForwardVelocity && base.characterMotor && base.characterDirection)
			{
				Vector3 vector = base.characterDirection.forward * this.forwardVelocityCurve.Evaluate(base.fixedAge / this.duration);
				Vector3 velocity = base.characterMotor.velocity;
				base.characterMotor.AddDisplacement(new Vector3(vector.x, 0f, vector.z));
			}
			this.authorityHitThisFixedUpdate = false;
			if (this.overlapAttack != null && (string.IsNullOrEmpty(this.mecanimHitboxActiveParameter) || this.animator.GetFloat(this.mecanimHitboxActiveParameter) > 0.5f || this.forceFire))
			{
				this.AuthorityFireAttack();
			}
			if (this.duration <= base.fixedAge)
			{
				this.AuthorityOnFinish();
			}
		}

		// Token: 0x060039B4 RID: 14772 RVA: 0x000E2C78 File Offset: 0x000E0E78
		public override void OnExit()
		{
			if (base.isAuthority)
			{
				if (!this.outer.destroying && !this.authorityHasFiredAtAll && this.allowExitFire)
				{
					this.BeginMeleeAttackEffect();
					this.AuthorityFireAttack();
				}
				if (this.authorityInHitPause)
				{
					this.AuthorityExitHitPause();
				}
			}
			if (this.swingEffectInstance)
			{
				EntityState.Destroy(this.swingEffectInstance);
			}
			if (this.animator)
			{
				this.animator.speed = 1f;
			}
			base.OnExit();
		}

		// Token: 0x060039B5 RID: 14773 RVA: 0x000E2CFF File Offset: 0x000E0EFF
		protected virtual void AuthorityOnFinish()
		{
			this.outer.SetNextStateToMain();
		}
	}
}
