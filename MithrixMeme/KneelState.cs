using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using RoR2.CharacterAI;
using static EntityStates.BrotherMonster.FistSlam;

namespace MithrixMeme
{
    public class KneelState : BaseState
    {
        float stopwatch = 0f;
        bool hasKnelt = false;
        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayCrossfade("FullBody Override", "FistSlam", "FistSlam.playbackRate", baseDuration, 0.1f);
        }

        public override void FixedUpdate()
		{
			base.FixedUpdate();
            stopwatch += Time.deltaTime;
            if (stopwatch > 3f)
            {
                if (hasKnelt)
                {
                    return;
                }
                hasKnelt = true;
                base.GetModelAnimator().speed = 0f;
                ReturnStolenItemsOnGettingHit component = base.GetComponent<ReturnStolenItemsOnGettingHit>();
                if (component && component.itemStealController)
                {
                    EntityState.Destroy(component.itemStealController.gameObject);
                }
            }
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
