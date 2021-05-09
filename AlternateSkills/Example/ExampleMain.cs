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

namespace MyNameSpace.Example
{
    public class ExampleMain
    {
        // myCharacter should either be
        // Resources.Load<GameObject>("prefabs/characterbodies/BanditBody");
        // or BodyCatalog.FindBodyIndex("BanditBody");
        // COMMENT: should probably use a base class to inherit or something but this is probably fine enough for the avg user
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/CommandoBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static void Init()
        {
            SetupSkills();
        }

        private static void SetupSkills()
        {
            // If you're confused about the language tokens, they're the proper way to add any strings used by the game.
            // We use AssetPlus API for that
            // ADD: -> We use R2API's LanguageAPI for that.
            LanguageAPI.Add("COMMANDO_SECONDARY_BACKUPSHIV_NAME", "Backup Shiv");
            LanguageAPI.Add("COMMANDO_SECONDARY_BACKUPSHIV_DESCRIPTION", "Slice for <style=cIsDamage>220% damage</style>. <style=cIsHealing>Recover 7% health</style> on kill.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(MyNameSpace.MyEntityStates.ExampleState));
            // ADD: some comment about how the machines work
            // ADD: some comment about looking at the prefabs for the name of the machine
            //      (EntityStateMachine) in the prefabs
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 7f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.dontAllowPastMaxStocks = true;
            mySkillDef.forceSprintDuringState = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            mySkillDef.skillDescriptionToken = "COMMANDO_SECONDARY_BACKUPSHIV_DESCRIPTION";
            mySkillDef.skillName = "COMMANDO_SECONDARY_BACKUPSHIV_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[] { "KEYWORD_RAPIDFIRE" };

            LoadoutAPI.AddSkillDef(mySkillDef);
            //This adds our skilldef. If you don't do this, the skill will not work.

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            //Note; if your character does not originally have a skill family for this, use the following:
            //skillLocator.special = gameObject.AddComponent<GenericSkill>();
            //var newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            //LoadoutAPI.AddSkillFamily(newFamily);
            //skillLocator.special.SetFieldValue("_skillFamily", newFamily);
            //var specialSkillFamily = skillLocator.special.skillFamily;

            //Note; you can change component.primary to component.secondary , component.utility and component.special
            var skillFamily = skillLocator.secondary.skillFamily;

            //If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the exisiting Skill Family.
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            //Note; if your character does not originally have a skill family for this, use the following:
            //skillFamily.variants = new SkillFamily.Variant[1]; // substitute 1 for the number of skill variants you are implementing

            //If this is the default/first skill, copy this code and remove the //,
            //skillFamily.variants[0] = new SkillFamily.Variant
            //{
            //    skillDef = mySkillDef,
            //    unlockableName = "",
            //    viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            //};
        }
    }
}
