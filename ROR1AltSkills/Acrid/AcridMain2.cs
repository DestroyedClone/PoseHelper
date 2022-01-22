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

namespace ROR1AltSkills.Acrid
{
    public class AcridMain2 : SurvivorMain
    {
        public override string CharacterName => "Croco";

        #region passive
        public static DamageAPI.ModdedDamageType OriginalPoisonOnHit;
        public static float cfgOriginalPoisonDamageCoefficient = 0.24f;

        public static DotController.DotIndex OriginalPoisonDot;
        public static CustomBuff OriginalPoisonBuff;
        #endregion
        #region primary
        public static float cfgFesteringWoundsDamageCoefficient = 1.8f;
        public static float cfgFesteringWoundsDPSCoefficient = 0.9f;
        #endregion
        #region utility
        public static GameObject acidPool;
        public static GameObject acidPoolDrop;

        internal static float acidPoolScale = 1f;
        private static float buffWard_to_acidPoolScale_ratio = 5f; //shouldn't be changed
        internal static float cfgCausticSludgeBuffDuration = 3f;

        internal static float cfgCausticSludgeActualScale = acidPoolScale * buffWard_to_acidPoolScale_ratio;

        public static float cfgCausticSludgeLifetime = 12f;
        public static float cfgCausticSludgeDuration = 2f;
        public static float cfgCausticSludgeSlowDuration = 3f;
        public static float cfgCausticSludgeDamageCoefficient = 0.5f;

        public static float cfgCausticSludgeLeapLandDamageCoefficient = 2f;

        #endregion
        public override string ConfigCategory => "Acrid";

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            SetupProjectiles();
            SetupBuffs();
        }

        private static void SetupProjectiles()
        {
            SetupAcidPool();
            SetupAcidPoolDrop();
            void SetupAcidPool()
            {
                acidPool = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/CrocoLeapAcid"), "CrocoSpeedAcid");
                acidPool.transform.localScale *= acidPoolScale;
                var buffWard = acidPool.AddComponent<BuffWard>();
                buffWard.buffDef = RoR2Content.Buffs.CloakSpeed;
                buffWard.buffDuration = cfgCausticSludgeBuffDuration;
                buffWard.expires = false;
                buffWard.floorWard = true;
                buffWard.radius = cfgCausticSludgeActualScale;
                buffWard.requireGrounded = true;
                buffWard.interval *= 0.5f;

                var enemyBuffWard = acidPool.AddComponent<BuffWard>();
                enemyBuffWard.buffDef = RoR2Content.Buffs.Slow60;
                enemyBuffWard.buffDuration = cfgCausticSludgeBuffDuration;
                enemyBuffWard.expires = false;
                enemyBuffWard.floorWard = true;
                enemyBuffWard.radius = cfgCausticSludgeActualScale;
                enemyBuffWard.requireGrounded = true;
                enemyBuffWard.invertTeamFilter = true;

                ProjectileDotZone projectileDotZone = acidPool.GetComponent<ProjectileDotZone>();
                projectileDotZone.damageCoefficient = cfgCausticSludgeDamageCoefficient;
                projectileDotZone.lifetime = cfgCausticSludgeLifetime;
                projectileDotZone.overlapProcCoefficient = 0f;

                PoisonSplatController poisonSplatController = acidPool.AddComponent<PoisonSplatController>();
                poisonSplatController.destroyOnTimer = acidPool.GetComponent<DestroyOnTimer>();
                poisonSplatController.projectileDotZone = projectileDotZone;
                poisonSplatController.projectileController = acidPool.GetComponent<ProjectileController>();

                ProjectileAPI.Add(acidPool);

                LeapDropAcid.groundedAcid = acidPool;
            }

            void SetupAcidPoolDrop()
            {
                Debug.Log("Setting up AcidPoolDrop");
                acidPoolDrop = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/SporeGrenadeProjectile"), "CrocoSpeedAcidDrop");

                acidPoolDrop.GetComponent<ProjectileSimple>().desiredForwardSpeed = 0;

                var atos = acidPoolDrop.AddComponent<ApplyTorqueOnStart>();
                atos.localTorque = Vector3.down * 3f;
                atos.randomize = false;

                var projectileImpactExplosion = acidPoolDrop.GetComponent<ProjectileImpactExplosion>();
                projectileImpactExplosion.blastDamageCoefficient = 0f;
                projectileImpactExplosion.childrenProjectilePrefab = acidPool;
                projectileImpactExplosion.impactEffect = null;
                projectileImpactExplosion.destroyOnEnemy = false;

                acidPoolDrop.AddComponent<PoisonFallController>();

                ProjectileAPI.Add(acidPoolDrop);
            }
            LeapDropAcid.projectilePrefab = acidPoolDrop;
        }

        private static void SetupBuffs()
        {
            var buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.buffColor = Color.green;
            buffDef.canStack = true;
            buffDef.isDebuff = true;
            buffDef.iconSprite = RoR2Content.Buffs.Poisoned.iconSprite;

            OriginalPoisonBuff = new CustomBuff(buffDef);
        }

