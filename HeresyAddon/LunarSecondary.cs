using EntityStates.GlobalSkills.LunarNeedle;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static HeresyAddon.Class1;

namespace HeresyAddon
{
    public class LunarSecondary
    {
        public static void ThrowLunarSecondary_PlayThrowAnimation(On.EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary.orig_PlayThrowAnimation orig, ThrowLunarSecondary self)
        {
            orig(self);
            switch (self.characterBody.baseNameToken)
            {
                case "COMMANDO_BODY_NAME":
                    //self.PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", self.duration * 2f);
                    //self.PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", self.duration * 2f);
                    self.PlayAnimation("Gesture, Additive", "FireFMJ", "FireFMJ.playbackRate", self.duration/4f);
                    self.PlayAnimation("Gesture, Override", "FireFMJ", "FireFMJ.playbackRate", self.duration/4f);
                    break;

                case "CROCO_BODY_NAME":
                    self.PlayCrossfade("Gesture, Additive", "Slash3", "Slash.playbackRate", self.duration * 2f, 0.05f);
                    self.PlayCrossfade("Gesture, Override", "Slash3", "Slash.playbackRate", self.duration * 2f, 0.05f);
                    break;

                case "MAGE_BODY_NAME":
                    self.PlayAnimation("Gesture, Additive", "FireNovaBomb", "FireNovaBomb.playbackRate", self.duration);
                    break;

                case "BANDIT2_BODY_NAME":
                    if (cfgNoWarnings.Value)
                        self.PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", self.duration * 1f);
                    //self.PlayAnimation("Gesture, Additive", "BufferEmpty", self.playbackRateParam, self.duration);
                    break;

                case "CAPTAIN_BODY_NAME":
                    self.PlayAnimation("Gesture, Additive", "FireCaptainShotgun");
                    self.PlayAnimation("Gesture, Override", "FireCaptainShotgun");
                    break;

                case "ENGI_BODY_NAME":
                    if (cfgNoWarnings.Value)
                    {
                        float num = self.duration * 0.3f;
                        self.PlayCrossfade("Gesture, Additive", "FireMineRight", "FireMine.playbackRate", self.duration + num, 0.05f);
                    }
                    break;

                case "HUNTRESS_BODY_NAME":
                    //self.PlayAnimation("FullBody, Override", "ThrowGlaive", "ThrowGlaive.playbackRate", self.duration);
                    self.PlayAnimation("FullBody, Override", "FireArrowRain");
                    break;

                case "LOADER_BODY_NAME":
                    //self.PlayAnimation("Grapple", "FireHookExit");
                    //self.PlayAnimation("FullBody, Override", "ChargePunch", "ChargePunch.playbackRate", self.duration);
                    //self.PlayCrossfade("FullBody, Override", "ChargePunch", "ChargePunch.playbackRate", self.duration, 0.1f);
                    //self.PlayAnimation("Body", "PreGroundSlam", "GroundSlam.playbackRate", self.duration);
                    //self.PlayCrossfade("Body", "GroundSlam", 0.2f);
                    
                    break;

                case "MERC_BODY_NAME":
                    self.PlayCrossfade("FullBody, Override", "WhirlwindGround", "Whirlwind.playbackRate", self.duration/2f, 0.1f);
                    //self.PlayAnimation("Gesture, Additive", "GroundLight3", "GroundLight.playbackRate", self.duration);
                    //self.PlayAnimation("Gesture, Override", "GroundLight3", "GroundLight.playbackRate", self.duration);
                    break;

                case "TOOLBOT_BODY_NAME":
                    if (cfgNoWarnings.Value)
                        self.PlayAnimation("Body", "BoxModeExit", self.playbackRateParam, self.duration);
                    //self.PlayAnimation("Gesture, Additive", "FireGrenadeLauncher", "FireGrenadeLauncher.playbackRate", 0.45f / self.attackSpeedStat);
                    break;

                case "TREEBOT_BODY_NAME":
                    if (cfgNoWarnings.Value)
                        self.PlayAnimation("Gesture, Additive", "FireSonicBoom");
                    break;

                case "HERETIC_BODY_NAME":
                default:
                    break;
            }
        }

