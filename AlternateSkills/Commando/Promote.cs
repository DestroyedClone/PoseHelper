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
	public class Promote : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			ChooseTarget();
		}

		public override void OnExit()
		{
			base.OnExit();

		}

		private void ChooseTarget()
        {

        }
	}
}
