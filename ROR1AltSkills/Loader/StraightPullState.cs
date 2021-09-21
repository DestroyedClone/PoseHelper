using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Projectile;
using EntityStates;
using UnityEngine;

namespace ROR1AltSkills.Loader
{
    public class StraightPullState : ProjectileGrappleController.BaseGripState
	{
		public static float yankSpeed;
		public static float delayBeforeYanking;
		public static float hoverTimeLimit = 0f;
		private CharacterBody stuckBody;

		public override void OnEnter()
		{
			base.OnEnter();
			stuckBody = grappleController.projectileStickOnImpactController.stuckBody;
		}


		public override void FixedUpdateBehavior()
		{
			base.FixedUpdateBehavior();
			if (stuckBody)
			{
				if (Util.HasEffectiveAuthority(stuckBody.gameObject))
				{
					Vector3 a = position - aimOrigin;
					IDisplacementReceiver component = characterBody.GetComponent<IDisplacementReceiver>();
					if ((Component)component && base.fixedAge >= ProjectileGrappleController.YankState.delayBeforeYanking)
					{
						component.AddDisplacement(a * (ProjectileGrappleController.YankState.yankSpeed * Time.fixedDeltaTime));
					}
				}
				if (base.owner.hasEffectiveAuthority && base.owner.characterMotor && base.fixedAge < ProjectileGrappleController.YankState.hoverTimeLimit)
				{
					Vector3 velocity = base.owner.characterMotor.velocity;
					if (velocity.y < 0f)
					{
						velocity.y = 0f;
						base.owner.characterMotor.velocity = velocity;
					}
				}
			}
		}
	}
}
