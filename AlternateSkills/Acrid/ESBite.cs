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

namespace AlternateSkills
{
    public class ESBite : EntityStates.Croco.Bite
    {
        public override void OnEnter()
        {
            base.OnEnter();
            damageCoefficient = 2f;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
		{
			base.AuthorityModifyOverlapAttack(overlapAttack);
			overlapAttack.damageType = DamageType.Generic;
		}
    }

}