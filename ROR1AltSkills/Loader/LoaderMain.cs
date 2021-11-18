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
using RoR2.Orbs;

namespace ROR1AltSkills.Loader
{
    public class LoaderMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/LoaderBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static SteppedSkillDef KnuckleBoomSkillDef;
        public static SkillDef DebrisShieldSkillDef;

        public static ConfigEntry<bool> DebrisShieldAffectsDrones;
        public static ConfigEntry<DebrisShieldMode> DebrisShieldSelectedMode;
        public static ConfigEntry<float> DebrisShieldDuration;
        public static ConfigEntry<float> DebrisShieldCooldown;

        public static CustomBuff DebrisShieldBarrierBuff;
        public static CustomBuff PylonPoweredBuff;

        private static int pylonPowerMaxBounces = 3;
        private static float pylonPowerRange = 20;
        private static float pylonPowerDamageCoefficient = 0.3f;

        public enum DebrisShieldMode
        {
            Immunity,
            Shield,
            Barrier
        }

        public static GameObject ConduitPrefab;

        public static void Init(ConfigFile config)
        {
            SetupConfig(config);

            ModifyLoader();

            SetupSkills();

            SetupBuffs();

            //SetupPrefabs();

            Hooks();
        }

        public static void ModifyLoader()
        {
            var loaderPrefab = RoR2.RoR2Content.Survivors.Loader.bodyPrefab;
            var stateMachine = loaderPrefab.AddComponent<EntityStateMachine>();
            stateMachine.customName = "DebrisShield";
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
        }

        public static void SetupConfig(ConfigFile config)
        {
            string sectionName = "SURVIVOR: LOADER";
            DebrisShieldAffectsDrones = config.Bind(sectionName, "Debris Shield Affects Your Drones", true, "If true, drones owned by the player will be given the buff too.");
            DebrisShieldSelectedMode = config.Bind(sectionName, "Debris Shield Type", DebrisShieldMode.Shield, "Sets the type of shielding provided by the skill." +
                "\nImmunity - Actually what the original skill provided in ROR1" +
                "\nShield - Provides 100% of your health as shield. Dissipates on skill end." +
                "\nBarrier - Provides 100% of your health as barrier. Dissipates on skill end.");
            DebrisShieldDuration = config.Bind(sectionName, "Debris Shield Duration", 3f, "The duration in seconds of how long the buff lasts for.");
            DebrisShieldCooldown = config.Bind(sectionName, "Debris Shield Cooldown", 5f, "The duration in seconds for the cooldown.");
        }

        public static void SetupPrefabs()
        {
            ConduitPrefab = new GameObject();
        }

