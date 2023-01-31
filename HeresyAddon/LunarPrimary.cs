using EntityStates.GlobalSkills.LunarNeedle;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace HeresyAddon
{
    public class LunarPrimary
    {
        //public static bool stepperCommando = false;
        //public static bool stepperMage = false;
        //public static bool stepperEngi = false;
       // public static bool stepperLoader = false;

        public static void FireLunarNeedle_OnEnter(On.EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle.orig_OnEnter orig, global::EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle self)
        {
            self.duration = FireLunarNeedle.baseDuration / self.attackSpeedStat;

            switch (self.characterBody.baseNameToken)
            {
                case "COMMANDO_BODY_NAME":
                    orig(self);
                    //OnEnter_Commando(self); //Disabled due to console spam
                    break;

                case "CROCO_BODY_NAME":
                    OnEnter_Croco(self);
                    break;

                case "MAGE_BODY_NAME":
                    OnEnter_Mage(self);
                    break;

                case "BANDIT2_BODY_NAME":
                    OnEnter_Bandit2(self);
                    break;

                case "CAPTAIN_BODY_NAME":
                    OnEnter_Captain(self);
                    break;

                case "ENGI_BODY_NAME":
                    OnEnter_Engi(self);
                    break;

                case "HUNTRESS_BODY_NAME":
                    OnEnter_Huntress(self);
                    break;

                case "LOADER_BODY_NAME":
                    OnEnter_Loader(self);
                    break;

                case "MERC_BODY_NAME":
                    OnEnter_Merc(self);
                    break;

                case "TOOLBOT_BODY_NAME":
                    OnEnter_Tool(self);
                    break;

                case "TREEBOT_BODY_NAME":
                    OnEnter_Tree(self);
                    break;

                case "VOIDSURVIVOR_BODY_NAME":
                    OnEnter_VoidSurvivor(self);
                    break;

                case "RAILGUNNER_BODY_NAME":
                    OnEnter_Railgunner(self);
                    break;

                case "HERETIC_BODY_NAME":
                default:
                    orig(self);
                    break;
            }
            self.StartAimMode(2f, false);
            self.characterBody.AddSpreadBloom(FireLunarNeedle.spreadBloomValue);
        }

        private static void OnEnter_VoidSurvivor(FireLunarNeedle self)
        {
            self.PlayAnimation("Gesture, Additive", "FireNailgun");
            self.GetModelAnimator().SetBool("isFiringNailgun", true);
            FireNeedle(self, EntityStates.VoidSurvivor.Weapon.FireBlasterBase.);
        }

        private static void OnEnter_Railgunner(FireLunarNeedle self)
        {
            self.PlayAnimation("Gesture, Additive", "FireNailgun");
            self.GetModelAnimator().SetBool("isFiringNailgun", true);
            FireNeedle(self, EntityStates.Railgunner.Weapon.FirePistol.animationStateName);
        }

        private static void OnEnter_Tool(FireLunarNeedle self)
        {
            self.PlayAnimation("Gesture, Additive", "FireNailgun");
            self.GetModelAnimator().SetBool("isFiringNailgun", true);
            FireNeedle(self, EntityStates.Toolbot.BaseNailgunState.muzzleName);
        }

        private static void OnEnter_Tree(FireLunarNeedle self)
        {
            self.PlayAnimation("Gesture, Additive", "FireSyringe");
            FireNeedle(self, EntityStates.Treebot.Weapon.FireSyringe.muzzleName);
        }

        private static void OnEnter_Merc(FireLunarNeedle self)
        {
            Animator modelAnimator = self.GetModelAnimator();
            if (modelAnimator)
            {
                bool @bool = modelAnimator.GetBool("isMoving");
                bool bool2 = modelAnimator.GetBool("isGrounded");
                if (@bool || !bool2)
                {
                    self.PlayAnimation("Gesture, Additive", "GroundLight3", "GroundLight.playbackRate", self.duration);
                    self.PlayAnimation("Gesture, Override", "GroundLight3", "GroundLight.playbackRate", self.duration);
                }
                else
                {
                    self.PlayAnimation("FullBody, Override", "GroundLight3", "GroundLight.playbackRate", self.duration);
                }
            }
            FireNeedle(self, "Head");
        }

        private static void OnEnter_Loader(FireLunarNeedle self)
        {
            //var stepper = self.characterBody.GetComponent<Class1.HeresyStepperTracker>();
            //stepper.stepperLoader = !stepper.stepperLoader;
            //string animationStateName = (stepper.Step()) ? "SwingFistRight" : "SwingFistLeft";
            //self.PlayCrossfade("Gesture, Additive", animationStateName, "SwingFist.playbackRate", self.duration, 0.1f);
            //self.PlayCrossfade("Gesture, Override", animationStateName, "SwingFist.playbackRate", self.duration, 0.1f);
            self.PlayCrossfade("FullBody, Override", "ChargePunch", "ChargePunch.playbackRate", self.duration, 0.1f);
            FireNeedle(self, "Head"); //todo find good muzzle
        }

        private static void OnEnter_Huntress(FireLunarNeedle self)
        {
            self.PlayAnimation("Gesture, Additive", "FireArrow", "FireArrow.playbackRate", self.duration * 2f);
            self.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", self.duration * 2f);
            //self.PlayAnimation("Body", "FireArrowSnipe", "FireArrowSnipe.playbackRate", self.duration); 
            FireNeedle(self, "Muzzle");
        }

        private static void OnEnter_Heretic(FireLunarNeedle self)
        {
        }

        private static void OnEnter_Engi(FireLunarNeedle self)
        {
            var stepper = self.characterBody.GetComponent<HeresyAnimsPlugin.HeresyStepperTracker>();
            if (stepper.Step())
            {
                self.PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", 0.1f);
                FireNeedle(self, "MuzzleLeft");
            }
            else
            {
                self.PlayCrossfade("Gesture Right Cannon, Additive", "FireGrenadeRight", 0.1f);
                FireNeedle(self, "MuzzleRight");
            }
        }

        private static void OnEnter_Captain(FireLunarNeedle self)
        {
            //self.PlayAnimation("Gesture, Additive", "FireCaptainShotgun");
            //self.PlayAnimation("Gesture, Override", "FireCaptainShotgun");
            self.PlayCrossfade("Gesture, Override", "ChargeCaptainShotgun", "ChargeCaptainShotgun.playbackRate", self.duration, 0.1f);
            self.PlayCrossfade("Gesture, Additive", "ChargeCaptainShotgun", "ChargeCaptainShotgun.playbackRate", self.duration, 0.1f);
            FireNeedle(self, EntityStates.Captain.Weapon.FireTazer.targetMuzzle);
        }

        private static void OnEnter_Bandit2(FireLunarNeedle self)
        {
            self.PlayAnimation("Gesture, Additive", "FireMainWeapon", self.playbackRateParam, self.duration);
            //self.PlayAnimation("Gesture, Additive", (self.characterBody.isSprinting && self.characterMotor && self.characterMotor.isGrounded) ? "ReloadSimple" : "Reload", "Reload.playbackRate", self.duration);
            //self.PlayAnimation("Gesture, Additive", "FireSideWeapon", "FireSideWeapon.playbackRate", self.duration);
            //self.PlayAnimation("Gesture, Additive", "MainToSide", "MainToSide.playbackRate", self.duration);
            FireNeedle(self, "MuzzleShotgun");
        }

        private static void OnEnter_Mage(FireLunarNeedle self)
        {
            var stepper = self.characterBody.GetComponent<Class1.HeresyStepperTracker>();
            //stepper.stepperMage = !stepper.stepperMage;
            if (stepper.Step())
            {
                if (self.attackSpeedStat < EntityStates.Mage.Weapon.FireFireBolt.attackSpeedAltAnimationThreshold)
                {
                    self.PlayCrossfade("Gesture, Additive", "Cast1Right", "FireGauntlet.playbackRate", self.duration, 0.1f);
                    self.PlayAnimation("Gesture Left, Additive", "Empty");
                    self.PlayAnimation("Gesture Right, Additive", "Empty");
                    return;
                }
                self.PlayAnimation("Gesture Right, Additive", "FireGauntletRight", "FireGauntlet.playbackRate", self.duration);
                self.PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", self.duration);
                FireNeedle(self, "MuzzleRight");
                return;
            }
            else
            {
                if (self.attackSpeedStat < EntityStates.Mage.Weapon.FireFireBolt.attackSpeedAltAnimationThreshold)
                {
                    self.PlayCrossfade("Gesture, Additive", "Cast1Left", "FireGauntlet.playbackRate", self.duration, 0.1f);
                    self.PlayAnimation("Gesture Left, Additive", "Empty");
                    self.PlayAnimation("Gesture Right, Additive", "Empty");
                    return;
                }
                self.PlayAnimation("Gesture Left, Additive", "FireGauntletLeft", "FireGauntlet.playbackRate", self.duration);
                self.PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", self.duration);
                FireNeedle(self, "MuzzleLeft");
                return;
            }
        }

        private static void OnEnter_Croco(FireLunarNeedle self)
        {
            self.PlayAnimation("Gesture, Mouth", "FireSpit", "FireSpit.playbackRate", self.duration);
            FireNeedle(self, "MouthMuzzle");
        }

        private static void OnEnter_Commando(FireLunarNeedle self)
        {
            var stepper = self.characterBody.GetComponent<Class1.HeresyStepperTracker>();
            if (stepper.Step())
            {
                self.PlayAnimation("Gesture Additive, Left", "FirePistol, Left", self.playbackRateParam, self.duration);
                FireNeedle(self, "MuzzleLeft");
            }
            else
            {
                self.PlayAnimation("Gesture Additive, Right", "FirePistol, Right", self.playbackRateParam, self.duration);
                FireNeedle(self, "MuzzleRight");
            }
            //self.PlayAnimation(self.animationLayerName, self.animationStateName, self.playbackRateParam, self.duration);
            //Gesture, Override FireLunarNeedle FireLunarNeedle.playbackRate 0.11
        }

        private static void PlayMuzzleEffect(FireLunarNeedle self, string targetMuzzle)
        {
            if (FireLunarNeedle.muzzleFlashEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireLunarNeedle.muzzleFlashEffectPrefab, self.gameObject, targetMuzzle, false);
            }
        }

        private static void FireNeedle(FireLunarNeedle self, string targetMuzzle)
        {
            Util.PlaySound(FireLunarNeedle.fireSound, self.gameObject);
            PlayMuzzleEffect(self, targetMuzzle);
            if (self.isAuthority)
            {
                Ray aimRay = self.GetAimRay();
                aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, FireLunarNeedle.maxSpread, 1f, 1f, 0f, 0f);
                FireProjectileInfo fireProjectileInfo = default;
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                fireProjectileInfo.crit = self.characterBody.RollCrit();
                fireProjectileInfo.damage = self.characterBody.damage * FireLunarNeedle.damageCoefficient;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                fireProjectileInfo.owner = self.gameObject;
                fireProjectileInfo.procChainMask = default;
                fireProjectileInfo.force = 0f;
                fireProjectileInfo.useFuseOverride = false;
                fireProjectileInfo.useSpeedOverride = false;
                fireProjectileInfo.target = null;
                fireProjectileInfo.projectilePrefab = FireLunarNeedle.projectilePrefab;
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
    }
}