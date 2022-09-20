using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using R2API.Utils;

namespace AlternateSkills.Modules
{
    public static class DamageTypes
    {
        internal static DamageAPI.ModdedDamageType DTCrocoPoisonCountdown;
        internal static DamageAPI.ModdedDamageType DTCommandoReinforcedBullet;

        internal static List<BuffDef> damageTypes = new List<BuffDef>();

        internal static void RegisterDamageTypes()
        {
            DTCrocoPoisonCountdown = DamageAPI.ReserveDamageType();
            DTCommandoReinforcedBullet = DamageAPI.ReserveDamageType();
        }
    }
}