        private static void SetupSkills()
        {
            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            #region primary
            /*
            LanguageAPI.Add("DC_LOADER_PRIMARY_KNUCKLEBOOM_NAME", "Knuckleboom");
            LanguageAPI.Add("DC_LOADER_PRIMARY_KNUCKLEBOOM_DESCRIPTION", "Batter nearby enemies for <style=cIsDamage>120%</style>. Every third hit deals <style=cIsDamage>240% and knocks up enemies</style>.");

            var oldDef = Resources.Load<SteppedSkillDef>("skilldefs/loaderbody/SwingFist");
            KnuckleBoomSkillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();
            KnuckleBoomSkillDef.activationState = new SerializableEntityStateType(typeof(SwingComboFistAlt));
            KnuckleBoomSkillDef.activationStateMachineName = oldDef.activationStateMachineName;
            KnuckleBoomSkillDef.baseMaxStock = oldDef.baseMaxStock;
            KnuckleBoomSkillDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            KnuckleBoomSkillDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            KnuckleBoomSkillDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            KnuckleBoomSkillDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            KnuckleBoomSkillDef.interruptPriority = oldDef.interruptPriority;
            KnuckleBoomSkillDef.isCombatSkill = oldDef.isCombatSkill;
            KnuckleBoomSkillDef.mustKeyPress = oldDef.mustKeyPress;
            KnuckleBoomSkillDef.rechargeStock = oldDef.rechargeStock;
            KnuckleBoomSkillDef.requiredStock = oldDef.requiredStock;
            KnuckleBoomSkillDef.stockToConsume = oldDef.stockToConsume;
            KnuckleBoomSkillDef.icon = oldDef.icon;
            KnuckleBoomSkillDef.skillDescriptionToken = "DC_LOADER_PRIMARY_KNUCKLEBOOM_DESCRIPTION";
            KnuckleBoomSkillDef.skillName = "DC_LOADER_PRIMARY_KNUCKLEBOOM_NAME";
            KnuckleBoomSkillDef.skillNameToken = KnuckleBoomSkillDef.skillName;
            KnuckleBoomSkillDef.stepCount = 3;
            KnuckleBoomSkillDef.resetStepsOnIdle = oldDef.resetStepsOnIdle;

            LoadoutAPI.AddSkillDef(KnuckleBoomSkillDef);


            var skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = KnuckleBoomSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(KnuckleBoomSkillDef.skillNameToken, false, null)
            };
            */
            #endregion

            LanguageAPI.Add("DC_LOADER_SECONDARY_SHIELD_NAME", "Debris Shield");
            string desc = (DebrisShieldAffectsDrones.Value ? "You and your drones gain" : "Gain");
            desc += " <style=cIsHealing>100% health</style> as ";
            switch (DebrisShieldSelectedMode.Value)
            {
                case DebrisShieldMode.Immunity:
                    desc += $"<style=cIsDamage>damage immunity";
                    break;
                case DebrisShieldMode.Barrier:
                    desc += $"<style=cIsDamage>barrier";
                    break;
                case DebrisShieldMode.Shield:
                    desc += $"<style=cIsUtility>shield";
                    break;
            }
            desc += $"</style> and become <style=cIsUtility>electrified</style>, causing your attacks to <style=cIsUtility>zap up to {pylonPowerMaxBounces} times</style> within <style=cIsDamage>{pylonPowerRange}m</style> for <style=cIsDamage>{pylonPowerDamageCoefficient * 100f}% damage</style>.";
            LanguageAPI.Add("DC_LOADER_SECONDARY_SHIELD_DESCRIPTION", desc);

            DebrisShieldSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            DebrisShieldSkillDef.activationState = new SerializableEntityStateType(typeof(ActivateShield));
            DebrisShieldSkillDef.activationStateMachineName = "DebrisShield";
            DebrisShieldSkillDef.baseMaxStock = 1;
            DebrisShieldSkillDef.baseRechargeInterval = DebrisShieldCooldown.Value;
            DebrisShieldSkillDef.cancelSprintingOnActivation = false;
            DebrisShieldSkillDef.beginSkillCooldownOnSkillEnd = false;
            DebrisShieldSkillDef.canceledFromSprinting = false;
            DebrisShieldSkillDef.fullRestockOnAssign = true;
            DebrisShieldSkillDef.interruptPriority = InterruptPriority.Any;
            DebrisShieldSkillDef.isCombatSkill = false;
            DebrisShieldSkillDef.mustKeyPress = false;
            DebrisShieldSkillDef.rechargeStock = 1;
            DebrisShieldSkillDef.requiredStock = 1;
            DebrisShieldSkillDef.stockToConsume = 1;
            DebrisShieldSkillDef.icon = RoR2Content.Items.PersonalShield.pickupIconSprite;
            DebrisShieldSkillDef.skillDescriptionToken = "DC_LOADER_SECONDARY_SHIELD_DESCRIPTION";
            DebrisShieldSkillDef.skillName = "DC_LOADER_SECONDARY_SHIELD_NAME";
            DebrisShieldSkillDef.skillNameToken = DebrisShieldSkillDef.skillName;
            DebrisShieldSkillDef.keywordTokens = new string[]
            {
                OriginalSkillsPlugin.modkeyword,
            };

            LoadoutAPI.AddSkillDef(DebrisShieldSkillDef);

            var skillFamily = skillLocator.secondary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = DebrisShieldSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(DebrisShieldSkillDef.skillNameToken, false, null)
            };

