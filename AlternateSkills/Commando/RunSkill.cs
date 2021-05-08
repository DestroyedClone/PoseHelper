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
    public class RunSkill : BaseSkillState
	{
		private bool released;
		public override void OnEnter()
		{
			base.OnEnter();
			if (this.characterBody && !this.characterBody.HasBuff(Buffs.runningBuff))
			{
				this.characterBody.AddBuff(Buffs.runningBuff);
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if (this.characterBody && this.characterBody.HasBuff(Buffs.runningBuff))
			{
				this.characterBody.RemoveBuff(Buffs.runningBuff);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority)
			{
				if (!this.released && (!base.inputBank || !base.inputBank.skill4.down))
				{
					this.released = true;
				}
				if (this.released)
				{
					OnExit();
				}
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
