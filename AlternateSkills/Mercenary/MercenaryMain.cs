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

namespace AlternateSkills.Mercenary
{
    public class MercenaryMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/MercBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static void Init()
        {
            SetupSkills();
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("MERC_SECONDARY_FALLINGLIGHT_NAME", "Falling Light");
            LanguageAPI.Add("MERC_SECONDARY_FALLINGLIGHTDESCRIPTION", "<style=cIsUtility>Heavy.</style> Unleash a slicing downwardcut, dealing <style=cIsDamage>550% damage</style> and sending you downwards.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Mercenary.FallingLight));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 2.5f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            mySkillDef.skillDescriptionToken = "MERC_SECONDARY_FALLINGLIGHT_DESCRIPTION";
            mySkillDef.skillName = "MERC_SECONDARY_FALLINGLIGHT_NAME";
            mySkillDef.skillNameToken = "MERC_SECONDARY_FALLINGLIGHT_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.secondary.skillFamily;

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
