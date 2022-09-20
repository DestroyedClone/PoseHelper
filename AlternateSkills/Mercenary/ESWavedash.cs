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
using EntityStates.Merc;

namespace AlternateSkills.Merc
{
	public class ESWavedash : Assaulter2
    {
        public override void AuthorityFixedUpdate()
        {
            base.AuthorityFixedUpdate();
            if (inputBank.skill1.down || inputBank.skill2.down || inputBank.skill4.down)
            {
                this.duration = 0;
            }
        }

    }
}