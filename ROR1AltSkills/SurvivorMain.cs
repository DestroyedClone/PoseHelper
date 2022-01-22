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

        public abstract string ConfigCategory { get; }

        public virtual void Init(ConfigFile config)
        {
            SetupConfig(config);
            SetupAssets();
            SetupLanguage();
            SetupSkills();
        }

        public abstract void SetupConfig(ConfigFile config);

        public abstract void SetupLanguage();

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
