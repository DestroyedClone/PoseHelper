using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates;
using EntityStates.Engi.EngiWeapon;
using EntityStates.Merc.Weapon;

namespace AlternateSkills.Merc
{
	public class ESFireSlash : BaseSkillState
	{
		public float damageCoefficient = 20f;
		public Animator animator;
		public float duration = 0.25f;
        private void FireSlash()
        {
			PlayAnimation();
			base.characterBody.AddSpreadBloom(FireGrenades.spreadBloomValue);
            if (base.isAuthority)
            {
                var resultingDamageCoefficient = damageCoefficient * charge;
                Debug.Log($"Merc Fire Slash coeff: {resultingDamageCoefficient}");
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.attacker = gameObject;
				blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
				blastAttack.baseDamage = resultingDamageCoefficient * damageStat;
				blastAttack.crit = base.RollCrit();
				blastAttack.damageType = DamageType.ResetCooldownsOnKill;
				blastAttack.falloffModel = BlastAttack.FalloffModel.None;
				blastAttack.inflictor = gameObject;
				blastAttack.losType = BlastAttack.LoSType.None;
				blastAttack.position = characterBody.corePosition;
				blastAttack.procCoefficient = 1f;
				blastAttack.radius = 20 * charge;
				blastAttack.teamIndex = teamComponent.teamIndex;
				var result = blastAttack.Fire();
				
            }
        }

        public void PlayAnimation()
        {
			var animationStateName = "GroundLight3";
			var soundString = GroundLight2.slash3Sound;

			//bool @bool = this.animator.GetBool("isMoving");
			//bool bool2 = this.animator.GetBool("isGrounded");
			bool @bool = this.animator.GetBool("isMoving");
			bool bool2 = this.animator.GetBool("isGrounded");
			if (!@bool && bool2)
			{
				base.PlayCrossfade("FullBody, Override", animationStateName, "GroundLight.playbackRate", this.duration, 0.05f);
			}
			else
			{
				base.PlayCrossfade("Gesture, Additive", animationStateName, "GroundLight.playbackRate", this.duration, 0.05f);
				base.PlayCrossfade("Gesture, Override", animationStateName, "GroundLight.playbackRate", this.duration, 0.05f);
			}
			Util.PlaySound(soundString, base.gameObject);
        }

		public override void OnEnter()
		{
			this.duration = FireGrenades.baseDuration / this.attackSpeedStat;
			this.modelTransform = base.GetModelTransform();
			base.StartAimMode(2f, false);
            damageCoefficient = 20;
            animator = GetModelAnimator();
			base.OnEnter();
		}

		public override void OnExit()
		{
			base.OnExit();
            FireSlash();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge >= this.duration)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		public static GameObject effectPrefab;

		public static GameObject projectilePrefab;

		public float charge = 0.01f;

		private Transform modelTransform;
	}
}
