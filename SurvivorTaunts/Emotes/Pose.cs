using System;
using System.Collections.Generic;
using System.Text;
using static RoR2.RoR2Content;
using UnityEngine;
using RoR2;

namespace SurvivorTaunts.Emotes
{
    public class Pose : BaseEmote
    {
        STPlugin.SurvivorTauntController survivorTauntController;
        RuntimeAnimatorController cachedRuntimeAnimatorController;
        public override void OnEnter()
        {
            survivorTauntController = this.characterBody.GetComponent<STPlugin.SurvivorTauntController>();
            var survivorDef = survivorTauntController.survivorDef;
            cachedRuntimeAnimatorController = base.GetModelAnimator().runtimeAnimatorController;

            if (survivorDef == Survivors.Bandit2)
            {

            } else if (survivorDef == Survivors.Captain)
            {

            } else if (survivorDef == Survivors.Commando)
            {
                base.GetModelAnimator().runtimeAnimatorController = STPlugin.introAnimatorController;
            }
            this.animString = "";
            this.animDuration = 0.75f;
            this.soundString = "";
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.GetModelAnimator().runtimeAnimatorController = cachedRuntimeAnimatorController;
            base.OnExit();

        }
    }
}
