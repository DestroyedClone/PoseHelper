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

namespace AlternateSkills.Commando
{
    public class BackupShiv : BaseSkillState, IOnKilledOtherServerReceiver
    {
		public float damageCoefficient = 2.2f;

		public override void OnEnter()
		{
			base.OnEnter();
			Attack();
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		private void Attack()
		{
			EffectManager.SimpleMuzzleFlash(EntityStates.ImpMonster.DoubleSlash.swipeEffectPrefab, base.gameObject, "MuzzleRight", false);
			if (NetworkServer.active)
			{
				new BlastAttack
				{
					attacker = base.gameObject,
					inflictor = base.gameObject,
					teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
					baseDamage = this.damageStat * damageCoefficient,
					baseForce = 0,
					position = base.transform.position,
					radius = 9f,
					falloffModel = BlastAttack.FalloffModel.None,
					attackerFiltering = AttackerFiltering.NeverHit,
				}.Fire();
			}
		}

		public void OnKilledOtherServer(DamageReport damageReport)
        {
			if (damageReport.attackerBody = this.characterBody)
            {
				this.healthComponent.HealFraction(0.07f, default);
            }
        }
	}
}