            //not a typo
            //this is to give the option of keeping the respective skill slot
            skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = DebrisShieldSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(DebrisShieldSkillDef.skillNameToken, false, null)
            };
            #region utility
            /* this is the most demonic skillstate ive seen

            LanguageAPI.Add("DC_LOADER_UTILITY_HOOK_NAME", "Hydraulic Gauntlet");
            LanguageAPI.Add("DC_LOADER_UTILITY_HOOK_DESCRIPTION", "Fire your gauntlet forward. If it hits an <style=cIsDamage>enemy or wall you pull yourself</style> towards them, <style=cIsDamage>stunning and hurting enemies for 210%.</style>");

            UtilitySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            UtilitySkillDef.activationState = new SerializableEntityStateType(typeof(FireStraightHook));
            UtilitySkillDef.activationStateMachineName = "Weapon";
            UtilitySkillDef.baseMaxStock = 1;
            UtilitySkillDef.baseRechargeInterval = 3f;
            UtilitySkillDef.beginSkillCooldownOnSkillEnd = true;
            UtilitySkillDef.canceledFromSprinting = false;
            UtilitySkillDef.fullRestockOnAssign = true;
            UtilitySkillDef.interruptPriority = InterruptPriority.Any;
            UtilitySkillDef.isCombatSkill = false;
            UtilitySkillDef.mustKeyPress = false;
            UtilitySkillDef.rechargeStock = 1;
            UtilitySkillDef.requiredStock = 1;
            UtilitySkillDef.stockToConsume = 1;
            UtilitySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            UtilitySkillDef.skillDescriptionToken = "DC_LOADER_UTILITY_HOOK_DESCRIPTION";
            UtilitySkillDef.skillName = "DC_LOADER_UTILITY_HOOK_NAME";
            UtilitySkillDef.skillNameToken = UtilitySkillDef.skillName;

            LoadoutAPI.AddSkillDef(UtilitySkillDef);

            skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = UtilitySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(UtilitySkillDef.skillNameToken, false, null)
            };*/
            #endregion

            #region special

            /*
            LanguageAPI.Add("DC_LOADER_SPECIAL_CONDUIT_NAME", "Place Conduit");
            LanguageAPI.Add("DC_LOADER_SPECIAL_CONDUIT_DESCRIPTION", "Place two conduits to fuck off.");

            var mySkillDefSpecial = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDefSpecial.activationState = new SerializableEntityStateType(typeof(PlaceConduit1));
            mySkillDefSpecial.activationStateMachineName = "Pylon";
            mySkillDefSpecial.baseMaxStock = 1;
            mySkillDefSpecial.baseRechargeInterval = 5f;
            mySkillDefSpecial.cancelSprintingOnActivation = false;
            mySkillDefSpecial.beginSkillCooldownOnSkillEnd = true;
            mySkillDefSpecial.canceledFromSprinting = false;
            mySkillDefSpecial.fullRestockOnAssign = true;
            mySkillDefSpecial.interruptPriority = InterruptPriority.Any;
            mySkillDefSpecial.isCombatSkill = false;
            mySkillDefSpecial.mustKeyPress = false;
            mySkillDefSpecial.rechargeStock = 1;
            mySkillDefSpecial.requiredStock = 1;
            mySkillDefSpecial.stockToConsume = 1;
            mySkillDefSpecial.icon = RoR2Content.Items.ShockNearby.pickupIconSprite;
            mySkillDefSpecial.skillDescriptionToken = "DC_LOADER_SPECIAL_CONDUIT_DESCRIPTION";
            mySkillDefSpecial.skillName = "DC_LOADER_SPECIAL_CONDUIT_NAME";
            mySkillDefSpecial.skillNameToken = mySkillDefSpecial.skillName;



            skillFamily = skillLocator.special.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDefSpecial,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDefSpecial.skillNameToken, false, null)
            };
            */
            #endregion
        }

        private static void SetupBuffs()
        {
            if (DebrisShieldSelectedMode.Value == DebrisShieldMode.Barrier)
            {
                DebrisShieldBarrierBuff = new CustomBuff("Debris Shield (Barrier)",
                    RoR2Content.Buffs.EngiShield.iconSprite,
                    Color.yellow,
                    false,
                    false);
                BuffAPI.Add(DebrisShieldBarrierBuff);
            }

            PylonPoweredBuff = new CustomBuff("Pylon Powered (Debris Shield)",
                RoR2Content.Buffs.FullCrit.iconSprite,
                Color.yellow,
                false,
                false);
            BuffAPI.Add(PylonPoweredBuff);
        }

        private static void Hooks()
        {
            //On.EntityStates.Loader.SwingComboFist.PlayAnimation += SwingComboFist_PlayAnimation;

            //On.EntityStates.BasicMeleeAttack.OnEnter += BasicMeleeAttack_OnEnter;
            //On.EntityStates.BasicMeleeAttack.PlayAnimation += BasicMeleeAttack_PlayAnimation;

            if (LoaderMain.DebrisShieldSelectedMode.Value == DebrisShieldMode.Barrier)
            {
                On.RoR2.HealthComponent.AddBarrier += HealthComponent_AddBarrier;
                On.RoR2.HealthComponent.Awake += HealthComponent_Awake;
                On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
                On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            }

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>())
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody.HasBuff(PylonPoweredBuff.BuffDef) && !damageInfo.procChainMask.HasProc(ProcType.LoaderLightning))
                {
                    float damageValue = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, pylonPowerDamageCoefficient);
                    LightningOrb lightningOrb = new LightningOrb();
                    lightningOrb.origin = damageInfo.position;
                    lightningOrb.damageValue = damageValue;
                    lightningOrb.isCrit = damageInfo.crit;
                    lightningOrb.bouncesRemaining = pylonPowerMaxBounces;
                    lightningOrb.teamIndex = attackerBody.teamComponent ? attackerBody.teamComponent.teamIndex : TeamIndex.None;
                    lightningOrb.attacker = damageInfo.attacker;
                    lightningOrb.bouncedObjects = new List<HealthComponent>
                            {
                                victim.GetComponent<HealthComponent>()
                            };
                    lightningOrb.procChainMask = damageInfo.procChainMask;
                    lightningOrb.procChainMask.AddProc(ProcType.LoaderLightning);
                    lightningOrb.procCoefficient = 0f;
                    lightningOrb.lightningType = LightningOrb.LightningType.Loader;
                    lightningOrb.damageColorIndex = DamageColorIndex.Item;
                    lightningOrb.range = pylonPowerRange;
                    HurtBox hurtBox = lightningOrb.PickNextTarget(damageInfo.position);
                    if (hurtBox)
                    {
                        lightningOrb.target = hurtBox;
                        OrbManager.instance.AddOrb(lightningOrb);
                    }
                }
            }
        }

        #region DebrisShield Barrier Type
        private static void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            orig(self, buffDef, duration);
            if (buffDef == DebrisShieldBarrierBuff.BuffDef)
            {
                var comp = self.GetComponent<TrackDebrisShield>();
                if (!comp)
                    comp = self.gameObject.AddComponent<TrackDebrisShield>();
                comp.OnBuffApplied();
            }
        }

        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == DebrisShieldBarrierBuff.BuffDef)
            {
                self.GetComponent<TrackDebrisShield>()?.OnBuffLost();
            }
        }

        private static void HealthComponent_Awake(On.RoR2.HealthComponent.orig_Awake orig, HealthComponent self)
        {
            orig(self);
            if (!self.GetComponent<TrackDebrisShield>())
                self.gameObject.AddComponent<TrackDebrisShield>().healthComponent = self;
        }

        private static void HealthComponent_AddBarrier(On.RoR2.HealthComponent.orig_AddBarrier orig, HealthComponent self, float value)
        {
            orig(self, value);
            if (value > 0)
                self.GetComponent<TrackDebrisShield>()?.OnAddBarrier(value);
        }
        #endregion

        private static bool IsSkillDef(EntityState entityState, SkillDef skillDef)
        {
            return (entityState.outer.commonComponents.characterBody?.skillLocator?.utility?.skillDef && entityState.outer.commonComponents.characterBody.skillLocator.utility.skillDef == skillDef);
        }

        private static void BasicMeleeAttack_PlayAnimation(On.EntityStates.BasicMeleeAttack.orig_PlayAnimation orig, BasicMeleeAttack self)
        {
            var isSkillDef = IsSkillDef(self, KnuckleBoomSkillDef);
            if (!isSkillDef)
            {
                orig(self);
                return;
            }
            var cock = self as EntityStates.Loader.SwingComboFist;
            string animationStateName = "";
            float duration = Mathf.Max(self.duration, 0.2f);
            switch (cock.gauntlet)
            {
                case 0:
                    animationStateName = "SwingFistRight";
                    break;

                case 1:
                    animationStateName = "SwingFistLeft";
                    break;

                case 2:
                    animationStateName = "BigPunch";
                    //base.PlayAnimation("FullBody, Override", "BigPunch", "BigPunch.playbackRate", duration); //BigPunch
                    break;
            }

            self.PlayCrossfade("Gesture, Additive", animationStateName, "SwingFist.playbackRate", duration, 0.1f);
            self.PlayCrossfade("Gesture, Override", animationStateName, "SwingFist.playbackRate", duration, 0.1f);
        }

        private static void BasicMeleeAttack_OnEnter(On.EntityStates.BasicMeleeAttack.orig_OnEnter orig, BasicMeleeAttack self)
        {
            var cock = self as EntityStates.Loader.SwingComboFist;
            if (cock.gauntlet == 2)
            {
                cock.damageCoefficient *= 2f;
                cock.overlapAttack.pushAwayForce = 35f;
                cock.overlapAttack.forceVector = Vector3.up;
            }
            orig(cock);
        }


        public class TrackDebrisShield : MonoBehaviour
        {
            public float barrierToRemove = 0f;
            public HealthComponent healthComponent;

            private bool acceptContributions = true;

            public void OnAddBarrier(float amount)
            {
                if (acceptContributions)
                {
                    //Chat.AddMessage($"OnAddBarrier: {barrierToRemove} - {amount} = {barrierToRemove-amount}");
                    barrierToRemove -= amount;
                }
            }

            public void OnBuffLost()
            {
                if (barrierToRemove > 0)
                {
                    //Chat.AddMessage($"OnBuffLost: Expectation of resulting barrier: {healthComponent.Networkbarrier - barrierToRemove}");
                    healthComponent.Networkbarrier = Mathf.Max(healthComponent.Networkbarrier - barrierToRemove, 0f);
                    barrierToRemove = 0f;
                }
            }

            public void OnBuffApplied()
            {
                var barrierLostPerSecond = healthComponent.body.barrierDecayRate;
                var barrierLostAfterXSeconds = barrierLostPerSecond * DebrisShieldDuration.Value;

                var barrierToGive = healthComponent.fullBarrier;// * DebrisShieldPercentage.Value;

                barrierToRemove = barrierToGive - barrierLostAfterXSeconds;
                //Chat.AddMessage("Expected Barrier to Remove: " + barrierToRemove);
                acceptContributions = false;
                healthComponent.AddBarrier(barrierToGive);
                acceptContributions = true;
                //healthComponent.Networkbarrier = healthComponent.fullBarrier;
            }
        }

        #region i dont want to look at this so ill collapse it
        /*
        private static void SwingComboFist_PlayAnimation(On.EntityStates.Loader.SwingComboFist.orig_PlayAnimation orig, EntityStates.Loader.SwingComboFist self)
        {
            string[] a = new string[]
            {
                $"hitBoxGroupName = \"{self.hitBoxGroupName}\";",
                $"hitEffectPrefab = {self.hitEffectPrefab.name}; //Resources.Load",
                $"procCoefficient = {self.procCoefficient};",
                $"pushAwayForce = {self.pushAwayForce};",
                $"forceVector = new Vector3{self.forceVector};",
                $"hitPauseDuration = {self.hitPauseDuration};",
                $"swingEffectPrefab = \"{self.swingEffectPrefab}\"; //Resources.Load",
                $"swingEffectMuzzleString = \"{self.swingEffectMuzzleString}\";",
                $"mecanimHitboxActiveParameter = \"{self.mecanimHitboxActiveParameter}\";",
                $"shorthopVelocityFromHit = {self.shorthopVelocityFromHit};",
                $"beginStateSoundString = \"{self.beginStateSoundString}\";",
                $"beginSwingSoundString = \"{self.beginSwingSoundString}\";",
                $"forceForwardVelocity = {self.forceForwardVelocity.ToString().ToLower()};",
                $"forwardVelocityCurve = null;",
                $"scaleHitPauseDurationAndVelocityWithAttackSpeed = {self.scaleHitPauseDurationAndVelocityWithAttackSpeed.ToString().ToLower()};",
                $"ignoreAttackSpeed = {self.ignoreAttackSpeed.ToString().ToLower()};"
            };

            //foreach (var b in a) { Chat.AddMessage(b); };

            var curve = self.forwardVelocityCurve;
            string[] aa = new string[]
            {
                $"Keys Length: {curve.keys.Length}",
                $"postWrapMode = {curve.postWrapMode}",
                $"preWrapMode = {curve.preWrapMode}",
            };
            Chat.AddMessage($"Keys Length: {curve.keys.Length}");
            Chat.AddMessage($"postWrapMode = {curve.postWrapMode}");
            Chat.AddMessage($"preWrapMode = {curve.preWrapMode}");
            foreach (var keyframe in curve.keys)
            {
                Chat.AddMessage($"value = {keyframe.value}");
                Chat.AddMessage($"time = {keyframe.time}");
                //Chat.AddMessage($"tangentMode = {keyframe.tangentMode} //obsolete");
                Chat.AddMessage($"inTangent = {keyframe.inTangent}");
                Chat.AddMessage($"outTangent = {keyframe.outTangent}");
                Chat.AddMessage($"weightedMode = {keyframe.weightedMode}");
                Chat.AddMessage($"inWeight = {keyframe.inWeight}");
                Chat.AddMessage($"outWeight = {keyframe.outWeight}");
            }

            orig(self);
        }
        */
        #endregion
    }
}
