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
	public class ESEDFRun : BaseSkillState
    {
        public BuffDef buffToGive = RoR2Content.Buffs.CloakSpeed;

		public override void OnEnter()
		{
			base.OnEnter();
			Transform modelTransform = base.GetModelTransform();
            if (base.characterBody)
            {
                base.characterBody.isSprinting = true;
                base.characterBody.AddBuff(buffToGive);
            }
            if (base.isAuthority)
            {
                base.gameObject.layer = LayerIndex.fakeActor.intVal;
                base.characterMotor.Motor.RebuildCollidableLayers();
            }
		}

		public override void OnExit()
		{
            if (base.isAuthority)
            {
                base.gameObject.layer = LayerIndex.defaultLayer.intVal;
                base.characterMotor.Motor.RebuildCollidableLayers();
            }
            if (base.characterBody)
            {
                base.characterBody.RemoveBuff(buffToGive);
                base.characterBody.isSprinting = false;
            }
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
            if (base.characterBody)
                base.characterBody.isSprinting = true;
			if ((!base.inputBank || !base.inputBank.skill3.down) && base.isAuthority)
			{
                outer.SetNextStateToMain();
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
    }
}