using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using EntityStates;

namespace SurvivorTaunts.Emotes
{
    public class Display : BaseEmote
    {
        RuntimeAnimatorController runtimeAnimator;
        Animator animator;
        STPlugin.SurvivorTauntController survivorTauntController;

        public override void OnEnter()
        {
            this.animDuration = 0.75f;
            animator = this.GetModelAnimator();
            runtimeAnimator = animator.runtimeAnimatorController;
            survivorTauntController = this.characterBody.GetComponent<STPlugin.SurvivorTauntController>();
            base.OnEnter();
            var index = (int)survivorTauntController.survivorIndex;
            Debug.Log("using survivorindex: " + index);
            animator.runtimeAnimatorController = SurvivorTaunts.Modules.Prefabs.runtimeAnimatorControllers[index];
        }

        public override void OnExit()
        {
            base.OnExit();

        }
    }
}
