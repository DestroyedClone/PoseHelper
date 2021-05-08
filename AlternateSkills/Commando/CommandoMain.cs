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
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static void Init()
        {
            SetupSkills();
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self && self.body && self.body.HasBuff(Buffs.runningBuff))
            {
                self.itemCounts.bear += 5;
            }
            orig(self, damageInfo);
        }

        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self)
            {
                if (self.HasBuff(Buffs.runningBuff))
                {
                    self.moveSpeed *= 1.25f;
                }
                if (self.HasBuff(Buffs.promotedBuff))
                {
                    self.attackSpeed *= 1.5f;
                    self.damage *= 1.25f;
                    self.armor += 20f;
                    self.regen *= 2f;
                }
            }
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("COMMANDO_SECONDARY_BACKUPSHIV_NAME", "Backup Shiv");
            LanguageAPI.Add("COMMANDO_SECONDARY_BACKUPSHIV_DESCRIPTION", "Slice for <style=cIsDamage>220% damage</style>. <style=cIsHealing>Recover 7% health</style> on kill.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Commando.BackupShiv));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 7f;
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

            LanguageAPI.Add("COMMANDO_UTILITY_RUN_NAME", "Run");
            LanguageAPI.Add("COMMANDO_UTILITY_RUN_DESCRIPTION", "Hold to <style=cIsUtility>run for 25% increased movement speed<style>. Grants a chance to dodge.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Commando.RunSkill));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.forceSprintDuringState = true;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffCloakIcon");
            mySkillDef.skillDescriptionToken = "COMMANDO_UTILITY_RUN_DESCRIPTION";
            mySkillDef.skillName = "COMMANDO_UTILITY_RUN_NAME";
            mySkillDef.skillNameToken = "COMMANDO_UTILITY_RUN_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            skillLocator = myCharacter.GetComponent<SkillLocator>();

            skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            LanguageAPI.Add("COMMANDO_SPECIAL_PROMOTE_NAME", "Promote");
            LanguageAPI.Add("COMMANDO_SPECIAL_PROMOTE_DESCRIPTION", "Promote an ally, grants +50% attack speed, +25% damage, +20 armor, and +100% health regen for 8 seconds.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Commando.Promote));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 60f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffWarbannerIcon");
            mySkillDef.skillDescriptionToken = "COMMANDO_SPECIAL_PROMOTE_DESCRIPTION";
            mySkillDef.skillName = "COMMANDO_SPECIAL_PROMOTE_NAME";
            mySkillDef.skillNameToken = "COMMANDO_SPECIAL_PROMOTE_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            skillLocator = myCharacter.GetComponent<SkillLocator>();

            skillFamily = skillLocator.special.skillFamily;

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
