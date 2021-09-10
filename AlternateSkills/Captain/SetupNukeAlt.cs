using System;
using System.Collections.Generic;
using System.Text;
using EntityStates.Captain.Weapon;
using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Projectile;

namespace AlternateSkills.Captain
{
    public class SetupNukeAlt : SetupAirstrikeAlt
    {
        public SetupNukeAlt()
        {
            SetupNukeAlt.primarySkillDef = CaptainMain.callNuke;
        }
    }

    public class CallNukeAltEnter : CallAirstrikeAltEnter
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                this.outer.SetNextState(new CallNukeAlt());
            }
        }
    }

    public class CallNukeAlt : CallAirstrikeAlt
    {
        public override void OnEnter()
        {
            this.damageCoefficient = 100000;
            base.OnEnter();
        }

        public override void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
        {
            fireProjectileInfo.projectilePrefab = Projectiles.nukeProjectile;
            base.ModifyProjectile(ref fireProjectileInfo);
        }
    }
}
