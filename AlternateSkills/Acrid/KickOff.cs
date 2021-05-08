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
    public class KickOff : BaseSkillState, IOnKilledOtherServerReceiver
    {

		public void OnKilledOtherServer(DamageReport damageReport)
        {
            if (damageReport.attacker && damageReport.attackerBody == this.characterBody)
            {

            }
        }
    }
}
