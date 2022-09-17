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
using EntityStates.Captain;
using AlternateSkills.Modules;

namespace AlternateSkills.Captain
{
    public class ESChargeAgileShotgun : EntityStates.Captain.Weapon.ChargeCaptainShotgun
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                characterBody.AddBuff(Buffs.captainAgilityBuff);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(Buffs.captainAgilityBuff);
            }
        }
    }

}