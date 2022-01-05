using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Artifacts;
using static RoR2.Artifacts.DoppelgangerInvasionManager;
using static RoR2.Artifacts.DoppelgangerSpawnCard;
using System;
using RoR2.Navigation;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace UmbraLandInPods
{
    [BepInPlugin("com.DestroyedClone.UmbraLandInPods", "Umbra Land In Pods", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.Artifacts.DoppelgangerInvasionManager.CreateDoppelganger += DoppelgangerInvasionManager_CreateDoppelganger;
            On.EntityStates.SurvivorPod.Landed.OnEnter += ExitIfUmbra;
        }

        private void ExitIfUmbra(On.EntityStates.SurvivorPod.Landed.orig_OnEnter orig, EntityStates.SurvivorPod.Landed self)
        {
			orig(self);
			if (self.vehicleSeat)
            {
				var currentPassenger = self.vehicleSeat.currentPassengerBody;
				if (currentPassenger && !currentPassenger.isPlayerControlled && currentPassenger.inventory && currentPassenger.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
                {
					bool? a = true;
					self.HandleVehicleExitRequest(gameObject, ref a);
                }
			}
        }

        private void DoppelgangerInvasionManager_CreateDoppelganger(On.RoR2.Artifacts.DoppelgangerInvasionManager.orig_CreateDoppelganger orig, CharacterMaster srcCharacterMaster, Xoroshiro128Plus rng)
        {
			SpawnCard spawnCard = DoppelgangerSpawnCard.FromMaster(srcCharacterMaster);
			if (!spawnCard)
			{
				return;
			}
			Transform playerSpawnTransform = Stage.instance.GetPlayerSpawnTransform();
			DirectorCore.MonsterSpawnDistance input;
			if (TeleporterInteraction.instance)
			{
				input = DirectorCore.MonsterSpawnDistance.Close;
			}
			else
			{
				input = DirectorCore.MonsterSpawnDistance.Far;
			}
			DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
			{
				spawnOnTarget = playerSpawnTransform,
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode
			};
			DirectorCore.GetMonsterSpawnDistance(input, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, rng);
			directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Monster);
			directorSpawnRequest.ignoreTeamMemberLimit = true;
			CombatSquad combatSquad = null;
			DirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
			directorSpawnRequest2.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest2.onSpawnedServer, new Action<SpawnCard.SpawnResult>(delegate (SpawnCard.SpawnResult result)
			{
				if (!combatSquad)
				{
					combatSquad = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/ShadowCloneEncounter")).GetComponent<CombatSquad>();
				}
				var spawnedInstance = result.spawnedInstance.GetComponent<CharacterMaster>();
				combatSquad.AddMember(spawnedInstance);

				Vector3 vector = Vector3.zero;
				Quaternion quaternion = Quaternion.identity;
				if (playerSpawnTransform)
				{
					vector = playerSpawnTransform.position;
					quaternion = playerSpawnTransform.rotation;
				}

				var body = spawnedInstance.GetBody();
				if (body.preferredPodPrefab)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(body.preferredPodPrefab, body.transform.position, quaternion);
					gameObject.GetComponent<VehicleSeat>().AssignPassenger(body.gameObject);
					NetworkServer.Spawn(gameObject);
				}
			}));
			var spawnRequest = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
			if (combatSquad)
			{
				NetworkServer.Spawn(combatSquad.gameObject);
			}
			UnityEngine.Object.Destroy(spawnCard);
		}
    }
}
