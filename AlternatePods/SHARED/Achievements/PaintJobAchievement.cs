using R2API;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using RoR2.Achievements;

namespace AlternatePods
{
    public class PaintJobAchievement : BaseAchievement
    {
        public override void OnBodyRequirementMet()
		{
			base.OnBodyRequirementMet();
			Run.onClientGameOverGlobal += this.OnClientGameOverGlobal;
		}

		public override void OnBodyRequirementBroken()
		{
			Run.onClientGameOverGlobal -= this.OnClientGameOverGlobal;
			base.OnBodyRequirementBroken();
		}

		private void OnClientGameOverGlobal(Run run, RunReport runReport)
		{
			if (!runReport.gameEnding)
			{
				return;
			}
			if (runReport.gameEnding.isWin)
			{
				DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(runReport.ruleBook.FindDifficulty());
				if (difficultyDef != null && difficultyDef.countsAsHardMode)
				{
					base.Grant();
				}
			}
		}
    }
}