using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Runtime.CompilerServices;
using EntityStates;
using EntityStates.GlobalSkills;
using EntityStates.GlobalSkills.LunarNeedle;
using EntityStates.Commando.CommandoWeapon;
using static HeresyAddon.LunarPrimary;
using static HeresyAddon.LunarSecondary;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HeresyAddon
{
    [BepInPlugin("com.DestroyedClone.HeresyAddOn", "Heresy AddOn", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public void Start()
        {
            On.EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle.OnEnter += FireLunarNeedle_OnEnter;
            On.EntityStates.GlobalSkills.LunarNeedle.ChargeLunarSecondary.PlayChargeAnimation += ChargeLunarSecondary_PlayChargeAnimation;
            On.EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary.PlayThrowAnimation += ThrowLunarSecondary_PlayThrowAnimation;
            On.EntityStates.GhostUtilitySkillState.OnEnter += GhostUtilitySkillState_OnEnter;
            On.EntityStates.EntityState.OnExit += EntityState_OnExit;
        }

        private void GhostUtilitySkillState_OnEnter(On.EntityStates.GhostUtilitySkillState.orig_OnEnter orig, GhostUtilitySkillState self)
        {
            orig(self);

            switch (self.characterBody.baseNameToken)
            {
                case "BANDIT2_BODY_NAME":
                    self.PlayAnimation("Gesture, Additive", "ThrowSmokebomb", "ThrowSmokebomb.playbackRate", EntityStates.Bandit2.ThrowSmokebomb.duration);
                    break;
                default:
                    break;
            }
        }

        private void EntityState_OnExit(On.EntityStates.EntityState.orig_OnExit orig, EntityState self)
        {
            orig(self);
            if (self is EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle)
            {
                var fireLunarNeedle = (FireLunarNeedle)self;
                switch (fireLunarNeedle.characterBody.baseNameToken)
                {
                    case "TOOLBOT_BODY_NAME":
                        fireLunarNeedle.GetModelAnimator().SetBool("isFiringNailgun", false);
                        break;
                    case "CAPTAIN_BODY_NAME":
                        if (fireLunarNeedle.activatorSkillSlot.stock == 0 || !fireLunarNeedle.inputBank.skill1.down)
                        {
                            fireLunarNeedle.PlayAnimation("Gesture, Additive", "FireCaptainShotgun", fireLunarNeedle.playbackRateParam, fireLunarNeedle.duration);
                            fireLunarNeedle.PlayAnimation("Gesture, Override", "FireCaptainShotgun", fireLunarNeedle.playbackRateParam, fireLunarNeedle.duration);
                        }
                        break;
                    case "HUNTRESS_BODY_NAME":
                        self.PlayAnimation("Body", "FireArrowSnipe", "FireArrowSnipe.playbackRate", 0.1f);
                        break;
                }
            }
        }



    }
}
