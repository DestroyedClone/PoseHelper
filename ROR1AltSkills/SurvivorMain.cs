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

namespace ROR1AltSkills
{
    public abstract class SurvivorMain
    {
        public abstract string CharacterName { get; }

        public virtual GameObject BodyPrefab { get; set; }
        public virtual BodyIndex BodyIndex { get; set; }
        public virtual SkillLocator SurvivorSkillLocator { get; set; }

        public virtual string ConfigCategory { get; set; }

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
                BodyPrefab = Resources.Load<GameObject>($"prefabs/CharacterBodies/{CharacterName}Body");
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
            SetupPassive();
            SetupPrimary();
            SetupSecondary();
            SetupUtility();
            SetupSpecial();
        }

        public virtual void SetupPassive() { }

        public virtual void SetupPrimary() { }

        public virtual void SetupSecondary() { }

        public virtual void SetupUtility() { }
        public virtual void SetupSpecial() { }

    }
}
