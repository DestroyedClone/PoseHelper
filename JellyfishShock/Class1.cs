using System;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using RoR2.Skills;
using EntityStates;
using KinematicCharacterController;

namespace JellyfishShock
{
    [BepInPlugin("com.DestroyedClone.JellyfishShocks", "Jellyfish Shocks", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI))]
    public class JellyfishShocksPlugin : BaseUnityPlugin
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/JellyfishBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public void Awake()
        {
            SetupBody();
            SetupSkills();
            SetupLanguage();
        }

        private static void SetupBody()
        {
            myCharacter.GetComponent<SetStateOnHurt>().canBeHitStunned = false;
            myCharacter.GetComponent<SphereCollider>().enabled = false;
            myCharacter.GetComponent<Rigidbody>().mass = 999999f;
        }

        private static void SetupLanguage()
        {
            var lore_en = "Field Notes:  An airborne creature, capable of flight using a combination of gases in its clear hull. Like the Jellyfish on earth, they also use pulsation to aid in locomotion; however, rather than a series of tentacles they have two 'branches' made of many tentacles wrapped around themselves." +
                "\n\nAlso like the Jellyfish, they have quite the sting, capable of penetrating my weather shielding. The same gases used for flight are used to create a very powerful electrostatic charge." +
                "\n\nWhen they are not busy hunting me, the Jellyfish have been seen sunbathing and absorbing the strange fumes from the ground.";

            LanguageAPI.Add("JELLYFISH_LORE", lore_en, "EN");
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("DESTROYEDCLONE_JELLYFISHSHOCK_NAME", "Discharge");
            LanguageAPI.Add("DESTROYEDCLONE_JELLYFISHSHOCK_DESCRIPTION", "Shocks nearby enemies for <style=cIsDamage>80% damage</style>.");


            var skillLocator = myCharacter.GetComponent<SkillLocator>();
            var skillFamily = skillLocator.secondary.skillFamily;
            var defaultSkillDef = skillFamily.variants[(int)skillFamily.defaultVariantIndex].skillDef;
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();

            mySkillDef.activationState = new SerializableEntityStateType(typeof(JellyShockSkill));
            mySkillDef.activationStateMachineName = defaultSkillDef.activationStateMachineName;
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 1f;
            mySkillDef.beginSkillCooldownOnSkillEnd = defaultSkillDef.beginSkillCooldownOnSkillEnd;
            mySkillDef.canceledFromSprinting = defaultSkillDef.canceledFromSprinting;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = defaultSkillDef.interruptPriority;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "DESTROYEDCLONE_JELLYFISHSHOCK_DESCRIPTION";
            mySkillDef.skillName = "DESTROYEDCLONE_JELLYFISHSHOCK_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.dontAllowPastMaxStocks = false;
            mySkillDef.forceSprintDuringState = false;
            mySkillDef.keywordTokens = new string[] { };
            mySkillDef.resetCooldownTimerOnUse = false;

            LoadoutAPI.AddSkillDef(mySkillDef);

            skillFamily.variants[(int)skillFamily.defaultVariantIndex].skillDef = mySkillDef;
        }
    }
}
