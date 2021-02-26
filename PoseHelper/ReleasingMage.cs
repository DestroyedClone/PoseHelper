using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using RoR2.CharacterAI;

namespace PoseHelper
{
    public class ReleasingMage : EntityStates.LockedMage.UnlockingMage
	{
		public static event Action<Interactor> OnOpened;
		public override void OnEnter()
		{
			base.OnEnter();
			EffectManager.SimpleEffect(ReleasingMage.unlockingMageChargeEffectPrefab, base.transform.position, Util.QuaternionSafeLookRotation(Vector3.up), false);
			Util.PlayScaledSound(ReleasingMage.unlockingChargeSFXString, base.gameObject, ReleasingMage.unlockingChargeSFXStringPitch);
			base.GetModelTransform().GetComponent<ChildLocator>().FindChild("Suspension").gameObject.SetActive(false);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= ReleasingMage.unlockingDuration && !this.unlocked)
			{
				base.gameObject.SetActive(false);
				EffectManager.SimpleEffect(ReleasingMage.unlockingMageExplosionEffectPrefab, base.transform.position, Util.QuaternionSafeLookRotation(Vector3.up), false);
				Util.PlayScaledSound(ReleasingMage.unlockingExplosionSFXString, base.gameObject, ReleasingMage.unlockingExplosionSFXStringPitch);
				this.unlocked = true;
				if (NetworkServer.active)
				{
					Action<Interactor> action = ReleasingMage.OnOpened;
					if (action == null)
					{
						return;
					}
					action(base.GetComponent<PurchaseInteraction>().lastActivator);
					SpawnMage();
				}
			}
		}

		public void SpawnMage()
        {
			GameObject mageMasterPrefab = MasterCatalog.FindMasterPrefab("MageMonsterMaster");
			GameObject mageBodyPrefab = mageMasterPrefab.GetComponent<CharacterMaster>().bodyPrefab;

			GameObject mageBodyGameObject = UnityEngine.Object.Instantiate(mageMasterPrefab, gameObject.transform.position, Quaternion.identity);
			CharacterMaster mageCharacterMaster = mageBodyGameObject.GetComponent<CharacterMaster>();
			AIOwnership mageAIOwnership = mageBodyGameObject.GetComponent<AIOwnership>();

			CharacterMaster playerMaster = base.GetComponent<PurchaseInteraction>().lastActivator.gameObject.GetComponent<CharacterBody>().master;
			BaseAI mageBaseAI = gameObject.GetComponent<BaseAI>();
			if (mageCharacterMaster)
			{
				mageCharacterMaster.inventory.GiveItem(ItemIndex.BoostDamage, 10);
				mageCharacterMaster.inventory.GiveItem(ItemIndex.BoostHp, 10);
				GameObject bodyObject = playerMaster.GetBodyObject();
				if (bodyObject)
				{
					Deployable component4 = mageBodyGameObject.GetComponent<Deployable>();
					if (!component4) component4 = mageBodyGameObject.AddComponent<Deployable>();
					playerMaster.AddDeployable(component4, DeployableSlot.ParentAlly);
				}
			}
			if (mageAIOwnership)
			{
				mageAIOwnership.ownerMaster = base.GetComponent<PurchaseInteraction>().lastActivator.gameObject.GetComponent<CharacterBody>().master;
			}
			if (mageBaseAI)
			{
				mageBaseAI.leader.gameObject = base.gameObject;
			}

			NetworkServer.Spawn(mageBodyGameObject);
			mageCharacterMaster.SpawnBody(mageBodyGameObject, gameObject.transform.position, Quaternion.identity);
		}
	}
}
