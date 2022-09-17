using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace AlternateSkills
{
    public static class Buffs
    {
        //internal static BuffDef acceleratedBuff;
        //internal static BuffDef tacticBuff;
        //internal static BuffDef promotedBuff;
        //internal static BuffDef tacticAllyBuff;
        //internal static BuffDef tacticEnemyBuff;
        //internal static BuffDef runningBuff;
        //internal static BuffDef promotedBuff;
        ///internal static BuffDef promotedScepterBuff;
        public static BuffDef mercAdrenalineBuff;
        public static BuffDef mercPeaceBuff;
        public static BuffDef crocoRemotePoisonDebuff;
        public static BuffDef captainAgilityBuff;

        internal static List<BuffDef> buffDefs = new List<BuffDef>();

        public static void RegisterBuffs()
        {   
            MainPlugin._logger.LogMessage("Registering Buffs");
            // fix the buff catalog to actually register our buffs

            MainPlugin._logger.LogMessage("Registering mercBuff");
            mercAdrenalineBuff = AddNewBuff("Adrenaline Rush", RoR2Content.Buffs.Energized.iconSprite, Color.yellow, true, false);
            MainPlugin._logger.LogMessage("Registering peaceBuff");
            mercPeaceBuff = AddNewBuff("Tranquility", RoR2Content.Buffs.LunarShell.iconSprite, Color.blue, false, false);
            crocoRemotePoisonDebuff = AddNewBuff("Infectious Gouge", RoR2Content.Buffs.Poisoned.iconSprite, Color.green, false, true);
            captainAgilityBuff = AddNewBuff("Agile Treads", RoR2Content.Buffs.BugWings.iconSprite, Color.green, false, false);
            //tacticAllyBuff = AddNewBuff("Tactics: Ally", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.blue, true, false);
            //tacticEnemyBuff = AddNewBuff("Tactics: Enemy", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.red, true, false);
            //runningBuff = AddNewBuff("Running!", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.red, false, false);
            ///promotedBuff = AddNewBuff("Promoted!", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.red, false, false);
            //promotedScepterBuff = AddNewBuff("Promoted! (Scepter)", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.red, false, false);

            //acceleratedBuff = AddNewBuff("Accelerated!", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.blue, false, false);
            //tacticBuff = AddNewBuff("Tactical Advantage", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.red, false, false);
            //promotedBuff = AddNewBuff("Promoted!", Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"), Color.yellow, false, false);
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

            return buffDef;
        }
    }
}
