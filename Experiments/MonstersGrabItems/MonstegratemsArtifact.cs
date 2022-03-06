using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
//using static MonstersGrabItems.BaseUnityPluginInheritedClass;

namespace MonstersGrabItems
{
    class MonstegratemsArtifact : ArtifactBase
    {
		public override string ArtifactName => "Artifact of Monstegratems";
		public override string ArtifactLangTokenName => "ARTIFACT_OF_MONSTEGRATEMS";
		public override string ArtifactDescription => "When enabled, allows monsters to pick up and use items, only dropping them on death." +
			"\nExperimental, very prone to breaking.";
		public override Sprite ArtifactEnabledIcon => null;
		public override Sprite ArtifactDisabledIcon => null;
		public override void Init(ConfigFile config)
		{
			CreateConfig(config);
			CreateLang();
			CreateArtifact();
			Hooks();
		}
		private void CreateConfig(ConfigFile config)
		{
		}
		public override void Hooks()
		{
			On.RoR2.GenericPickupController.BodyHasPickupPermission += GenericPickupController_BodyHasPickupPermission;
			On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
			On.RoR2.GenericPickupController.GrantLunarCoin += GenericPickupController_GrantLunarCoin;
			On.RoR2.GenericPickupController.GrantItem += GenericPickupController_GrantItem;
			On.RoR2.GenericPickupController.OnTriggerStay += GenericPickupController_OnTriggerStay;
		}

		private void GenericPickupController_OnTriggerStay(On.RoR2.GenericPickupController.orig_OnTriggerStay orig, GenericPickupController self, Collider other)
		{
			if (!ArtifactEnabled)
            {
				orig(self, other);
				return;
            }

			if (NetworkServer.active && self.waitStartTime.timeSince >= self.waitDuration && !self.consumed)
			{
				CharacterBody component = other.GetComponent<CharacterBody>();
				if (component)
				{
					PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickupIndex);
					ItemIndex itemIndex = (pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None;
					if (itemIndex != ItemIndex.None && ItemCatalog.GetItemDef(itemIndex).tier == ItemTier.Lunar && component.isPlayerControlled)
					{
						return;
					}
					EquipmentIndex equipmentIndex = (pickupDef != null) ? pickupDef.equipmentIndex : EquipmentIndex.None;
					if (equipmentIndex != EquipmentIndex.None)
					{
						if (EquipmentCatalog.GetEquipmentDef(equipmentIndex).isLunar)
						{
							return;
						}
						if (component.inventory && component.inventory.currentEquipmentIndex != EquipmentIndex.None)
						{
							return;
						}
					}
					if (pickupDef != null && pickupDef.coinValue != 0U && component.isPlayerControlled)
					{
						return;
					}
					if (GenericPickupController.BodyHasPickupPermission(component))
					{
						self.AttemptGrant(component);
					}
				}
			}
		}

		private void GenericPickupController_GrantItem(On.RoR2.GenericPickupController.orig_GrantItem orig, GenericPickupController self, CharacterBody body, Inventory inventory)
		{
			orig(self, body, inventory);
			if (!ArtifactEnabled)
			{
				return;
			}
			PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickupIndex);
			body.master?.GetBodyObject().GetComponent<DropInventoryOnDeath>()?.AddItem((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None, 1);
		}

