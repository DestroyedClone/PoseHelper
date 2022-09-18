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
using EntityStates.Commando.CommandoWeapon;

namespace AlternateSkills.Commando
{
	public class ESDualTap : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
			this.totalDuration = FireSweepBarrage.baseTotalDuration / this.attackSpeedStat;
			this.firingDuration = FireSweepBarrage.baseFiringDuration / this.attackSpeedStat;
			base.characterBody.SetAimTimer(3f);
			this.PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
			this.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
			Util.PlaySound(FireSweepBarrage.enterSound, base.gameObject);
            GetTargets();
            this.FireBullets();
        }
        
        private void FireBullets()
		{
            string[] muzzles = new string[]{"MuzzleLeft","MuzzleRight"};
            int firedBullets = 0;
            foreach (var targetMuzzle in muzzles)
            {
			    Util.PlaySound(FirePistol2.firePistolSoundString, base.gameObject);
                
                if (FirePistol2.muzzleEffectPrefab)
                {
                    EffectManager.SimpleMuzzleFlash(FirePistol2.muzzleEffectPrefab, base.gameObject, targetMuzzle, false);
                }

                base.AddRecoil(-0.4f * FirePistol2.recoilAmplitude, -0.8f * FirePistol2.recoilAmplitude, -0.3f * FirePistol2.recoilAmplitude, 0.3f * FirePistol2.recoilAmplitude);
                
			if (base.isAuthority)
			{
                if (this.targetHurtboxes.Count > 0 && firedBullets > ((char)targetHurtboxes.Count))
                {
                    
                }

				new BulletAttack
				{
					owner = base.gameObject,
					weapon = base.gameObject,
					origin = this.aimRay.origin,
					aimVector = this.aimRay.direction,
					minSpread = 0f,
					maxSpread = base.characterBody.spreadBloomAngle,
					damage = FirePistol2.damageCoefficient * this.damageStat,
					force = FirePistol2.force,
					tracerEffectPrefab = FirePistol2.tracerEffectPrefab,
					muzzleName = targetMuzzle,
					hitEffectPrefab = FirePistol2.hitEffectPrefab,
					isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
					radius = 0.1f,
					smartCollision = true
				}.Fire();
			}
            }
			base.characterBody.AddSpreadBloom(FirePistol2.spreadBloomValue);
        }
        
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge < this.duration || !base.isAuthority)
			{
				return;
			}
			this.outer.SetNextStateToMain();
		}


		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

        public void GetTargets()
        {
            Ray aimRay = base.GetAimRay();
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam());
			bullseyeSearch.maxAngleFilter = FireSweepBarrage.fieldOfView * 0.5f;
			bullseyeSearch.maxDistanceFilter = FireSweepBarrage.maxDistance;
			bullseyeSearch.searchOrigin = aimRay.origin;
			bullseyeSearch.searchDirection = aimRay.direction;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
			bullseyeSearch.filterByLoS = true;
			bullseyeSearch.RefreshCandidates();
			this.targetHurtboxes = bullseyeSearch.GetResults().Where(new Func<HurtBox, bool>(Util.IsValid)).Distinct(default(HurtBox.EntityEqualityComparer)).ToList<HurtBox>();
			this.totalBulletsToFire = Mathf.Max(this.targetHurtboxes.Count, FireSweepBarrage.minimumFireCount);
			this.timeBetweenBullets = this.firingDuration / (float)this.totalBulletsToFire;
			this.childLocator = base.GetModelTransform().GetComponent<ChildLocator>();
			this.muzzleIndex = this.childLocator.FindChildIndex(FireSweepBarrage.muzzle);
			this.muzzleTransform = this.childLocator.FindChild(this.muzzleIndex);
        }

		public static string enterSound;

		public static string muzzle;

		public static string fireSoundString;

		public static GameObject muzzleEffectPrefab;

		public static GameObject tracerEffectPrefab;

		public static float baseTotalDuration;

		public static float baseFiringDuration;

		public static float fieldOfView;

		public static float maxDistance;

		public static float damageCoefficient;

		public static float procCoefficient;

		public static float force;

		public static int minimumFireCount;

		public static GameObject impactEffectPrefab;

		private float totalDuration;

		private float firingDuration;

		private int totalBulletsToFire;

		private int totalBulletsFired;

		private int targetHurtboxIndex;

		private float timeBetweenBullets;

		private List<HurtBox> targetHurtboxes = new List<HurtBox>();

		private float fireTimer;

		private ChildLocator childLocator;

		private int muzzleIndex;

		private Transform muzzleTransform;


    }
}