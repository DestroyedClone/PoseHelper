using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace ROR1AltSkills.Acrid
{
    public class UtilitySkill : BaseSkillState
    {
		public static float baseDuration = AcridMain.CausticSludgeDuration;
		private float duration;
		public static GameObject projectilePrefab = AcridMain.acidPool; //EntityStates.Croco.BaseLeap.projectilePrefab;
		bool isCritAuthority = false;

		private int counter = 0;

		private readonly float acidPoolSize = AcridMain.CausticSludgeActualScale;
		private Vector3 lastPosition;
		private readonly float distanceLeniency = 0.9f;

		public override void OnEnter()
		{
			base.OnEnter();
			lastPosition = Vector3.zero;
			this.duration = baseDuration;

			this.isCritAuthority = base.RollCrit();

			if (base.isAuthority)
			{
				base.gameObject.layer = LayerIndex.fakeActor.intVal;
				base.characterMotor.Motor.RebuildCollidableLayers();
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration)
			{
				this.outer.SetNextStateToMain();
				return;
			}
			if (base.isAuthority)
			{
				if (base.characterBody)
				{
					base.characterBody.isSprinting = true;
				}

				float ratio = GetIdealVelocity().magnitude / characterBody.moveSpeed;
				int frequency = Mathf.FloorToInt(8f * ratio);
				if (counter % frequency == 0)
				{
					Vector3 footPosition = base.characterBody.footPosition;
					if (Vector3.Distance(characterBody.corePosition, lastPosition) >= acidPoolSize * distanceLeniency)
					{
						FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
						{
							projectilePrefab = projectilePrefab,
							crit = this.isCritAuthority,
							force = 0f,
							damage = characterBody.damage * 0.25f,
							owner = base.gameObject,
							rotation = Quaternion.identity,
							position = footPosition
						};
						ProjectileManager.instance.FireProjectile(fireProjectileInfo);
						lastPosition = characterBody.corePosition;
					}
				}
				counter++;
			}
		}

		public override void OnExit()
		{
			base.gameObject.layer = LayerIndex.defaultLayer.intVal;
			base.characterMotor.Motor.RebuildCollidableLayers();

			base.PlayAnimation("Fullbody, Override", "UtilityEnd");

			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		private Vector3 GetIdealVelocity()
		{
			return base.characterDirection.forward * Mathf.Sqrt((base.characterBody.moveSpeed * base.characterBody.moveSpeed) + 300f); //base.characterBody.moveSpeed * this.speedMultiplier;
		}
	}
}
