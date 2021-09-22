using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates.Croco;

namespace ROR1AltSkills.Acrid
{
	public class ModifiedBaseLeap : GenericCharacterMain
	{
		// no fist effects
		public static float minimumDuration = BaseLeap.minimumDuration;
		public static float blastRadius = BaseLeap.blastRadius;
		public static float blastProcCoefficient = BaseLeap.blastProcCoefficient;
		[SerializeField]
		public float blastDamageCoefficient = AcridMain.CausticSludgeLeapLandDamageCoefficient;
		[SerializeField]
		public float blastForce = 0;
		public static string leapSoundString = BaseLeap.leapSoundString;
		public static GameObject projectilePrefab;
		[SerializeField]
		public Vector3 blastBonusForce = Vector3.zero;
		[SerializeField]
		public GameObject blastImpactEffectPrefab = null;
		[SerializeField]
		public GameObject blastEffectPrefab = null;
		public static float airControl = BaseLeap.airControl;
		public static float aimVelocity = BaseLeap.aimVelocity;
		public static float upwardVelocity = BaseLeap.upwardVelocity;
		public static float forwardVelocity = BaseLeap.forwardVelocity;
		public static float minimumY = BaseLeap.minimumY;
		public static float minYVelocityForAnim = BaseLeap.minYVelocityForAnim;
		public static float maxYVelocityForAnim = BaseLeap.maxYVelocityForAnim;
		public static float knockbackForce = BaseLeap.knockbackForce;
		public static string soundLoopStartEvent = BaseLeap.soundLoopStartEvent;
		public static string soundLoopStopEvent = BaseLeap.soundLoopStopEvent;
		public static NetworkSoundEventDef landingSound = BaseLeap.landingSound;
		private float previousAirControl = 0;
		protected bool isCritAuthority;
		protected CrocoDamageTypeController crocoDamageTypeController;
		public bool detonateNextFrame;

		public bool SetToMainOnImpact = true;
		public bool hasLanded = false;

		public bool hasPlayedLandingAnimation = false;

		protected virtual DamageType GetBlastDamageType()
		{
			return DamageType.Generic;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			this.crocoDamageTypeController = base.GetComponent<CrocoDamageTypeController>();
			this.previousAirControl = base.characterMotor.airControl;
			base.characterMotor.airControl = BaseLeap.airControl;
			Vector3 direction = base.GetAimRay().direction;
			if (base.isAuthority)
			{
				base.characterBody.isSprinting = true;
				direction.y = Mathf.Max(direction.y, BaseLeap.minimumY);
				Vector3 a = direction.normalized * BaseLeap.aimVelocity * this.moveSpeedStat;
				Vector3 b = Vector3.up * BaseLeap.upwardVelocity;
				Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * BaseLeap.forwardVelocity;
				base.characterMotor.Motor.ForceUnground();
				base.characterMotor.velocity = a + b + b2;
				this.isCritAuthority = base.RollCrit();
			}
			base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
			base.GetModelTransform().GetComponent<AimAnimator>().enabled = true;
			base.PlayCrossfade("Gesture, Override", "Leap", 0.1f);
			base.PlayCrossfade("Gesture, AdditiveHigh", "Leap", 0.1f);
			base.PlayCrossfade("Gesture, Override", "Leap", 0.1f);
			Util.PlaySound(BaseLeap.leapSoundString, base.gameObject);
			base.characterDirection.moveVector = direction;
			if (base.isAuthority)
			{
				base.characterMotor.onMovementHit += this.OnMovementHit;
			}
			Util.PlaySound(BaseLeap.soundLoopStartEvent, base.gameObject);
		}

		private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
		{
			this.detonateNextFrame = true;
		}

		public override void UpdateAnimationParameters()
		{
			base.UpdateAnimationParameters();
			float value = Mathf.Clamp01(Util.Remap(base.estimatedVelocity.y, BaseLeap.minYVelocityForAnim, BaseLeap.maxYVelocityForAnim, 0f, 1f)) * 0.97f;
			base.modelAnimator.SetFloat("LeapCycle", value, 0.1f, Time.deltaTime);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.characterMotor)
			{
				if (!hasLanded)
				{
					base.characterMotor.moveDirection = base.inputBank.moveVector;
					if (base.fixedAge >= BaseLeap.minimumDuration && (this.detonateNextFrame || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
					{
						hasLanded = true;
						this.DoImpactAuthority();
						if (SetToMainOnImpact)
							this.outer.SetNextStateToMain();
					}
				}
			}
		}

		public virtual void DoImpactAuthority()
		{
			if (BaseLeap.landingSound)
			{
				EffectManager.SimpleSoundEffect(BaseLeap.landingSound.index, base.characterBody.footPosition, true);
			}
		}

		protected BlastAttack.Result DetonateAuthority()
		{
			Vector3 footPosition = base.characterBody.footPosition;
			EffectManager.SpawnEffect(this.blastEffectPrefab, new EffectData
			{
				origin = footPosition,
				scale = BaseLeap.blastRadius
			}, true);
			return new BlastAttack
			{
				attacker = base.gameObject,
				baseDamage = this.damageStat * this.blastDamageCoefficient,
				baseForce = this.blastForce,
				bonusForce = this.blastBonusForce,
				crit = this.isCritAuthority,
				damageType = this.GetBlastDamageType(),
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = BaseLeap.blastProcCoefficient,
				radius = BaseLeap.blastRadius,
				position = footPosition,
				attackerFiltering = AttackerFiltering.NeverHit,
				impactEffect = EffectCatalog.FindEffectIndexFromPrefab(this.blastImpactEffectPrefab),
				teamIndex = base.teamComponent.teamIndex
			}.Fire();
		}

		protected void DropAcidPoolAuthority()
		{
			Vector3 footPosition = base.characterBody.footPosition;
			FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
			{
				projectilePrefab = projectilePrefab,
				crit = this.isCritAuthority,
				force = 0f,
				damage = this.damageStat,
				owner = base.gameObject,
				rotation = Quaternion.identity,
				position = footPosition
			};
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		}

		public override void OnExit()
		{
			Util.PlaySound(BaseLeap.soundLoopStopEvent, base.gameObject);
			if (base.isAuthority)
			{
				base.characterMotor.onMovementHit -= this.OnMovementHit;
			}
			base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
			base.characterMotor.airControl = this.previousAirControl;
			base.characterBody.isSprinting = false;
			PlayLandingAnimation();
			base.OnExit();
		}

		public virtual void PlayLandingAnimation()
		{
			if (!hasPlayedLandingAnimation)
			{
				int layerIndex = base.modelAnimator.GetLayerIndex("Impact");
				if (layerIndex >= 0)
				{
					base.modelAnimator.SetLayerWeight(layerIndex, 2f);
					base.PlayAnimation("Impact", "LightImpact");
				}
				base.PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
				base.PlayCrossfade("Gesture, AdditiveHigh", "BufferEmpty", 0.1f);
				hasPlayedLandingAnimation = true;
			}
        }

		// Token: 0x06004472 RID: 17522 RVA: 0x0006E4AF File Offset: 0x0006C6AF
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

	}
}
