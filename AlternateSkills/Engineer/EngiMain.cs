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
using AlternateSkills.Modules;

namespace AlternateSkills.Engi
{
    public class EngiMain : SurvivorMain
    {
        public override string CharacterName => "Engi";
        public string TokenPrefix = "DCALTSKILLS_ENGI";
        public static DeployableSlot DeployableSlot_MechanicalClone;
        public static int DeployableSlot_MechanicalClone_MaxCount = 1;

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            SetupDeployableSlots();
        }

        public void SetupDeployableSlots()
        {
            BodyPrefab.AddComponent<AllyTracker>();
            DeployableSlot_MechanicalClone = DeployableAPI.RegisterDeployableSlot((master, multiplier) => DeployableSlot_MechanicalClone_MaxCount);
        }

        public override void SetupSpecial()
        {
            var mySkillDef = ScriptableObject.CreateInstance<AllyTrackingSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESCreateClone));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 20;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.special.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_SPECIAL";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            specialSkillDefs.Add(mySkillDef);
            base.SetupSpecial();
        }
    }
}
