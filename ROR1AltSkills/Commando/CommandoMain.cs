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


        public static void Init()
        {
            SetupSkills();

            On.EntityStates.Commando.DodgeState.OnEnter += DodgeState_OnEnter;
        }

        private static void DodgeState_OnEnter(On.EntityStates.Commando.DodgeState.orig_OnEnter orig, EntityStates.Commando.DodgeState self)
        {
            orig(self);
            Chat.AddMessage(self.initialSpeedCoefficient.ToString());
            Chat.AddMessage(self.finalSpeedCoefficient.ToString());
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_NAME", "Tactical Dive");
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION", "<style=cIsDamage>Roll forward</style> a small distance. You <style=cIsUtility>cannot be hit</style> while rolling.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(TacticalDive));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 4f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION";
            mySkillDef.skillName = "DC_COMMANDO_UTILITY_TACTICALDIVE_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.utility.skillFamily;

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
