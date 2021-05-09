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

namespace AlternateSkills.Bandit2
{
    public class Bandit2Main
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/Bandit2Body");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static void Init()
        {
            SetupSkills();
        }


        private static void SetupSkills()
        {
            LanguageAPI.Add("BANDIT2SCEPTER_NAME", "Eclipse");
            LanguageAPI.Add("BANDIT2SCEPTER_DESCRIPTION", "<style=cIsUtility>Cripples</style> nearby enemies within a radius of <style=cIsDamage>7 meters</style>.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Bandit2.PrepEclipse));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 4f;
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
            mySkillDef.skillDescriptionToken = "BANDIT2SCEPTER_DESCRIPTION";
            mySkillDef.skillName = "BANDIT2SCEPTER_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.special.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            LanguageAPI.Add("BANDIT2DESPERADOSCEPTER_NAME", "Renegade");
            LanguageAPI.Add("BANDIT2DESPERADOSCEPTER_DESCRIPTION", "Marks up to three targets before firing.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Bandit2.PrepPaint));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 4f;
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
            mySkillDef.skillDescriptionToken = "BANDIT2DESPERADOSCEPTER_DESCRIPTION";
            mySkillDef.skillName = "BANDIT2DESPERADOSCEPTER_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            LoadoutAPI.AddSkillDef(mySkillDef);

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }
    }
}
