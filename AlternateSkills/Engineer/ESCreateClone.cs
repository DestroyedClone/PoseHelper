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

namespace AlternateSkills.Engi
{
	public class ESCreateClone : BaseSkillState
    {
        // 1. Target an ally
        // 1b. Just do proximity for now
        // 2. Summon clone
        // 3. Modify, add Mechanical tag if missing.
        // 4. Exit.


    }
}