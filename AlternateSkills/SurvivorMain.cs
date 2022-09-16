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

namespace AlternateSkills
{
    public abstract class SurvivorMain
    {
        public abstract string CharacterName { get; }

        public virtual GameObject BodyPrefab { get; set; }
        public virtual BodyIndex BodyIndex { get; set; }
        public virtual SkillLocator SurvivorSkillLocator { get; set; }

        public virtual string ConfigCategory { get; set; }

        public virtual List<SkillDef> primarySkillDefs {get; set; }
        public virtual List<SkillDef> secondarySkillDefs {get; set; }
        public virtual List<SkillDef> utilitySkillDefs {get; set; }
        public virtual List<SkillDef> specialSkillDefs {get; set; }

        public virtual void Init(ConfigFile config)
        {
            SetupDefaults();
            SetupConfig(config);
            SetupAssets();
            SetupLanguage();
            SetupSkills();
            Hooks();
        }

        public void SetupDefaults()
        {
            if (ConfigCategory.IsNullOrWhiteSpace())
                ConfigCategory = CharacterName;

            if (!BodyPrefab)
                BodyPrefab = LegacyResourcesAPI.Load<GameObject>($"prefabs/CharacterBodies/{CharacterName}Body");
            if (BodyIndex == BodyIndex.None)
                BodyIndex = BodyPrefab.GetComponent<CharacterBody>().bodyIndex;
            if (!SurvivorSkillLocator)
                SurvivorSkillLocator = BodyPrefab.GetComponent<SkillLocator>();
        }

        public virtual void Hooks()
        {

        }

        public virtual void SetupConfig(ConfigFile config) { }

        public virtual void SetupLanguage() { }

        public void SetupAssets()
        {

        }


        public void SetupSkills()
        {
            primarySkillDefs = new List<SkillDef>();
            secondarySkillDefs = new List<SkillDef>();
            utilitySkillDefs = new List<SkillDef>();
            specialSkillDefs = new List<SkillDef>();

            SetupPassive();
            SetupPrimary();
            SetupSecondary();
            SetupUtility();
            SetupSpecial();
        }

        public virtual void SetupPassive()
        {
            //Setup skills add to List
            //Then call base
            //
        }

        private void AddSkillToSkillFamily(SkillDef skillDef, SkillFamily skillFamily)
        {
            ContentAddition.AddSkillDef(skillDef);

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        private void AddSkillsToSkillFamily(List<SkillDef> skillDefs, SkillFamily skillFamily)
        {
            foreach (var skillDef in skillDefs)
            {
                AddSkillToSkillFamily(skillDef, skillFamily);
            }
        }

        public virtual void SetupPrimary()
        {
            AddSkillsToSkillFamily(primarySkillDefs, SurvivorSkillLocator.primary.skillFamily);
        }

        public virtual void SetupSecondary() {
            AddSkillsToSkillFamily(secondarySkillDefs, SurvivorSkillLocator.primary.skillFamily); }

        public virtual void SetupUtility() {
            AddSkillsToSkillFamily(utilitySkillDefs, SurvivorSkillLocator.primary.skillFamily); }
        public virtual void SetupSpecial() {
            AddSkillsToSkillFamily(specialSkillDefs, SurvivorSkillLocator.primary.skillFamily); }

    }
}
