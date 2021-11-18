using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2.Skills;
using RoR2;
using EntityStates.Loader;
using RoR2.Projectile;
using UnityEngine;
using EntityStates.Huntress.HuntressWeapon;

namespace ROR1AltSkills.Huntress
{
    public class FireClusterBombs : BaseSkillState
    {
        public static GameObject projectilePrefab = HuntressMain.projectilePrefab;
        public static GameObject effectPrefab;
        public static float baseDuration = 0.5f;
        public static float force = 20f;
        public static string attackString = FireArrow.attackSoundString;
        private float duration = 0.25f;

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
            this.duration = baseDuration / this.attackSpeedStat;
            base.PlayAnimation("Gesture, Additive", "FireArrow", "FireArrow.playbackRate", this.duration);
            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
            Util.PlaySound(attackString, base.gameObject);
            Ray aimRay = base.GetAimRay();
            if (isAuthority)
            {
                ProjectileManager.instance.FireProjectile(projectilePrefab, 
                    aimRay.origin, 
                    Util.QuaternionSafeLookRotation(aimRay.direction), 
                    base.gameObject, 
                    this.damageStat, 
                    force, 
                    Util.CheckRoll(this.critStat, base.characterBody.master),
                    DamageColorIndex.Default, 
                    null, 
                    -1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            //base.PlayAnimation("FullBody, Override", "FireArrowRain");
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
