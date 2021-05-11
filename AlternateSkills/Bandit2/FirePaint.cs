using System;
using System.Collections.Generic;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using EntityStates;
using RoR2.Projectile;

namespace AlternateSkills.Bandit2
{
    public class FirePaint : BaseSkillState
    {
		public static float baseDurationPerMissile;
		[SerializeField]
		public static float damageCoefficient;
		public static GameObject projectilePrefab;
		public static GameObject muzzleflashEffectPrefab;
		public List<HurtBox> targetsList;
		private int fireIndex;
		private float durationPerMissile;
		private float stopwatch;
		//2
		[SerializeField]
		public GameObject effectPrefab;
		[SerializeField]
		public GameObject hitEffectPrefab;
		[SerializeField]
		public GameObject tracerEffectPrefab;
		[SerializeField]
		public float force;
		[SerializeField]
		public float minSpread;
		[SerializeField]
		public float maxSpread;
		[SerializeField]
		public string attackSoundString;
		[SerializeField]
		public float recoilAmplitude;
		[SerializeField]
		public float bulletRadius;
		//3

		[SerializeField]
		public float baseDuration;
		[SerializeField]
		public GameObject crosshairOverridePrefab;
		protected float duration;
		private Animator animator;
		private int bodySideWeaponLayerIndex;
		private GameObject originalCrosshairPrefab;

		public virtual string exitAnimationStateName
		{
			get
			{
				return "BufferEmpty";
			}
		}


		public override void OnEnter()
		{
			base.OnEnter();
			this.animator = base.GetModelAnimator();
			this.duration = this.baseDuration / this.attackSpeedStat;
			if (this.animator)
			{
				this.bodySideWeaponLayerIndex = this.animator.GetLayerIndex("Body, SideWeapon");
				this.animator.SetLayerWeight(this.bodySideWeaponLayerIndex, 1f);
			}
			if (this.crosshairOverridePrefab)
			{
				this.originalCrosshairPrefab = base.characterBody.crosshairPrefab;
				base.characterBody.crosshairPrefab = this.crosshairOverridePrefab;
			}
			base.characterBody.SetAimTimer(3f);
			this.durationPerMissile = FirePaint.baseDurationPerMissile / this.attackSpeedStat;
			base.PlayAnimation("Gesture, Additive", "IdleHarpoons");
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			bool flag = false;
			if (base.isAuthority)
			{
				this.stopwatch += Time.fixedDeltaTime;
				if (this.stopwatch >= this.durationPerMissile)
				{
					this.stopwatch -= this.durationPerMissile;
					while (this.fireIndex < this.targetsList.Count)
					{
						List<HurtBox> list = this.targetsList;
						int num = this.fireIndex;
						this.fireIndex = num + 1;
						HurtBox hurtBox = list[num];
						if (hurtBox.healthComponent && hurtBox.healthComponent.alive)
						{
							Vector3 position = base.inputBank.aimOrigin;
							if (transform != null)
							{
								position = transform.position;
							}
							if (this.effectPrefab)
							{
								EffectManager.SimpleMuzzleFlash(this.effectPrefab, base.gameObject, "MuzzlePistol", false);
							}

							Util.PlaySound(this.attackSoundString, base.gameObject);
							base.AddRecoil(-3f * this.recoilAmplitude, -4f * this.recoilAmplitude, -0.5f * this.recoilAmplitude, 0.5f * this.recoilAmplitude);
							this.FireMissile(hurtBox, position);
							flag = true;
							break;
						}
						//base.activatorSkillSlot.AddOneStock();
					}
					if (this.fireIndex >= this.targetsList.Count)
					{
						this.outer.SetNextState(new EntityStates.Bandit2.Weapon.ExitSidearmRevolver());
					}
				}
			}
			if (flag)
			{
				base.PlayAnimation((this.fireIndex % 2 == 0) ? "Gesture Left Cannon, Additive" : "Gesture Right Cannon, Additive", "FireHarpoon");
			}
			if (base.isAuthority && base.characterBody.isSprinting)
			{
				this.outer.SetNextStateToMain();
			}
		}

		private void FireMissile(HurtBox target, Vector3 position)
		{
			var aimVector = target.transform.position - position;

			BulletAttack bulletAttack = default;
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = position;
			bulletAttack.aimVector = aimVector;
			bulletAttack.minSpread = this.minSpread;
			bulletAttack.maxSpread = this.maxSpread;
			bulletAttack.bulletCount = 1U;
			bulletAttack.damage = damageCoefficient * this.damageStat;
			bulletAttack.force = this.force;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
			bulletAttack.tracerEffectPrefab = this.tracerEffectPrefab;
			bulletAttack.muzzleName = "MuzzlePistol";
			bulletAttack.hitEffectPrefab = this.hitEffectPrefab;
			bulletAttack.isCrit = base.RollCrit();
			bulletAttack.HitEffectNormal = false;
			bulletAttack.radius = this.bulletRadius;
			bulletAttack.damageType |= DamageType.BonusToLowHealth | DamageType.GiveSkullOnKill;
			bulletAttack.smartCollision = true;
			bulletAttack.Fire();
		}

		public override void OnExit()
		{
			if (this.animator)
			{
				this.animator.SetLayerWeight(this.bodySideWeaponLayerIndex, 0f);
			}
			base.PlayAnimation("Gesture, Additive", this.exitAnimationStateName);
			if (this.crosshairOverridePrefab)
			{
				base.characterBody.crosshairPrefab = this.originalCrosshairPrefab;
			}
			Transform transform = base.FindModelChild("SpinningPistolFX");
			if (transform)
			{
				transform.gameObject.SetActive(false);
			}
			base.OnExit();
			base.PlayCrossfade("Gesture, Additive", "ExitHarpoons", 0.1f);
		}

		public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
