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

namespace ROR1AltSkills.Huntress
{
    public class HuntressMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/HuntressBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static GameObject projectilePrefab;
        public static GameObject projectileBombletsPrefab;

        public static void Init()
        {
            SetupSkills();
            SetupProjectiles();
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("DC_HUNTRESS_SPECIAL_CLUSTERBOMBS_NAME", "Cluster Bombs");
            LanguageAPI.Add("DC_HUNTRESS_SPECIAL_CLUSTERBOMBS_DESCRIPTION", "Fire an <style=cIsDamage>explosive arrow</style> for <style=cIsDamage>320% damage</style>. The arrow drops bomblets that detonate for <style=cIsDamage6x80%.</style>");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(FireClusterBombs));
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
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "DC_HUNTRESS_SPECIAL_CLUSTERBOMBS_DESCRIPTION";
            mySkillDef.skillName = "DC_HUNTRESS_SPECIAL_CLUSTERBOMBS_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.special.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private static void SetupProjectiles()
        {
            //projectileBombletsPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/SyringeProjectile"), "ClusterBombletsArrow");

            //ProjectileAPI.Add(projectileBombletsPrefab);


            projectilePrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/PaladinRocket"), "ClusterBombArrow");
            var projectileImpactExplosion = projectilePrefab.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.lifetime = 8;
            projectileImpactExplosion.lifetimeAfterImpact = 0;
            projectileImpactExplosion.blastRadius = 0;
            projectileImpactExplosion.blastDamageCoefficient = 0;
            projectileImpactExplosion.blastProcCoefficient = 0;
            projectileImpactExplosion.blastAttackerFiltering = AttackerFiltering.Default;
            projectileImpactExplosion.fireChildren = Resources.Load<GameObject>("prefabs/projectiles/SyringeProjectile");
            projectileImpactExplosion.childrenCount = 6;
            projectileImpactExplosion.childrenDamageCoefficient = 0.8f;
            projectileImpactExplosion.minAngleOffset = new Vector3(-2, -2, -2);
            projectileImpactExplosion.maxAngleOffset = new Vector3(2, 2, 2);
            projectileImpactExplosion.useLocalSpaceForChildren = false;

            ProjectileAPI.Add(projectilePrefab);
        }
    }
}
