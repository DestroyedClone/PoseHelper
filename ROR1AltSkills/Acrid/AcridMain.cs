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

namespace ROR1AltSkills.Acrid
{
    public class AcridMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/CrocoBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static void Init()
        {
            SetupSkills();
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("DC_CROCO_PRIMARY_FESTERINGWOUNDS_NAME", "Festering Wounds");
            LanguageAPI.Add("DC_CROCO_PRIMARY_FESTERINGWOUNDS_DESCRIPTION", "Maul an enemy for <style=cIsDamage>120% damage</style>. The target is poisoned for <style=cIsDamage>24% damage per second</style>.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Croco.Bite));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
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
            mySkillDef.skillDescriptionToken = "DC_CROCO_PRIMARY_FESTERINGWOUNDS_DESCRIPTION";
            mySkillDef.skillName = "DC_CROCO_PRIMARY_FESTERINGWOUNDS_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            LanguageAPI.Add("DC_CROCO_UTILITY_CAUSTICSLUDGE_NAME", "Caustic Sludge");
            LanguageAPI.Add("DC_CROCO_UTILITY_CAUSTICSLUDGE_DESCRIPTION", "Secrete <style=cIsDamage>poisonous sludge</style> for 2 seconds. <style=cIsUtility>Speeds up allies,</style> while <style=cIsDamage>slowing and hurting enemies</style> for <style=cIsDamage>90% damage</style>");

            var mySkillDefUtil = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDefUtil.activationState = new SerializableEntityStateType(typeof(Acrid.UtilitySkill));
            mySkillDefUtil.activationStateMachineName = "Body";
            mySkillDefUtil.baseMaxStock = 1;
            mySkillDefUtil.baseRechargeInterval = 12f;
            mySkillDefUtil.beginSkillCooldownOnSkillEnd = true;
            mySkillDefUtil.canceledFromSprinting = false;
            mySkillDefUtil.fullRestockOnAssign = true;
            mySkillDefUtil.interruptPriority = InterruptPriority.Any;
            mySkillDefUtil.isCombatSkill = false;
            mySkillDefUtil.mustKeyPress = true;
            mySkillDefUtil.rechargeStock = 1;
            mySkillDefUtil.requiredStock = 1;
            mySkillDefUtil.stockToConsume = 1;
            mySkillDefUtil.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDefUtil.skillDescriptionToken = "DC_CROCO_UTILITY_CAUSTICSLUDGE_DESCRIPTION";
            mySkillDefUtil.skillName = "DC_CROCO_UTILITY_CAUSTICSLUDGE_NAME";
            mySkillDefUtil.skillNameToken = mySkillDefUtil.skillName;

            LoadoutAPI.AddSkillDef(mySkillDefUtil);

            skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDefUtil,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDefUtil.skillNameToken, false, null)
            };
        }
    }
}
