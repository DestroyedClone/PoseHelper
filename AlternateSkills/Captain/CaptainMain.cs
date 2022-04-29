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
using static R2API.RecalculateStatsAPI ;

namespace AlternateSkills.Captain
{
    public class CaptainMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/CaptainBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static GameObject accelerantThrownProjectile;
        public static GameObject accelerantSplashProjectile;

        public static CustomBuff accelerantBuff;

        public static void Init()
        {
            SetupProjectiles();
            SetupBuffs();
            SetupSkills();
            GetStatCoefficients += CaptainMain_GetStatCoefficients;

            //On.RoR2.EntityStateCatalog.Init += EntityStateCatalog_Init;
        }

        private static void CaptainMain_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(accelerantBuff.BuffDef))
            {
                args.moveSpeedMultAdd += 9f;
            }
        }

        public static void SetupBuffs()
        {
            accelerantBuff = new CustomBuff("Accelerant",
                RoR2Content.Buffs.CloakSpeed.iconSprite,
                Color.blue,
                false,
                false);
            BuffAPI.Add(accelerantBuff);
        }

        private static void SetupProjectiles()
        {
            accelerantSplashProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/SporeGrenadeProjectileDotZone"), "ThrownAccelerantDotZone");
            accelerantSplashProjectile.GetComponentInChildren<BuffWard>().buffDef = accelerantBuff.BuffDef;

            accelerantThrownProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/SporeGrenadeProjectile"), "ThrownAccelerant");
            var pie = accelerantThrownProjectile.GetComponent<ProjectileImpactExplosion>();
            pie.childrenProjectilePrefab = accelerantSplashProjectile;

            ProjectileAPI.Add(accelerantSplashProjectile);
            ProjectileAPI.Add(accelerantThrownProjectile);
        }

        private static void EntityStateCatalog_Init(On.RoR2.EntityStateCatalog.orig_Init orig)
        {
            Debug.Log($"ImpBossMonster");
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("CAPTAIN_PRIMARY_LUCKVOLVER_NAME", "Lucky Slug");
            LanguageAPI.Add("CAPTAIN_PRIMARY_LUCKVOLVER_DESC", "Fire off a slug for <style=cIsDamage>140% damage</style> with a <style=cIsUtility>50% increased chance</style> to proc items.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            //mySkillDef.activationState = new SerializableEntityStateType(typeof(Captain.SetupNukeAlt));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 20f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "CAPTAINSCEPTER_DESCRIPTION";
            mySkillDef.skillName = "CAPTAINSCEPTER_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.dontAllowPastMaxStocks = false;
            mySkillDef.forceSprintDuringState = false;
            mySkillDef.keywordTokens = new string[] { };
            mySkillDef.resetCooldownTimerOnUse = false;

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
