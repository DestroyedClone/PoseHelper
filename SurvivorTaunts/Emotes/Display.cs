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
        RuntimeAnimatorController cachedRuntimeAnimator;
        Animator animator;
        STPlugin.SurvivorTauntController survivorTauntController;

        public override void OnEnter()
        {
            Debug.Log("Entered");
            animator = this.GetModelAnimator();

            cachedRuntimeAnimator = animator.runtimeAnimatorController;
            survivorTauntController = this.characterBody.GetComponent<STPlugin.SurvivorTauntController>();

            base.OnEnter();

            Modules.Prefabs.survivorDef_to_animationController.TryGetValue(survivorTauntController.survivorDef, out RuntimeAnimatorController runtimeAnimator);
            animator.runtimeAnimatorController = runtimeAnimator;
            Modules.Prefabs.survivorDef_to_gameObject.TryGetValue(survivorTauntController.survivorDef, out GameObject displayPrefab);

            // virtuals
            animString = "Spawn";
            animDuration = 0.75f;

        }

        public override void OnExit()
        {
            animator.runtimeAnimatorController = cachedRuntimeAnimator;
            base.OnExit();

        }
    }
}
