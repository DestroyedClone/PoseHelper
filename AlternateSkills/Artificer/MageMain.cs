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

namespace AlternateSkills.Mage
{
    public class MageMain : SurvivorMain
    {
        public override string CharacterName => "Mage";
        public string TokenPrefix = "DCALTSKILLS_MAGE";

        public override void Hooks()
        {
            base.Hooks();
            
            CharacterBody.onBodyStartGlobal += MageComponents;
        }
        
        public void MageComponents(CharacterBody self)
        {
            if (self.bodyIndex == BodyIndex)
            {
                //self.gameObject.AddComponent<MageAspectShareComponent>().owner = self;
                
                var prefab = RoR2Content.Survivors.Huntress.bodyPrefab;
                var copy = self.gameObject.AddComponent<HuntressTracker>();
            }
        }

        public class MageAspectShareComponent : MonoBehaviour
        {
            public CharacterBody owner;
            public List<CharacterBody> nearbyElites;
            public SphereSearch sphereSearch;
            private float stopwatch = 0;
            private float frequency = 5f;

            public void Start()
            {
                nearbyElites = new List<CharacterBody>();
                sphereSearch = new SphereSearch();
            }

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch < frequency)
                {
                    return;
                }
                stopwatch = 0;
                if (!owner)
                    return;
                sphereSearch.origin = owner.corePosition;
                sphereSearch.radius = 10;
                sphereSearch.RefreshCandidates();
                var hurtboxes = sphereSearch.searchData.GetHurtBoxes();
                if (hurtboxes.Length > 0)
                {

                }
            }

        }

        public class MageCanisterComponent : MonoBehaviour
        {
            public EquipmentDef currentEquipmentDef;
        }

        public override void SetupPrimary()
        {
            if (true) return;
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESShareBuffs));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 0;
            //mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
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
            return;
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESFireTentacle));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 5;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Frozen;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
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
            var mySkillDef = ScriptableObject.CreateInstance<HuntressTrackingSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESShareBuffs));
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
            //mySkillDef.icon = SurvivorSkillLocator.utility.skillDef.icon;
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
            var mySkillDef = ScriptableObject.CreateInstance<HuntressTrackingSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ESDealDebuffDamage));
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
            //mySkillDef.icon = SurvivorSkillLocator.primary.skillDef.icon;
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
