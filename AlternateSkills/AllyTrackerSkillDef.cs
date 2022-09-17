using System;
using JetBrains.Annotations;
using UnityEngine;
using RoR2;
using RoR2.Skills;

namespace AlternateSkills
{
	public class AllyTrackingSkillDef : SkillDef
	{
		public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
		{
			return new AllyTrackingSkillDef.InstanceData
			{
				allyTracker = skillSlot.GetComponent<AllyTracker>()
			};
		}

		private static bool HasTarget([NotNull] GenericSkill skillSlot)
		{
			AllyTracker allyTracker = ((AllyTrackingSkillDef.InstanceData)skillSlot.skillInstanceData).allyTracker;
			return (allyTracker != null) ? allyTracker.GetTrackingTarget() : null;
		}

		public override bool CanExecute([NotNull] GenericSkill skillSlot)
		{
			return AllyTrackingSkillDef.HasTarget(skillSlot) && base.CanExecute(skillSlot);
		}

		public override bool IsReady([NotNull] GenericSkill skillSlot)
		{
			return base.IsReady(skillSlot) && AllyTrackingSkillDef.HasTarget(skillSlot);
		}

		protected class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public AllyTracker allyTracker;
		}
	}
}
