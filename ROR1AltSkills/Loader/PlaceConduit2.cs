using System;
using RoR2;
using UnityEngine;
using EntityStates;

namespace ROR1AltSkills.Loader
{
    public class PlaceConduit2 : BaseSkillState
    {
		public GameObject conduitA;

		public override void OnEnter()
		{
			base.OnEnter();
		}

        public override void FixedUpdate()
        {
            base.FixedUpdate();
			if (base.IsKeyDownAuthority())
			{
				PlaceRod();
				this.outer.SetNextStateToMain();
			}
        }


        public GameObject PlaceRod()
		{
			var conduit = UnityEngine.Object.Instantiate<GameObject>(LoaderMain.ConduitPrefab);
			conduit.transform.position = outer.commonComponents.characterBody.corePosition;
			PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.ArmorPlate.itemIndex), conduit.transform.position, Vector3.up);
			var component = conduit.AddComponent<ConduitController>();
			component.conduitA = conduitA;
			component.conduitB = conduit;
			return conduit;
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
	}
}
