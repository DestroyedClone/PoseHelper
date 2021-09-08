using System;
using System.Collections.Generic;
using System.Text;
using R2API;

namespace AlternateSkills.Captain
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType irradiateDamageType;

        public static void SetupDamageTypes()
        {
            irradiateDamageType = DamageAPI.ReserveDamageType();
        }
    }
}
