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
            animator = this.GetModelAnimator();
            runtimeAnimator = animator.runtimeAnimatorController;
            survivorTauntController = this.characterBody.GetComponent<STPlugin.SurvivorTauntController>();
            base.OnEnter();
            animator.runtimeAnimatorController = SurvivorTaunts.Modules.Prefabs.runtimeAnimatorControllers[(int)survivorTauntController.survivorIndex];
        }

        public override void OnExit()
        {
            base.OnExit();

        }
    }
}
