using System;
using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Projectile;

namespace AlternateSkills.Treebot
{
    public class TreebotFireFruitSeedSCEPTER : BaseState
    {
		public override void OnEnter()
		{
			base.OnEnter();
			EffectManager.SimpleMuzzleFlash(this.muzzleFlashPrefab, base.gameObject, this.muzzleName, false);
			this.duration = this.baseDuration / this.attackSpeedStat;
			Util.PlaySound(this.enterSoundString, base.gameObject);
			base.PlayAnimation(this.animationLayerName, this.animationStateName, this.playbackRateParam, this.duration);
			if (base.isAuthority)
			{
				Ray aimRay = base.GetAimRay();
				FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
				{
					crit = base.RollCrit(),
					damage = this.damageCoefficient * this.damageStat,
					damageColorIndex = DamageColorIndex.Default,
					damageTypeOverride = DamageType.LunarSecondaryRootOnHit,
					force = 0f,
					owner = base.gameObject,
					position = aimRay.origin,
					procChainMask = default(ProcChainMask),
					projectilePrefab = this.projectilePrefab,
					rotation = Quaternion.LookRotation(aimRay.direction),
					useSpeedOverride = false
				};
				for (int i = 0; i < 3; i++)
				{
					ProjectileManager.instance.FireProjectile(fireProjectileInfo);
				}
			}
		}

		// Token: 0x06003AFE RID: 15102 RVA: 0x000E85D6 File Offset: 0x000E67D6
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge >= this.duration)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x06003AFF RID: 15103 RVA: 0x0006E4AF File Offset: 0x0006C6AF
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		// Token: 0x040031A0 RID: 12704
		[SerializeField]
		public GameObject projectilePrefab;

		// Token: 0x040031A1 RID: 12705
		[SerializeField]
		public float baseDuration;

		// Token: 0x040031A2 RID: 12706
		[SerializeField]
		public float damageCoefficient;

		// Token: 0x040031A3 RID: 12707
		[SerializeField]
		public string enterSoundString;

		// Token: 0x040031A4 RID: 12708
		[SerializeField]
		public string muzzleName;

		// Token: 0x040031A5 RID: 12709
		[SerializeField]
		public GameObject muzzleFlashPrefab;

		// Token: 0x040031A6 RID: 12710
		[SerializeField]
		public string animationLayerName = "Gesture, Additive";

		// Token: 0x040031A7 RID: 12711
		[SerializeField]
		public string animationStateName = "FireFlower";

		// Token: 0x040031A8 RID: 12712
		[SerializeField]
		public string playbackRateParam = "FireFlower.playbackRate";

		// Token: 0x040031A9 RID: 12713
		private float duration;
	}
}
