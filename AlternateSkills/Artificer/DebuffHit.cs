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
using UnityEngine.Networking;
using EntityStates.Croco;
using RoR2.Projectile;

namespace AlternateSkills.Artificer
{
    public class DebuffHit : BaseSkillState
    {
        public GameObject projectilePrefab = Artificer.ArtificerMain.debuffHitProjectile;
        float damageCoefficient = 0.7f;

        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Gesture Right, Additive", "FireGauntletRight", "FireGauntlet.playbackRate", 1f);
            base.PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", 1f);
            this.FireGauntlet();
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        public void FireGauntlet()
        {
            Ray aimRay = base.GetAimRay();
            if (base.isAuthority)
            {
                ProjectileManager.instance.FireProjectile(this.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
        }
    }
}
