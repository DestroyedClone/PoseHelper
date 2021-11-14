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
    public class AcridMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/CrocoBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        #region passive
        public static DamageAPI.ModdedDamageType OriginalPoisonOnHit;
        public static readonly float OriginalPoisonDamageCoefficient = 0.24f;

        public static DotController.DotIndex OriginalPoisonDot;
        public static CustomBuff OriginalPoisonBuff;
        #endregion
        #region primary
        public static readonly float FesteringWoundsDamageCoefficient = 1.8f;
        public static readonly float FesteringWoundsDPSCoefficient = 0.9f;
        #endregion

        #region utility
        public static GameObject acidPool;
        public static GameObject acidPoolDrop;

        internal static float acidPoolScale = 1f;
        private static readonly float buffWard_to_acidPoolScale_ratio = 5f; //shouldn't be changed
        internal static float CausticSludgeBuffDuration = 3f;

        internal static float CausticSludgeActualScale = acidPoolScale * buffWard_to_acidPoolScale_ratio;

        public static readonly float CausticSludgeLifetime = 12f;
        public static readonly float CausticSludgeDuration = 2f;
        public static readonly float CausticSludgeSlowDuration = 3f;
        public static readonly float CausticSludgeDamageCoefficient = 0.5f;

        public static readonly float CausticSludgeLeapLandDamageCoefficient = 2f;
        #endregion

        internal static void AddPassiveSkill(GameObject targetPrefab, SkillDef skillDef)
        {
            SkillFamily skillFamily = null;
            foreach (var gs in targetPrefab.GetComponents<GenericSkill>())
            {
                skillFamily = gs.skillFamily;
                break;
            }
            if (!skillFamily)
            {
                Debug.LogWarning("Passive skill family not found");
                return;
            }

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }


        public static void Init()
        {
            var a = myCharacter.transform.GetComponentsInChildren<HitBox>();
            foreach (var b in a)
            {
                Debug.Log(b);
            }


            SetupProjectiles();
            SetupSkills();
            SetupBuffs();
            //SetupModdedDamageTypes();
            //SetupModdedDots();
            Hooks();
        }

        private static void SetupSkills()
        {
            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            #region passive
            /*
            LanguageAPI.Add("DC_CROCO_PASSIVE_POISON_NAME", "Poison");
            LanguageAPI.Add("DC_CROCO_PASSIVE_POISON_DESCRIPTION", $"Deals {OriginalPoisonDamageCoefficient * 100f}% damage per second to enemies.");

            var passiveSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            passiveSkillDef.activationState = new SerializableEntityStateType(typeof(OriginalPoison));
            passiveSkillDef.baseMaxStock = 1;
            passiveSkillDef.baseRechargeInterval = 0f;
            passiveSkillDef.beginSkillCooldownOnSkillEnd = true;
            passiveSkillDef.canceledFromSprinting = false;
            passiveSkillDef.fullRestockOnAssign = true;
            passiveSkillDef.interruptPriority = InterruptPriority.Any;
            passiveSkillDef.isCombatSkill = true;
            passiveSkillDef.mustKeyPress = true;
            passiveSkillDef.rechargeStock = 1;
            passiveSkillDef.requiredStock = 1;
            passiveSkillDef.stockToConsume = 1;
            passiveSkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            passiveSkillDef.skillDescriptionToken = "DC_CROCO_PASSIVE_POISON_DESCRIPTION";
            passiveSkillDef.skillName = "DC_CROCO_PASSIVE_POISON_NAME";
            passiveSkillDef.skillNameToken = passiveSkillDef.skillName;
            passiveSkillDef.keywordTokens = new string[]
            {
                OriginalSkillsPlugin.modkeyword
            };

            LoadoutAPI.AddSkillDef(passiveSkillDef);
            AddPassiveSkill(myCharacter, passiveSkillDef);*/
            #endregion

            #region primary
            LanguageAPI.Add("DC_CROCO_PRIMARY_FESTERINGWOUNDS_NAME", "Festering Wounds");
            LanguageAPI.Add("DC_CROCO_PRIMARY_FESTERINGWOUNDS_DESCRIPTION", $"Maul an enemy for <style=cIsDamage>{FesteringWoundsDamageCoefficient * 100f}% damage</style>." +
                $" The target is poisoned for <style=cIsDamage>{FesteringWoundsDPSCoefficient * 100f}% damage per second</style>.");

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


            var skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            #endregion

            LanguageAPI.Add("DC_CROCO_UTILITY_CAUSTICSLUDGE_NAME", "Caustic Sludge");
            LanguageAPI.Add("DC_CROCO_UTILITY_CAUSTICSLUDGE_DESCRIPTION", $"<style=cIsUtility>Leap in the air</style>, and secrete <style=cIsDamage>poisonous sludge</style> for {CausticSludgeDuration} seconds." +
                $" <style=cIsUtility>Speeds up allies,</style> while <style=cIsDamage>slowing and hurting enemies</style> for <style=cIsDamage>{CausticSludgeDamageCoefficient  * 100f}% damage</style>." +
                $" If you are leaping, the impact deals <style=cIsDamage>{CausticSludgeLeapLandDamageCoefficient * 100}% damage</style>.");

            var mySkillDefUtil = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDefUtil.activationState = new SerializableEntityStateType(typeof(Acrid.LeapDropAcid));
            mySkillDefUtil.activationStateMachineName = "Weapon";
            mySkillDefUtil.baseMaxStock = 1;
            mySkillDefUtil.baseRechargeInterval = CausticSludgeLifetime + 3f;
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

            skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDefUtil,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDefUtil.skillNameToken, false, null)
            };
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
                buffWard.buffDuration = CausticSludgeBuffDuration;
                buffWard.expires = false;
                buffWard.floorWard = true;
                buffWard.radius = CausticSludgeActualScale;
                buffWard.requireGrounded = true;
                buffWard.interval *= 0.5f;

                var enemyBuffWard = acidPool.AddComponent<BuffWard>();
                enemyBuffWard.buffDef = RoR2Content.Buffs.Slow60;
                enemyBuffWard.buffDuration = CausticSludgeBuffDuration;
                enemyBuffWard.expires = false;
                enemyBuffWard.floorWard = true;
                enemyBuffWard.radius = CausticSludgeActualScale;
                enemyBuffWard.requireGrounded = true;
                enemyBuffWard.invertTeamFilter = true;

                ProjectileDotZone projectileDotZone = acidPool.GetComponent<ProjectileDotZone>();
                projectileDotZone.damageCoefficient = CausticSludgeDamageCoefficient;
                projectileDotZone.lifetime = CausticSludgeLifetime;
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

        private static void Hooks()
        {
           // On.RoR2.CrocoDamageTypeController.GetDamageType += CrocoDamageTypeController_GetDamageType;
            //On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.HasModdedDamageType(OriginalPoisonOnHit))
            {
                DotController.InflictDot(victim, damageInfo.attacker, OriginalPoisonDot, 2f, 1f);
            }
            orig(self, damageInfo, victim);
        }

        private static DamageType CrocoDamageTypeController_GetDamageType(On.RoR2.CrocoDamageTypeController.orig_GetDamageType orig, CrocoDamageTypeController self)
        {
            if (self.passiveSkillSlot)
            {
                if (self.passiveSkillSlot.skillDef == self.poisonSkillDef)
                {
                    return (DamageType)OriginalPoisonOnHit;
                }
            }
            return orig(self);
        }

        private static void SetupModdedDamageTypes()
        {
            OriginalPoisonOnHit = DamageAPI.ReserveDamageType();
        }

        private static void SetupModdedDots()
        {
            DotController.DotDef dotDef = new DotController.DotDef()
            {
                associatedBuff = OriginalPoisonBuff.BuffDef,
                damageCoefficient = OriginalPoisonDamageCoefficient,
                damageColorIndex = DamageColorIndex.Poison,
                interval = 0.333f
            };

            DotController.DotStack dotStack = new DotController.DotStack()
            {
                
            };

            DotAPI.CustomDotBehaviour customDotBehaviour = new DotAPI.CustomDotBehaviour(ApplyCustomPoison);

            OriginalPoisonDot = DotAPI.RegisterDotDef(dotDef, customDotBehaviour);
        }

        private static void ApplyCustomPoison(DotController self, DotController.DotStack dotStack)
        {

        }

    }
}
