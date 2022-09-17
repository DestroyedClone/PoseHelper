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

namespace AlternateSkills.Acrid
{
    public class ESFang : EntityStates.Croco.Bite
    {
        public override void OnEnter()
        {
            base.OnEnter();
            damageCoefficient = 4.5f;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
		{
			base.AuthorityModifyOverlapAttack(overlapAttack);
			DamageType damageType = this.crocoDamageTypeController ? this.crocoDamageTypeController.GetDamageType() : DamageType.Generic;
			overlapAttack.damageType = (damageType | DamageType.BleedOnHit);
		}
    }

}