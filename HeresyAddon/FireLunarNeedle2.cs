using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates.GlobalSkills.LunarNeedle;
using EntityStates;

namespace HeresyAddon.EntityStates
{
	public class FireLunarNeedle : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = FireLunarNeedle.baseDuration / this.attackSpeedStat;
			if (base.isAuthority)
			{
				Ray aimRay = base.GetAimRay();
				aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, FireLunarNeedle.maxSpread, 1f, 1f, 0f, 0f);
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
				fireProjectileInfo.crit = base.characterBody.RollCrit();
				fireProjectileInfo.damage = base.characterBody.damage * FireLunarNeedle.damageCoefficient;
				fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.procChainMask = default(ProcChainMask);
				fireProjectileInfo.force = 0f;
				fireProjectileInfo.useFuseOverride = false;
				fireProjectileInfo.useSpeedOverride = false;
				fireProjectileInfo.target = null;
				fireProjectileInfo.projectilePrefab = FireLunarNeedle.projectilePrefab;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
			base.AddRecoil(-0.4f * FireLunarNeedle.recoilAmplitude, -0.8f * FireLunarNeedle.recoilAmplitude, -0.3f * FireLunarNeedle.recoilAmplitude, 0.3f * FireLunarNeedle.recoilAmplitude);
			base.characterBody.AddSpreadBloom(FireLunarNeedle.spreadBloomValue);
			base.StartAimMode(2f, false);
			EffectManager.SimpleMuzzleFlash(FireLunarNeedle.muzzleFlashEffectPrefab, base.gameObject, "Head", false);
			Util.PlaySound(FireLunarNeedle.fireSound, base.gameObject);
			base.PlayAnimation(this.animationLayerName, this.animationStateName, this.playbackRateParam, this.duration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge >= this.duration)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		public static float baseDuration;

		public static float damageCoefficient;

		public static GameObject projectilePrefab;

		public static float recoilAmplitude;

		public static float spreadBloomValue;

		public static GameObject muzzleFlashEffectPrefab;

		public static string fireSound;

		public static float maxSpread;

		private float duration;

		[SerializeField]
		public string animationLayerName = "Gesture, Override";

		[SerializeField]
		public string animationStateName = "FireLunarNeedle";

		[SerializeField]
		public string playbackRateParam = "FireLunarNeedle.playbackRate";
	}
}
