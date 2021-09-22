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

namespace ROR1AltSkills.Loader
{
    public class LoaderMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/LoaderBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static SkillDef UtilitySkillDef;

        public static GameObject StraightHookProjectile;

        public static void Init()
        {
            SetupSkills();

            SetupBuffs();

            SetupProjectiles();

            Hooks();
        }

        public static void SetupProjectiles()
        {
            var hook = Resources.Load<GameObject>("prefabs/projectiles/LoaderYankHook");
            StraightHookProjectile = PrefabAPI.InstantiateClone(hook, "LoaderStraightHook");
            var grappleController = StraightHookProjectile.GetComponent<RoR2.Projectile.ProjectileGrappleController>();
            grappleController.yankMassLimit = 0;


            ProjectileAPI.Add(StraightHookProjectile);
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("DC_LOADER_PRIMARY_KNUCKLEBOOM_NAME", "Knuckleboom");
            LanguageAPI.Add("DC_LOADER_PRIMARY_KNUCKLEBOOM_DESCRIPTION", "Batter nearby enemies for <style=cIsDamage>120%</style>. Every third hit deals <style=cIsDamage>240% and knocks up enemies</style>.");

            var oldDef = Resources.Load<SteppedSkillDef>("skilldefs/loaderbody/SwingFist");
            var mySkillDef = ScriptableObject.CreateInstance<SteppedSkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(SwingComboFistAlt));
            mySkillDef.activationStateMachineName = oldDef.activationStateMachineName;
            mySkillDef.baseMaxStock = oldDef.baseMaxStock;
            mySkillDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            mySkillDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            mySkillDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            mySkillDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            mySkillDef.interruptPriority = oldDef.interruptPriority;
            mySkillDef.isCombatSkill = oldDef.isCombatSkill;
            mySkillDef.mustKeyPress = oldDef.mustKeyPress;
            mySkillDef.rechargeStock = oldDef.rechargeStock;
            mySkillDef.requiredStock = oldDef.requiredStock;
            mySkillDef.stockToConsume = oldDef.stockToConsume;
            mySkillDef.icon = oldDef.icon;
            mySkillDef.skillDescriptionToken = "DC_LOADER_PRIMARY_KNUCKLEBOOM_DESCRIPTION";
            mySkillDef.skillName = "DC_LOADER_PRIMARY_KNUCKLEBOOM_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;
            mySkillDef.stepCount = 3;
            mySkillDef.resetStepsOnIdle = true;

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            LanguageAPI.Add("DC_LOADER_SECONDARY_SHIELD_NAME", "Debris Shield");
            LanguageAPI.Add("DC_LOADER_SECONDARY_SHIELD_DESCRIPTION", "Shield yourself for <style=cIsHealing>100% of your health for 3 seconds</style> while also <style=cIsUtility>increasing your move speed.</style>");

            var mySkillDefSecondary = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDefSecondary.activationState = new SerializableEntityStateType(typeof(ActivateShield));
            mySkillDefSecondary.activationStateMachineName = "Pylon";
            mySkillDefSecondary.baseMaxStock = 1;
            mySkillDefSecondary.baseRechargeInterval = 5f;
            mySkillDefSecondary.beginSkillCooldownOnSkillEnd = true;
            mySkillDefSecondary.canceledFromSprinting = false;
            mySkillDefSecondary.fullRestockOnAssign = true;
            mySkillDefSecondary.interruptPriority = InterruptPriority.Any;
            mySkillDefSecondary.isCombatSkill = false;
            mySkillDefSecondary.mustKeyPress = false;
            mySkillDefSecondary.rechargeStock = 1;
            mySkillDefSecondary.requiredStock = 1;
            mySkillDefSecondary.stockToConsume = 1;
            mySkillDefSecondary.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDefSecondary.skillDescriptionToken = "DC_LOADER_SECONDARY_SHIELD_DESCRIPTION";
            mySkillDefSecondary.skillName = "DC_LOADER_SECONDARY_SHIELD_NAME";
            mySkillDefSecondary.skillNameToken = mySkillDefSecondary.skillName;

            LoadoutAPI.AddSkillDef(mySkillDefSecondary);

            skillFamily = skillLocator.secondary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDefSecondary,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDefSecondary.skillNameToken, false, null)
            };

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
            };
        }

        private static void SetupBuffs()
        {

        }

        private static void Hooks()
        {
            On.RoR2.Projectile.ProjectileGrappleController.FlyState.FixedUpdateBehavior += FlyState_FixedUpdateBehavior;
            //On.EntityStates.Loader.SwingComboFist.PlayAnimation += SwingComboFist_PlayAnimation;
        }

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
                Chat.AddMessage($"tangentMode = {keyframe.tangentMode} //obsolete");
                Chat.AddMessage($"inTangent = {keyframe.inTangent}");
                Chat.AddMessage($"outTangent = {keyframe.outTangent}");
                Chat.AddMessage($"weightedMode = {keyframe.weightedMode}");
                Chat.AddMessage($"inWeight = {keyframe.inWeight}");
                Chat.AddMessage($"outWeight = {keyframe.outWeight}");
            }

            orig(self);
        }

        private static void FlyState_FixedUpdateBehavior(On.RoR2.Projectile.ProjectileGrappleController.FlyState.orig_FixedUpdateBehavior orig, BaseState self)
        {
            var here = self as ProjectileGrappleController.FlyState;
            if (here.owner.characterBody?.skillLocator?.utility?.skillDef == UtilitySkillDef)
            {
                here.FixedUpdateBehavior();
                if (here.isAuthority)
                {
                    if (here.grappleController.projectileStickOnImpactController.stuck)
                    {
                        EntityState entityState = null;
                        if (here.grappleController.projectileStickOnImpactController.stuckBody) //combine with top with an OR?
                        {
                            Rigidbody component = here.grappleController.projectileStickOnImpactController.stuckBody.GetComponent<Rigidbody>();
                            if (component)
                            {
                                CharacterBody component2 = component.GetComponent<CharacterBody>();
                                if (!component2 || !component2.isPlayerControlled || component2.teamComponent.teamIndex != here.projectileController.teamFilter.teamIndex || FriendlyFireManager.ShouldDirectHitProceed(component2.healthComponent, here.projectileController.teamFilter.teamIndex))
                                {
                                    entityState = new StraightPullState();
                                }
                            }
                        }
                        if (entityState == null)
                        {
                            entityState = new ProjectileGrappleController.GripState();
                        }
                        here.DeductOwnerStock();
                        here.outer.SetNextState(entityState);
                        return;
                    }
                    if (here.duration <= here.fixedAge) //remove?
                    {
                        here.outer.SetNextState(new ProjectileGrappleController.ReturnState());
                        return;
                    }
                }
            } else
            {
                orig(self);
            }
        }
    }
}
