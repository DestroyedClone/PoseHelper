using EntityStates;
using static AlternateSkills.Merc.MercenaryMain;
using EntityStates.Treebot.Weapon;
using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace AlternateSkills.Loader
{
	public class ESStreamline : BaseSkillState
    {
        // The last attack's type and position are stored on comp
        // 1. Enter skill
        // 2. Get info from comp
        // 3. Activate
        // 4. Leave

        public override void OnEnter()
        {
            base.OnEnter();
            OnExit();
        }

        public void Launch()
        {
            if (base.isAuthority)
            {
                var aimRay = base.GetAimRay();
				float num3 = base.characterBody.characterMotor ? base.characterBody.characterMotor.mass : 1f;
				float acceleration2 = base.characterBody.acceleration;
				float num4 = Trajectory.CalculateInitialYSpeedForHeight(15, -acceleration2);
				base.characterBody.characterMotor.ApplyForce(-num4 * num3 * aimRay.direction, false, false);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Launch();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return base.GetMinimumInterruptPriority();
        }

    }
}