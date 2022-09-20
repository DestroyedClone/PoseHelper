using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using System.Linq;
using RoR2.Orbs;
using AlternateSkills.Modules;

namespace AlternateSkills.Loader
{
    public class LoaderMain : SurvivorMain
    {
        public override string CharacterName => "Loader";
        public string TokenPrefix = "DCALTSKILLS_LOADER";

        public override void Init(ConfigFile config)
        {
            return;
        }
        public override void SetupUtility()
        {
            var mySkillDef = ScriptableObject.CreateInstance<HuntressTrackingSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESStreamline));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 3;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.utility.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_UTILITY";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            utilitySkillDefs.Add(mySkillDef);
            base.SetupUtility();
        }
    }
}
