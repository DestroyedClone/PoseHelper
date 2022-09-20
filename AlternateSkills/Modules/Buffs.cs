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
        public static BuffDef mercAdrenalineBuff = null;
        public static BuffDef mercPeaceBuff = null;
        public static BuffDef crocoRemotePoisonDebuff = null;
        public static BuffDef captainAgilityBuff = null;
        public static BuffDef commandoSquadronBuff;
        public static BuffDef commandoReinforcingVisualBuff;

        internal static List<BuffDef> buffDefs = new List<BuffDef>();

        public static void RegisterBuffs()
        {   
            // fix the buff catalog to actually register our buffs

            mercAdrenalineBuff = AddNewBuff("Adrenaline Rush", null, Color.yellow, true, false);
            mercPeaceBuff = AddNewBuff("Tranquility", null, Color.blue, false, false);
            crocoRemotePoisonDebuff = AddNewBuff("Infectious Gouge", null, Color.green, false, true);
            captainAgilityBuff = AddNewBuff("Agile Treads", null, Color.green, false, false);
            commandoSquadronBuff = AddNewBuff("Squadron", null, Color.yellow, false, false);
            commandoReinforcingVisualBuff = AddNewBuff("Reinforcing Ally!", null, Color.blue, true, false, true);
            
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
        internal static BuffDef AddNewBuff(string buffName, Sprite buffIcon, Color buffColor, bool canStack, bool isDebuff, bool isCooldown = false)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.eliteDef = null;
            buffDef.iconSprite = buffIcon;
            buffDef.isCooldown = isCooldown;
            buffDef.isHidden = false;
            R2API.ContentAddition.AddBuffDef(buffDef);

            buffDefs.Add(buffDef);

            return buffDef;
        }
    }
}
