using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace AlternateSkills.Modules
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
            MainPlugin._logger.LogMessage("Registering Buffs 14:15");
            // fix the buff catalog to actually register our buffs

            MainPlugin._logger.LogMessage("Registering mercBuff 14:15");
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
            int i = 0;
            MainPlugin._logger.LogMessage(i++);
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            MainPlugin._logger.LogMessage(i++);
            buffDef.name = buffName;
            MainPlugin._logger.LogMessage(i++);
            buffDef.buffColor = buffColor;
            MainPlugin._logger.LogMessage(i++);
            buffDef.canStack = canStack;
            MainPlugin._logger.LogMessage(i++);
            buffDef.isDebuff = isDebuff;
            MainPlugin._logger.LogMessage(i++);
            buffDef.eliteDef = null;
            MainPlugin._logger.LogMessage(i++);
            buffDef.iconSprite = buffIcon;
            MainPlugin._logger.LogMessage(i++);
            buffDef.isCooldown = false;
            MainPlugin._logger.LogMessage(i++);
            buffDef.isHidden = false;
            MainPlugin._logger.LogMessage(i++);
            R2API.ContentAddition.AddBuffDef(buffDef);
            MainPlugin._logger.LogMessage(i++);

            buffDefs.Add(buffDef);
            MainPlugin._logger.LogMessage(i++);

            return buffDef;
        }
    }
}