        public override void SetupConfig(ConfigFile config)
        {
            cfgOriginalPoisonDamageCoefficient = config.Bind(ConfigCategory, "Original Poison Damage Coefficient", 0.24f, "The damage coefficient of the Original Poison.").Value;
            cfgFesteringWoundsDamageCoefficient = config.Bind(ConfigCategory, "Festering Wounds Damage Coefficient", 1.8f, "").Value;
            cfgFesteringWoundsDPSCoefficient = config.Bind(ConfigCategory, "Festering Wounds DPS Coefficient", 0.9f, "").Value;
            cfgCausticSludgeBuffDuration = config.Bind(ConfigCategory, "Caustic Sludge Buff Duration", 3f, "").Value;
            cfgCausticSludgeLifetime = config.Bind(ConfigCategory, "Caustic Sludge Lifetime", 12f, "").Value;
            cfgCausticSludgeDuration = config.Bind(ConfigCategory, "Caustic Sludge Duration", 2f, "").Value;
            cfgCausticSludgeSlowDuration = config.Bind(ConfigCategory, "Caustic Sludge Slow Duration", 3f, "").Value;
            cfgCausticSludgeDamageCoefficient = config.Bind(ConfigCategory, "Caustic Sludge Damage Coefficient", 0.5f, "").Value;
            cfgCausticSludgeLeapLandDamageCoefficient = config.Bind(ConfigCategory, "Caustic Sludge Leap Land Damage Coefficient", 2f, "").Value;
        }

        public override void SetupLanguage()
        {
        }

        public override void SetupPrimary()
        {
            LanguageAPI.Add("DC_CROCO_PRIMARY_FESTERINGWOUNDS_NAME", "Festering Wounds");
            LanguageAPI.Add("DC_CROCO_PRIMARY_FESTERINGWOUNDS_DESCRIPTION", $"Maul an enemy for <style=cIsDamage>{cfgFesteringWoundsDamageCoefficient * 100f}% damage</style>." +
                $" The target is poisoned for <style=cIsDamage>{cfgFesteringWoundsDPSCoefficient * 100f}% damage per second</style>.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(PoisonBite));
            mySkillDef.activationStateMachineName = "Mouth";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "DC_CROCO_PRIMARY_FESTERINGWOUNDS_DESCRIPTION";
            mySkillDef.skillName = "DC_CROCO_PRIMARY_FESTERINGWOUNDS_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[]
            {
                OriginalSkillsPlugin.modkeyword,
                "KEYWORD_POISON",
                "KEYWORD_SLAYER",
                "KEYWORD_RAPID_REGEN",
            };

            LoadoutAPI.AddSkillDef(mySkillDef);


            var skillFamily = SurvivorSkillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        public override void SetupSecondary()
        {
        }

        public override void SetupUtility()
        {
            LanguageAPI.Add("DC_CROCO_UTILITY_CAUSTICSLUDGE_NAME", "Caustic Sludge");
            LanguageAPI.Add("DC_CROCO_UTILITY_CAUSTICSLUDGE_DESCRIPTION", $"<style=cIsUtility>Leap in the air</style>, and secrete <style=cIsDamage>poisonous sludge</style> for {cfgCausticSludgeDuration} seconds." +
                $" <style=cIsUtility>Speeds up allies,</style> while <style=cIsDamage>slowing and hurting enemies</style> for <style=cIsDamage>{cfgCausticSludgeDamageCoefficient * 100f}% damage</style>." +
                $" If you are leaping, the impact deals <style=cIsDamage>{cfgCausticSludgeLeapLandDamageCoefficient * 100}% damage</style>.");

            var mySkillDefUtil = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDefUtil.activationState = new SerializableEntityStateType(typeof(Acrid.LeapDropAcid));
            mySkillDefUtil.activationStateMachineName = "Weapon";
            mySkillDefUtil.baseMaxStock = 1;
            mySkillDefUtil.baseRechargeInterval = cfgCausticSludgeLifetime + 3f;
            mySkillDefUtil.beginSkillCooldownOnSkillEnd = true;
            mySkillDefUtil.canceledFromSprinting = false;
            mySkillDefUtil.fullRestockOnAssign = true;
            mySkillDefUtil.interruptPriority = InterruptPriority.Frozen;
            mySkillDefUtil.isCombatSkill = false;
            mySkillDefUtil.mustKeyPress = true;
            mySkillDefUtil.rechargeStock = 1;
            mySkillDefUtil.requiredStock = 1;
            mySkillDefUtil.stockToConsume = 1;
            mySkillDefUtil.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDefUtil.skillDescriptionToken = "DC_CROCO_UTILITY_CAUSTICSLUDGE_DESCRIPTION";
            mySkillDefUtil.skillName = "DC_CROCO_UTILITY_CAUSTICSLUDGE_NAME";
            mySkillDefUtil.skillNameToken = mySkillDefUtil.skillName;
            mySkillDefUtil.keywordTokens = new string[]
            {
                OriginalSkillsPlugin.modkeyword,
                "KEYWORD_POISON",
            };

            LoadoutAPI.AddSkillDef(mySkillDefUtil);

            var skillFamily = SurvivorSkillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDefUtil,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDefUtil.skillNameToken, false, null)
            };
        }

        public override void SetupSpecial()
        {
        }
    }
}
