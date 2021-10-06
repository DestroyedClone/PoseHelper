using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates.Croco;

namespace ROR1AltSkills.Acrid
{
	public class LeapDropAcid : ModifiedBaseLeap
	{
		public LeapDropAcid()
        {
			SetToMainOnImpact = false;
		}
		public static float baseDuration = AcridMain.CausticSludgeDuration;
		private float duration;

		private int counter = 0;

		private readonly float acidPoolSize = AcridMain.CausticSludgeActualScale;
		private Vector3 lastPosition;
		private readonly float distanceLeniency = 0.9f;
		public static GameObject groundedAcid;

		private bool hasRebuiltLayers = false;

		public override void OnEnter()
        {
            base.OnEnter();
			lastPosition = Vector3.zero;
			duration = baseDuration;
			isCritAuthority = RollCrit();
		}

		public void RebuildLayersStart()
		{
			if (isAuthority)
			{
				gameObject.layer = LayerIndex.fakeActor.intVal;
				characterMotor.Motor.RebuildCollidableLayers();
			}
		}

        public override void DoImpactAuthority()
        {
            base.DoImpactAuthority();
			PlayLandingAnimation();
			RebuildLayersStart();
		}

        public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (characterBody)
			{
				characterBody.isSprinting = true;
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
						crit = isCritAuthority,
						force = 0f,
						damage = characterBody.damage * 0.25f,
						owner = gameObject,
						rotation = Quaternion.identity,
						position = characterBody.corePosition
					};
					if (hasLanded)
                    {
						fireProjectileInfo.projectilePrefab = groundedAcid;
						fireProjectileInfo.position = footPosition;
					}

					ProjectileManager.instance.FireProjectile(fireProjectileInfo);
					lastPosition = characterBody.corePosition;
				}
			}
			counter++;
			if (fixedAge >= duration)
			{
				outer.SetNextStateToMain();
				return;
			}
		}

        public override void OnExit()
		{
			RebuildLayersEnd();
			base.OnExit();
		}

		public void RebuildLayersEnd()
		{
			if (!hasRebuiltLayers)
			{
				gameObject.layer = LayerIndex.defaultLayer.intVal;
				characterMotor.Motor.RebuildCollidableLayers();
				hasRebuiltLayers = true;
			}
		}

		private Vector3 GetIdealVelocity()
		{
			return characterDirection.forward * Mathf.Sqrt((characterBody.moveSpeed * characterBody.moveSpeed) + 300f); //base.characterBody.moveSpeed * this.speedMultiplier;
		}
	}
}
