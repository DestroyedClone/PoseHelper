using System;
using BepInEx;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace MyNameSpace
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(
        "com.MyName.IHerebyGrantPermissionToDeprecateMyModFromThunderstoreBecauseIHaveNotChangedTheName",
        "IHerebyGrantPermissionToDeprecateMyModFromThunderstoreBecauseIHaveNotChangedTheName",
        "1.0.0")]
    [R2APISubmoduleDependency(nameof(ContentAddition), nameof(LanguageAPI))]
    public class sExamplePlugin : BaseUnityPlugin
    {
        public static GameObject myCharacter;

        public void Start()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ExampleState));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 6;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.dontAllowPastMaxStocks = true;
            mySkillDef.forceSprintDuringState = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.icon = null;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isCombatSkill = false;
            mySkillDef.keywordTokens = null;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.skillDescriptionToken = "CHARACTERNAME_SKILLSLOT_SKILLNAME_DESCRIPTION";
            mySkillDef.skillName = "SkillName";
            mySkillDef.skillNameToken = "CHARACTERNAME_SKILLSLOT_SKILLNAME_NAME";
            mySkillDef.stockToConsume = 1;
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
        }
    }
}