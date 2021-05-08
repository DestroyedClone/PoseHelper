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

namespace AlternateSkills.Acrid
{
    public class KickOff : BaseSkillState, IOnKilledOtherServerReceiver
    {
        public void OnEnter()
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
			this.leftFistEffectInstance = UnityEngine.Object.Instantiate<GameObject>(this.fistEffectPrefab, base.FindModelChild("MuzzleHandL"));
			this.rightFistEffectInstance = UnityEngine.Object.Instantiate<GameObject>(this.fistEffectPrefab, base.FindModelChild("MuzzleHandR"));
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

		protected override void UpdateAnimationParameters()
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
				base.characterMotor.moveDirection = base.inputBank.moveVector;
				if (base.fixedAge >= BaseLeap.minimumDuration && (this.detonateNextFrame || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
				{
					this.DoImpactAuthority();
					this.outer.SetNextStateToMain();
				}
			}
		}

		protected virtual void DoImpactAuthority()
		{
			if (BaseLeap.landingSound)
			{
				EffectManager.SimpleSoundEffect(BaseLeap.landingSound.index, base.characterBody.footPosition, true);
			}
		}

		public void OnKilledOtherServer(DamageReport damageReport)
        {
            if (damageReport.attacker && damageReport.attackerBody == this.characterBody)
            {

            }
        }
    }
}
