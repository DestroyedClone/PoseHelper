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
using RoR2.Projectile;

namespace ROR1AltSkills.Commando
{
    public class CommandoMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/CommandoBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static SkillDef rollSkillDef;


        public static void Init()
        {
            SetupSkills();

            On.EntityStates.Commando.DodgeState.OnEnter += DodgeState_OnEnter;
        }

        private static void DodgeState_OnEnter(On.EntityStates.Commando.DodgeState.orig_OnEnter orig, EntityStates.Commando.DodgeState self)
        {
            orig(self);
            if (self.outer.commonComponents.characterBody?.skillLocator?.utility?.skillDef && self.outer.commonComponents.characterBody.skillLocator.utility.skillDef == rollSkillDef)
            {
                self.outer.commonComponents.characterBody.AddTimedBuff(RoR2Content.Buffs.Immune, self.duration);
            }
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_NAME", "Tactical Dive");
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION", "<style=cIsDamage>Roll forward</style> a small distance. You <style=cIsUtility>cannot be hit</style> while rolling.");

            var oldDef = Resources.Load<SkillDef>("skilldefs/commandobody/CommandoBodyRoll");
            rollSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            rollSkillDef.activationState = oldDef.activationState;
            rollSkillDef.activationStateMachineName = oldDef.activationStateMachineName;
            rollSkillDef.baseMaxStock = 1;
            rollSkillDef.baseRechargeInterval = 4f;
            rollSkillDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            rollSkillDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            rollSkillDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            rollSkillDef.interruptPriority = oldDef.interruptPriority;
            rollSkillDef.isCombatSkill = oldDef.isCombatSkill;
            rollSkillDef.mustKeyPress = oldDef.mustKeyPress;
            rollSkillDef.rechargeStock = 1;
            rollSkillDef.requiredStock = 1;
            rollSkillDef.stockToConsume = 1;
            rollSkillDef.icon = oldDef.icon;
            rollSkillDef.skillDescriptionToken = "DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION";
            rollSkillDef.skillName = "DC_COMMANDO_UTILITY_TACTICALDIVE_NAME";
            rollSkillDef.skillNameToken = rollSkillDef.skillName;

            LoadoutAPI.AddSkillDef(rollSkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = rollSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(rollSkillDef.skillNameToken, false, null)
            };
        }
    }
}
