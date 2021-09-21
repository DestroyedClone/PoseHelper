using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2.Skills;
using RoR2;
using EntityStates.Loader;

namespace ROR1AltSkills.Loader
{
    public class FireStraightHook : FireYankHook
    {
        public FireStraightHook()
        {
            projectilePrefab = LoaderMain.StraightHookProjectile;
            damageCoefficient = 2.1f;
        }

    }
}
