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
using EntityStates.Engi.EngiWeapon;

namespace AlternateSkills.Merc
{
	public class ESChargeSlash : BaseSkillState
    {
        // Use two skillstates?
        // On Enter, sheck if button is held
        // if held, increase value and apply slow debuff
        // on exit, exit skill + fire slash.

        public override void OnEnter()
		{
			base.OnEnter();
			this.totalDuration = ChargeGrenades.baseTotalDuration / this.attackSpeedStat;
			this.maxChargeTime = ChargeGrenades.baseMaxChargeTime / this.attackSpeedStat;
			Util.PlaySound(ChargeGrenades.chargeLoopStartSoundString, base.gameObject);
		}

		public override void OnExit()
		{
			base.OnExit();
			Util.PlaySound(ChargeGrenades.chargeLoopStopSoundString, base.gameObject);
            if (hasFullyCharged)
            {
                characterBody.RemoveBuff(JunkContent.Buffs.Deafened);
                base.characterBody.RemoveBuff(DLC1Content.Buffs.Blinded);
            }
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.lastCharge = this.charge;
			this.charge = Mathf.Clamp(base.fixedAge / this.maxChargeTime, 0f, 1f);
			
            float t = (float)this.charge / (float)ChargeGrenades.maxCharges;
			float value = Mathf.Lerp(ChargeGrenades.minBonusBloom, ChargeGrenades.maxBonusBloom, t);
			base.characterBody.SetSpreadBloom(value, true);
            
            if (!hasFullyCharged && charge >= 1)
            {
                hasFullyCharged = true;
                base.characterBody.AddBuff(JunkContent.Buffs.Deafened);
                base.characterBody.AddBuff(DLC1Content.Buffs.Blinded);
            }
            if ((!base.inputBank || !base.inputBank.skill4.down) && base.isAuthority)
			{
				ESFireSlash esFireFlash = new ESFireSlash();
				esFireFlash.charge = charge;
				this.outer.SetNextState(esFireFlash);
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
        public bool hasFullyCharged = false;

		public static float baseTotalDuration;

		public static float baseMaxChargeTime;

		public static GameObject chargeEffectPrefab;

		private float charge;

		private float lastCharge;

		private float totalDuration;

		private float maxChargeTime;
    }
}