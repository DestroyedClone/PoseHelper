using System;
using RoR2;
using UnityEngine;
using EntityStates;

namespace ROR1AltSkills.Loader
{
    public class PlaceConduit1 : BaseSkillState
    {
		public GameObject conduitA;

		public override void OnEnter()
		{
			base.OnEnter();
			conduitA = PlaceRod();
			this.outer.SetNextState(this.GetNextStateAuthority());
		}

		public GameObject PlaceRod()
        {
			var conduit = UnityEngine.Object.Instantiate<GameObject>(LoaderMain.ConduitPrefab);
			conduit.transform.position = outer.commonComponents.characterBody.corePosition;
			PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.ArmorPlate.itemIndex), conduit.transform.position, Vector3.up);
			return conduit;
        }

		protected virtual EntityState GetNextStateAuthority()
		{
			return new PlaceConduit2
			{
				conduitA = conduitA
			};
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
	}
}
