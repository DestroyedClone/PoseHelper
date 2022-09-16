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
    public class ESKickOff : ChainableLeap, IOnKilledOtherServerReceiver
    {
        public int remainingLeaps = 2;

        public override void OnEnter()
        {
            base.OnEnter();
        }

		public void OnKilledOtherServer(DamageReport damageReport)
        {
            if (damageReport.attacker && damageReport.attackerBody == this.characterBody)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, 3);
            }
        }

        public override void DoImpactAuthority()
		{
			base.DoImpactAuthority();
            if (remainingLeaps > 0)
            {
                BlastAttack.Result result = base.DetonateAuthority();
                if (result.hitCount > 0)
                {
                    var nextState = new ESKickOff();
                    nextState.remainingLeaps -= 1;
                    this.outer.SetNextState(nextState);
                }
            }
			//base.skillLocator.utility.RunRecharge((float)result.hitCount * ChainableLeap.refundPerHit);
		}
    }
}
