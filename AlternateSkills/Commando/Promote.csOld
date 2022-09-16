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
		private GameObject target = null;

		public override void OnEnter()
		{
			base.OnEnter();
			OnExit();
		}

		public override void OnExit()
		{
			base.OnExit();
			ChooseTarget();
			if (target)
			{
				PromoteTarget();
			} else
            {
				Debug.Log("No target!");
            }

			this.outer.SetNextStateToMain();
		}

		private void ChooseTarget()
        {
			InputBankTest component = characterBody.inputBank;
			if (component)
			{
				Ray ray = new Ray(component.aimOrigin, component.aimDirection);
				if (Util.CharacterRaycast(gameObject, ray, out RaycastHit raycastHit, float.PositiveInfinity, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
				{
					target = raycastHit.collider.gameObject;
					Debug.Log("Fired Raycast");
				}
			}
		}

		private void PromoteTarget()
		{
			if (target.GetComponent<CharacterBody>())
			{
				target.GetComponent<CharacterBody>().AddTimedBuff(Buffs.promotedBuff, 16f);
				Chat.AddMessage("Promoting target!");
			} else
			{
				Debug.Log("No characterBody!");
			}
		}
	}
}