        //public static int bodySideWeaponLayerIndex;
        public static void ChargeLunarSecondary_PlayChargeAnimation(On.EntityStates.GlobalSkills.LunarNeedle.ChargeLunarSecondary.orig_PlayChargeAnimation orig, ChargeLunarSecondary self)
        {
            switch (self.characterBody.baseNameToken)
            {
                case "COMMANDO_BODY_NAME":
                    //self.PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", self.duration * 10f);
                    //self.PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", self.duration * 10f);
                    self.PlayAnimation("Gesture, Additive", "FireFMJ", "FireFMJ.playbackRate", self.duration * 15);
                    self.PlayAnimation("Gesture, Override", "FireFMJ", "FireFMJ.playbackRate", self.duration * 15);
                    break;

                case "CROCO_BODY_NAME":
                    self.PlayCrossfade("Gesture, Override", "Leap", self.duration);
                    //self.PlayCrossfade("Gesture, AdditiveHigh", "Leap", self.duration); //wavy feet
                    //self.PlayCrossfade("Gesture, Override", "Leap", self.duration);
                    //self.PlayCrossfade("Gesture, Additive", "Slash3", "Slash.playbackRate", self.duration * 5f, 0.05f);
                    //self.PlayCrossfade("Gesture, Override", "Slash3", "Slash.playbackRate", self.duration * 5f, 0.05f);
                    break;

                case "MAGE_BODY_NAME":
                    self.PlayAnimation("Gesture, Additive", "ChargeNovaBomb", "ChargeNovaBomb.playbackRate", self.duration);
                    break;

                case "BANDIT2_BODY_NAME":
                    //self.PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", self.duration);
                    //if (cfgNoWarnings.Value)
                        self.PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", self.duration * 8f);
                    //self.PlayAnimation("Gesture, Additive", "ThrowSmokebomb", "ThrowSmokebomb.playbackRate", self.duration * 3f);
                    break;

                case "CAPTAIN_BODY_NAME":
                    self.PlayAnimation("Gesture, Override", "ChargeCaptainShotgun", "ChargeNovaBomb.playbackRate", self.duration * 20f);
                    self.PlayAnimation("Gesture, Additive", "ChargeCaptainShotgun", "ChargeNovaBomb.playbackRate", self.duration * 20f);
                    break;

                case "ENGI_BODY_NAME":
                    //if (cfgNoWarnings.Value)
                        self.PlayAnimation("Gesture, Additive", "ChargeGrenades");
                    break;

                case "HUNTRESS_BODY_NAME":
                    self.PlayAnimation("FullBody, Override", "ThrowGlaive", "ThrowGlaive.playbackRate", self.duration * 1.5f);
                    break;

                case "LOADER_BODY_NAME":
                    //self.PlayCrossfade("Body", "PreGroundSlam", "GroundSlam.playbackRate", self.duration * 5f, 0.1f);
                    //base.PlayCrossfade("Body", "GroundSlam", 0.2f);
                    //self.PlayCrossfade("Gesture, Additive", "ChargePunchIntro", self.playbackRateParam, self.duration * 5f, 0.1f);
                    //self.PlayCrossfade("Gesture, Override", "ChargePunchIntro", self.playbackRateParam, self.duration * 5f, 0.1f);
                    //self.PlayAnimation("Grapple", "FireHookIntro");
                    //base.PlayAnimation("Grapple", "FireHookLoop");
                    break;

                case "MERC_BODY_NAME":
                    //self.PlayAnimation("FullBody, Override", "EvisPrep", "EvisPrep.playbackRate", EntityStates.Merc.EvisDash.dashPrepDuration);
                    //Animator modelAnimator = self.GetModelAnimator();
                    //self.PlayCrossfade("FullBody, Override", "WhirlwindGround", "Whirlwind.playbackRate", self.duration * 2f, 0.1f);
                    self.PlayCrossfade("FullBody, Override", "Uppercut", "Uppercut.playbackRate", self.duration * 4, 0.1f);
                    break;

                case "TOOLBOT_BODY_NAME":
                    if (cfgNoWarnings.Value)
                        self.PlayCrossfade("Body", "BoxModeEnter", self.duration * 2f);
                    //self.PlayAnimation("Gesture, Additive", "FireGrenadeLauncher", self.playbackRateParam, self.duration * 15f);
                    break;

                case "TREEBOT_BODY_NAME":
                    //self.PlayAnimation("Gesture, Additive", "FireSonicBoom", "GroundSlam.playbackRate", self.duration * 40f);
                    orig(self);
                    break;

                case "HERETIC_BODY_NAME":
                default:
                    orig(self);
                    break;
            }
        }

    }
}
