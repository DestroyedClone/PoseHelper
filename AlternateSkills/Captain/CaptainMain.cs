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
using System.Linq;
using RoR2.Orbs;
using AlternateSkills.Modules;

namespace AlternateSkills.Captain
{
    public class CaptainMain : SurvivorMain
    {
        public override string CharacterName => "Captain";
        public string TokenPrefix = "DCALTSKILLS_CAPTAIN";

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            SetupCaptainBody();
            R2API.RecalculateStatsAPI.GetStatCoefficients += CaptainBuffs;
        }
        
        public void CaptainBuffs(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Buffs.captainAgilityBuff))
            {
                args.moveSpeedMultAdd += 0.5f;
                args.jumpPowerMultAdd -= 10;
            }
        }

        public void SetupCaptainBody()
        {
            var bodyPrefab = RoR2Content.Survivors.Captain.bodyPrefab;
            bodyPrefab.GetComponent<CaptainDefenseMatrixController>().enabled = false;
            bodyPrefab.AddComponent<CaptainItemController>();
            var skillLoctor = bodyPrefab.GetComponent<SkillLocator>();
            skillLoctor.passiveSkill.skillNameToken = TokenPrefix+"_PASSIVE_NAME";
            skillLoctor.passiveSkill.skillDescriptionToken = TokenPrefix+"_PASSIVE_DESC";
        }


        public override void SetupPrimary()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESChargeAgileShotgun));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 0;
            mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_PRIMARY";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            primarySkillDefs.Add(mySkillDef);
            base.SetupPrimary();
        }

        public override void SetupSecondary()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESExciteTarget));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 30;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_SECONDARY";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            secondarySkillDefs.Add(mySkillDef);
            base.SetupSecondary();
        }

        public override void SetupUtility()
        {
            return;
            var mySkillDef = ScriptableObject.CreateInstance<HuntressTrackingSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESAssignTarget));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 16;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = SurvivorSkillLocator.utility.skillDef.icon;
            mySkillDef.skillName = TokenPrefix+"_UTILITY";
            mySkillDef.skillNameToken = $"{mySkillDef.skillName}_NAME";
            mySkillDef.skillDescriptionToken = $"{mySkillDef.skillName}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]{};
            utilitySkillDefs.Add(mySkillDef);
            base.SetupUtility();
        }

        public override void SetupSpecial()
        {
            return;
            var mySkillDef = ScriptableObject.CreateInstance<HuntressTrackingSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESAssignTarget));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 10;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
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
