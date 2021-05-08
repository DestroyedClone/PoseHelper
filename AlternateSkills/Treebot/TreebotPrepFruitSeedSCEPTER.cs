using System;
using RoR2;
using UnityEngine;
using EntityStates;

namespace AlternateSkills.Treebot
{
    public class TreebotPrepFruitSeedSCEPTER : BaseState
    {
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = this.baseDuration / this.attackSpeedStat;
			Util.PlaySound(this.enterSoundString, base.gameObject);
			base.PlayAnimation(this.animationLayerName, this.animationStateName, this.playbackRateParam, this.duration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration)
			{
				this.outer.SetNextState(new TreebotFireFruitSeedSCEPTER());
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		// Token: 0x040031B5 RID: 12725
		[SerializeField]
		public float baseDuration;

		// Token: 0x040031B6 RID: 12726
		[SerializeField]
		public string enterSoundString;

		// Token: 0x040031B7 RID: 12727
		[SerializeField]
		public string animationLayerName = "Gesture, Additive";

		// Token: 0x040031B8 RID: 12728
		[SerializeField]
		public string animationStateName = "PrepFlower";

		// Token: 0x040031B9 RID: 12729
		[SerializeField]
		public string playbackRateParam = "PrepFlower.playbackRate";

		// Token: 0x040031BA RID: 12730
		private float duration;
	}
}
