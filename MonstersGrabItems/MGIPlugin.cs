using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.EquipmentIndex;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EntityStates;
using JetBrains.Annotations;
using RoR2.Navigation;
using UnityEngine.AI;
using EntityStates.GoldGat;
using System.Security;
using System.Security.Permissions;

using EntityStates.AI;
using static RoR2.RoR2Content;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MonstersGrabItems
{
    [BepInPlugin("com.DestroyedClone.MonstersGrabItems", "Monsters Grab Items", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class MGI_Plugin : BaseUnityPlugin
    {
		public static EquipmentIndex[] desirableEquipment = { };

        void Awake()
        {
            On.RoR2.GenericPickupController.BodyHasPickupPermission += GenericPickupController_BodyHasPickupPermission;
            On.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
            On.RoR2.GenericPickupController.GrantItem += GenericPickupController_GrantItem;
            On.RoR2.EquipmentCatalog.SetEquipmentDefs += EquipmentCatalog_SetEquipmentDefs;
        }

        private void GenericPickupController_GrantItem(On.RoR2.GenericPickupController.orig_GrantItem orig, GenericPickupController self, CharacterBody body, Inventory inventory)
        {
			if (body.master.playerCharacterMasterController)
            {
				orig(self, body, inventory);
				return;
            }
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.GenericPickupController::GrantItem(RoR2.CharacterBody,RoR2.Inventory)' called on client");
				return;
			}
			PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickupIndex);
			inventory.GiveItem((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None, 1);
			GenericPickupController.SendPickupMessage(inventory.GetComponent<CharacterMaster>(), self.pickupIndex);
			UnityEngine.Object.Destroy(base.gameObject);
		}

        private void EquipmentCatalog_SetEquipmentDefs(On.RoR2.EquipmentCatalog.orig_SetEquipmentDefs orig, EquipmentDef[] newEquipmentDefs)
        {
			orig(newEquipmentDefs);
			EquipmentIndex[] array =
		{
			Equipment.BFG.equipmentIndex,
			Equipment.Blackhole.equipmentIndex,
			Equipment.CommandMissile.equipmentIndex,
			Equipment.CritOnUse.equipmentIndex,
			Equipment.DroneBackup.equipmentIndex,
			Equipment.FireBallDash.equipmentIndex,
			Equipment.Fruit.equipmentIndex,
			Equipment.GainArmor.equipmentIndex,
			Equipment.LifestealOnHit.equipmentIndex,
			Equipment.Lightning.equipmentIndex,
			Equipment.Meteor.equipmentIndex,
			Equipment.QuestVolatileBattery.equipmentIndex,
			Equipment.Recycle.equipmentIndex,
			Equipment.Saw.equipmentIndex,
			Equipment.TeamWarCry.equipmentIndex
		};
			desirableEquipment = array;
		}

        private void GenericPickupController_AttemptGrant(On.RoR2.GenericPickupController.orig_AttemptGrant orig, GenericPickupController self, CharacterBody body)
        {
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.GenericPickupController::AttemptGrant(RoR2.CharacterBody)' called on client");
				return;
			}

			TeamComponent component = body.GetComponent<TeamComponent>();
			if (component)
			{
				Inventory inventory = body.inventory;
				if (inventory)
				{
					PlayerCharacterMasterController isPlayer = body.masterObject.GetComponent<PlayerCharacterMasterController>();

					self.consumed = true;
					PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickupIndex);
					if (pickupDef.itemIndex != ItemIndex.None)
					{
						self.GrantItem(body, inventory);
					}
					if (pickupDef.equipmentIndex != EquipmentIndex.None)
					{
						if ((desirableEquipment.Contains(inventory.currentEquipmentIndex) && !isPlayer) || isPlayer)
							self.GrantEquipment(body, inventory);
					}
					if (isPlayer)
					{
						if (pickupDef.artifactIndex != ArtifactIndex.None)
						{
							self.GrantArtifact(body, pickupDef.artifactIndex);
						}
						if (pickupDef.coinValue != 0U)
						{
							self.GrantLunarCoin(body, pickupDef.coinValue);
						}
					}
				}
			}
		}

        private bool GenericPickupController_BodyHasPickupPermission(On.RoR2.GenericPickupController.orig_BodyHasPickupPermission orig, CharacterBody body)
        {
            return body.masterObject && body.inventory;
        }

		public class DropInventoryOnDeath : MonoBehaviour, IOnKilledServerReceiver
        {
			List<ItemIndex> itemIndices = new List<ItemIndex>();
			EquipmentIndex equipmentIndex = EquipmentIndex.None;

			public void AddItem(ItemIndex itemIndex, uint amount)
            {

            }



			public void OnKilledServer(DamageReport damageReport)
			{
				Vector3 position = Vector3.zero;
				if (damageReport.attackerBody)
                {
					position = damageReport.victimBody.corePosition;
                } else
				{
					foreach (var teammate in TeamComponent.GetTeamMembers(TeamIndex.Player))
                    {
						if (teammate.body?.master?.playerCharacterMasterController)
                        {
							position = teammate.body.corePosition;
                        }
                    }
				}

				foreach (var itemIndex in itemIndices)
				{
					PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemIndex), position, Vector3.up * 20f);
				}

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
        }
    }
}
