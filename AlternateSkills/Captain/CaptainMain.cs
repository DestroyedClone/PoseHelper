﻿using Mono.Cecil.Cil;
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

namespace AlternateSkills.Captain
{
    public class CaptainMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/CaptainBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static SkillDef callNuke;

        public static void Init()
        {

            SetupSkills();
            //On.RoR2.EntityStateCatalog.Init += EntityStateCatalog_Init;
        }

        private static void EntityStateCatalog_Init(On.RoR2.EntityStateCatalog.orig_Init orig)
        {
            Debug.Log($"ImpBossMonster");
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("CAPTAINSCEPTER_NAME", "");
            LanguageAPI.Add("CAPTAINSCEPTER_DESCRIPTION", ".");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Captain.SetupNukeAlt));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 20f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "CAPTAINSCEPTER_DESCRIPTION";
            mySkillDef.skillName = "CAPTAINSCEPTER_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.dontAllowPastMaxStocks = false;
            mySkillDef.forceSprintDuringState = false;
            mySkillDef.keywordTokens = new string[] { };
            mySkillDef.resetCooldownTimerOnUse = false;

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();
            var skillFamily = skillLocator.utility.skillFamily;
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };



            callNuke = ScriptableObject.CreateInstance<SkillDef>();
            callNuke.activationState = new SerializableEntityStateType(typeof(Captain.CallNukeAltEnter));
            callNuke.activationStateMachineName = "Weapon";
            callNuke.baseMaxStock = 1;
            callNuke.baseRechargeInterval = 0f;
            callNuke.beginSkillCooldownOnSkillEnd = true;
            callNuke.canceledFromSprinting = true;
            callNuke.fullRestockOnAssign = true;
            callNuke.interruptPriority = InterruptPriority.Any;
            callNuke.isCombatSkill = true;
            callNuke.mustKeyPress = true;
            callNuke.rechargeStock = 1;
            callNuke.requiredStock = 1;
            callNuke.stockToConsume = 1;
            callNuke.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            callNuke.skillDescriptionToken = "CAPTAINSCEPTER_DESCRIPTION";
            callNuke.skillName = "CAPTAINSCEPTER_NAME";
            callNuke.skillNameToken = callNuke.skillName;

            callNuke.cancelSprintingOnActivation = false;
            callNuke.dontAllowPastMaxStocks = false;
            callNuke.forceSprintDuringState = false;
            callNuke.keywordTokens = new string[] { };
            callNuke.resetCooldownTimerOnUse = false;
        }
    }
}