		private void GenericPickupController_GrantLunarCoin(On.RoR2.GenericPickupController.orig_GrantLunarCoin orig, GenericPickupController self, CharacterBody body, uint count)
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.GenericPickupController::GrantLunarCoin(RoR2.CharacterBody,System.UInt32)' called on client");
				return;
			}
			if (!ArtifactEnabled)
			{
				orig(self, body, count);
				return;
			}
			CharacterMaster master = body.master;
			NetworkUser networkUser = Util.LookUpBodyNetworkUser(body);
			if (networkUser)
			{
				if (master)
				{
					GenericPickupController.SendPickupMessage(master, self.pickupIndex);
				}
				networkUser.AwardLunarCoins(count);
				UnityEngine.Object.Destroy(self.gameObject);
			}
			else
			{
				if (master && master.teamIndex != TeamIndex.Player)
				{
					var component = master.GetBodyObject().GetComponent<DropInventoryOnDeath>();
					if (!component)
						component = master.GetBodyObject().AddComponent<DropInventoryOnDeath>();
					component.incrementCoins();
					GenericPickupController.SendPickupMessage(master, self.pickupIndex);
					UnityEngine.Object.Destroy(self.gameObject);
				}
			}
		}

		private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, GenericPickupController self, CharacterBody body)
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.GenericPickupController::AttemptGrant(RoR2.CharacterBody)' called on client");
				return;
			}
			if (!ArtifactEnabled) return;

			if (!body.masterObject) return;

			TeamComponent component = body.GetComponent<TeamComponent>();
			if (component)
			{
				Inventory inventory = body.inventory;
				if (inventory)
				{
					PlayerCharacterMasterController isPlayer = body.masterObject.GetComponent<PlayerCharacterMasterController>();

					self.consumed = true;
					PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickupIndex);
					DropInventoryOnDeath comp = null;
					if (!isPlayer && body.teamComponent.teamIndex != TeamIndex.Player)
					{
						comp = body.gameObject.GetComponent<DropInventoryOnDeath>();
						if (!comp)
							comp = body.gameObject.AddComponent<DropInventoryOnDeath>();
					}
					if (pickupDef.itemIndex != ItemIndex.None)
					{
						self.GrantItem(body, inventory);
					}
					if (pickupDef.coinValue != 0U)
					{
						self.GrantLunarCoin(body, pickupDef.coinValue);
					}
					if (isPlayer)
					{
						if (pickupDef.equipmentIndex != EquipmentIndex.None)
						{
							self.GrantEquipment(body, inventory);
						}
						if (pickupDef.artifactIndex != ArtifactIndex.None)
						{
							self.GrantArtifact(body, pickupDef.artifactIndex);
						}
					}
				}
			}
		}

		private bool GenericPickupController_BodyHasPickupPermission(On.RoR2.GenericPickupController.orig_BodyHasPickupPermission orig, CharacterBody body)
		{
			if (!ArtifactEnabled) return orig(body);
			return body.masterObject && body.inventory;
		}

		public class DropInventoryOnDeath : MonoBehaviour, IOnKilledServerReceiver
		{
			List<ItemIndex> itemIndices = new List<ItemIndex>();
			EquipmentIndex equipmentIndex = EquipmentIndex.None;
			uint coinCount = 0U;

			public void AddItem(ItemIndex itemIndex, uint amount)
			{
				for (int i = 0; i < amount; i++)
				{
					itemIndices.Add(itemIndex);
				}
			}

			public void incrementCoins()
			{
				coinCount++;
			}

			public void OnKilledServer(DamageReport damageReport)
			{
				Vector3 position = Vector3.zero;
				if (damageReport.attackerBody)
				{
					position = damageReport.victimBody.corePosition;
				}
				else
				{
					foreach (var teammate in TeamComponent.GetTeamMembers(TeamIndex.Player))
					{
						if (teammate.body?.master?.playerCharacterMasterController)
						{
							position = teammate.body.corePosition;
						}
					}
				}

				DropItems(position);

				PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(equipmentIndex), position, Vector3.up * 20f);

				DropCoins(position);
			}

			public void DropItems(Vector3 position)
			{
				if (itemIndices.Count == 0) return;
				float angle = 360f / itemIndices.Count;
				var chestVelocity = (Vector3.up * 20f) + (Vector3.forward * 5f);
				Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
				int i = 0;
				while (i < itemIndices.Count)
				{
					PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemIndices[i]), position + Vector3.up * 1.5f, chestVelocity);
					i++;
					chestVelocity = rotation * chestVelocity;
				}
			}

			public void DropCoins(Vector3 position)
			{
				if (coinCount == 0) return;
				float angle = 360f / coinCount;
				var chestVelocity = (Vector3.up * 20f) + (Vector3.forward * 5f);
				Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
				int i = 0;
				while (i < coinCount)
				{
					PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex("LunarCoin.Coin0"), position + Vector3.up * 1.5f, chestVelocity);
					i++;
					chestVelocity = rotation * chestVelocity;
				}
			}
		}
	}
}
