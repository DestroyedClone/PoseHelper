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

namespace AlternateSkills.Commando
{
    public class CommandoMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/CommandoBody");

        public static void Init()
        {
            SetupSkills();
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("COMMANDO_SECONDARY_BACKUPSHIV_NAME", "Backup Shiv");
            LanguageAPI.Add("COMMANDO_SECONDARY_BACKUPSHIV_DESCRIPTION", "Slice for <style=cDamage>220% damage</style>. <style=cHealing>Recover 7% health</style> on kill.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Commando.BackupShiv));
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
            mySkillDef.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            mySkillDef.skillDescriptionToken = "COMMANDO_SECONDARY_BACKUPSHIV_DESCRIPTION";
            mySkillDef.skillName = "COMMANDO_SECONDARY_BACKUPSHIV_NAME";
            mySkillDef.skillNameToken = "COMMANDO_SECONDARY_BACKUPSHIV_NAME";

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
