using System;
using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.JellyfishMonster;

namespace JellyfishShock
{
	public class JellyShockSkill : BaseState
	{
		public JellyShockSkill()
        {
			chargingEffectPrefab = JellyNova.chargingEffectPrefab;
			novaEffectPrefab = JellyNova.novaEffectPrefab;
			chargingSoundString = ShockState.enterSoundString;
			novaSoundString = JellyNova.novaSoundString;
			novaDamageCoefficient = 0.8f;
			novaRadius = JellyNova.novaRadius;
			novaForce = 0;
		}


		public static float baseDuration = 1f;
		public static GameObject chargingEffectPrefab;
		public static GameObject novaEffectPrefab;
		public static string chargingSoundString;
		public static string novaSoundString;
		public static float novaDamageCoefficient;
		public static float novaRadius;
		public static float novaForce;
		private bool hasExploded;
		private float duration;
		private float stopwatch;
		private GameObject chargeEffect;
		private PrintController printController;
		private uint soundID;
		public override void OnEnter()
		{
			base.OnEnter();
			this.stopwatch = 0f;
			this.duration = JellyNova.baseDuration / this.attackSpeedStat;
			Transform modelTransform = base.GetModelTransform();
			base.PlayCrossfade("Body", "Nova", "Nova.playbackRate", this.duration, 0.1f);
			this.soundID = Util.PlaySound(JellyNova.chargingSoundString, base.gameObject);
			if (JellyNova.chargingEffectPrefab)
			{
				this.chargeEffect = UnityEngine.Object.Instantiate<GameObject>(JellyNova.chargingEffectPrefab, base.transform.position, base.transform.rotation);
				this.chargeEffect.transform.parent = base.transform;
				this.chargeEffect.transform.localScale = new Vector3(JellyNova.novaRadius, JellyNova.novaRadius, JellyNova.novaRadius);
				this.chargeEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = this.duration;
			}
			if (modelTransform)
			{
				this.printController = modelTransform.GetComponent<PrintController>();
				if (this.printController)
				{
					this.printController.enabled = true;
					this.printController.printTime = this.duration;
				}
			}
		}

		// Token: 0x0600409B RID: 16539 RVA: 0x001007D0 File Offset: 0x000FE9D0
		public override void OnExit()
		{
			base.OnExit();
			AkSoundEngine.StopPlayingID(this.soundID);
			if (this.chargeEffect)
			{
				EntityState.Destroy(this.chargeEffect);
			}
			if (this.printController)
			{
				this.printController.enabled = false;
			}
		}

		// Token: 0x0600409C RID: 16540 RVA: 0x0010081F File Offset: 0x000FEA1F
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatch += Time.fixedDeltaTime;
			if (this.stopwatch >= this.duration && base.isAuthority && !this.hasExploded)
			{
				this.Detonate();
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x0600409D RID: 16541 RVA: 0x00100860 File Offset: 0x000FEA60
		private void Detonate()
		{
			this.hasExploded = true;
			Util.PlaySound(JellyNova.novaSoundString, base.gameObject);
			if (this.chargeEffect)
			{
				EntityState.Destroy(this.chargeEffect);
			}
			if (JellyNova.novaEffectPrefab)
			{
				EffectManager.SpawnEffect(JellyNova.novaEffectPrefab, new EffectData
				{
					origin = base.transform.position,
					scale = JellyNova.novaRadius
				}, true);
			}
			new BlastAttack
			{
				attacker = base.gameObject,
				inflictor = base.gameObject,
				teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
				baseDamage = this.damageStat * JellyNova.novaDamageCoefficient,
				baseForce = JellyNova.novaForce,
				position = base.transform.position,
				radius = JellyNova.novaRadius,
				procCoefficient = 1f,
				attackerFiltering = AttackerFiltering.NeverHit
			}.Fire();
		}

		// Token: 0x0600409E RID: 16542 RVA: 0x0006E9B6 File Offset: 0x0006CBB6
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
