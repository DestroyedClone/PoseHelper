using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.Bandit2.Weapon;
using EntityStates.GlobalSkills.LunarNeedle;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using static HeresyAddon.LunarPrimary;
using static HeresyAddon.LunarSecondary;
using static HeresyAddon.LunarUtility;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HeresyAddon
{
    [BepInPlugin("com.DestroyedClone.HeresyAnims", "HeresyAnims", "1.1.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class HeresyAnimsPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> cfgNoWarnings;
        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Start()
        {
            cfgNoWarnings = Config.Bind("", "No Warnings", false, "If true, then the animations that play with errors will play.");

            if (cfgNoWarnings.Value)
            {
                _logger.LogMessage("Config Setting \"No Warnings\" set to true, " +
                    "Charge animations for Bandit2, Engineer, MUL-T, and REX will heft up the size of your log.");
            }
            On.EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle.OnEnter += FireLunarNeedle_OnEnter;
            On.EntityStates.GlobalSkills.LunarNeedle.ChargeLunarSecondary.PlayChargeAnimation += ChargeLunarSecondary_PlayChargeAnimation;
            On.EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary.PlayThrowAnimation += ThrowLunarSecondary_PlayThrowAnimation;
            On.EntityStates.GhostUtilitySkillState.OnEnter += GhostUtilitySkillState_OnEnter;
            On.EntityStates.EntityState.OnExit += EntityState_OnExit;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            //On.EntityStates.GlobalSkills.LunarDetonator.Detonate.OnEnter += Detonate_OnEnter;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            var tracker = obj.gameObject.AddComponent<HeresyStepperTracker>();
        }

        public class HeresyStepperTracker : MonoBehaviour
        {
            public bool toggle = false;

            public bool Step()
            {
                toggle = !toggle;
                return toggle;
            }
        }

        private void EntityState_OnExit(On.EntityStates.EntityState.orig_OnExit orig, EntityState self)
        {
            orig(self);
            if (self is EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle)
            {
                var fireLunarNeedle = (FireLunarNeedle)self;
                bool isNotHeldOrEmpty = fireLunarNeedle.activatorSkillSlot.stock == 0 || !fireLunarNeedle.inputBank.skill1.down;
                switch (fireLunarNeedle.characterBody.baseNameToken)
                {
                    case "TOOLBOT_BODY_NAME":
                        if (isNotHeldOrEmpty)
                            fireLunarNeedle.GetModelAnimator().SetBool("isFiringNailgun", false);
                        break;

                    case "CAPTAIN_BODY_NAME":
                        if (isNotHeldOrEmpty)
                        {
                            fireLunarNeedle.PlayAnimation("Gesture, Additive", "FireCaptainShotgun", fireLunarNeedle.playbackRateParam, fireLunarNeedle.duration);
                            fireLunarNeedle.PlayAnimation("Gesture, Override", "FireCaptainShotgun", fireLunarNeedle.playbackRateParam, fireLunarNeedle.duration);
                        }
                        break;

                    case "HUNTRESS_BODY_NAME":
                        //if (isNotHeldOrEmpty)
                        //self.PlayAnimation("Body", "FireArrowSnipe", "FireArrowSnipe.playbackRate", 0.1f);
                        break;

                    case "BANDIT2_BODY_NAME":
                        if (isNotHeldOrEmpty)
                        {
                            Util.PlayAttackSpeedSound(Reload.exitSoundString, base.gameObject, Reload.exitSoundPitch);
                            //EffectManager.SimpleMuzzleFlash(Reload.reloadEffectPrefab, base.gameObject, Reload.reloadEffectMuzzleString, false);
                            self.PlayAnimation("Gesture, Additive", (self.characterBody.isSprinting && self.characterMotor && self.characterMotor.isGrounded) ? "ReloadSimple" : "Reload", "Reload.playbackRate", fireLunarNeedle.duration);
                        }
                        break;
                }
            }
        }
    }
}