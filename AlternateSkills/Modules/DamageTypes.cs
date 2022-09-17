using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using R2API.Utils;

namespace AlternateSkills
{
    public static class DamageTypes
    {
        internal static DamageAPI.ModdedDamageType DTCrocoPoisonCountdown;

        internal static List<BuffDef> buffDefs = new List<BuffDef>();

        internal static void RegisterBuffs()
        {
        }

        // simple helper method
        internal static BuffDef AddNewBuff(string buffName, Sprite buffIcon, Color buffColor, bool canStack, bool isDebuff)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.eliteDef = null;
            buffDef.iconSprite = buffIcon;
            buffDef.isCooldown = false;
            buffDef.isHidden = false;

            buffDefs.Add(buffDef);
            R2API.ContentAddition.AddBuffDef(buffDef);

            return buffDef;
        }
    }
}
