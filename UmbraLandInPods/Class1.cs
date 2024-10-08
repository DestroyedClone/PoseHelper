﻿using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace UmbraSurvivorPods
{
    [BepInPlugin("com.DestroyedClone.UmbraSurvivorPods", "Umbra Survivor Pods", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public static ConfigEntry<float> cfgDestroyTimer;

        public void Start()
        {
            cfgDestroyTimer = Config.Bind("", "Umbra Survivor Pod Delete Delay", 10f, "The amount of time, in seconds, that the game will wait before deleting the pod.");

            On.RoR2.CombatSquad.AddMember += CombatSquad_AddMember;
            On.EntityStates.SurvivorPod.Landed.OnEnter += ExitIfUmbra;
            //RoR2.VehicleSeat.onPassengerExitGlobal += VehicleSeat_onPassengerExitGlobal;
            On.EntityStates.SurvivorPod.ReleaseFinished.OnEnter += ReleaseFinished_OnEnter;
            On.EntityStates.SurvivorPod.Release.OnEnter += Release_OnEnter;
        }

        private void Release_OnEnter(On.EntityStates.SurvivorPod.Release.orig_OnEnter orig, EntityStates.SurvivorPod.Release self)
        {
            if (self.vehicleSeat && self.vehicleSeat.currentPassengerBody && IsUmbra(self.vehicleSeat.currentPassengerBody))
            {
                var dot = self.gameObject.GetComponent<DestroyOnTimer>();
                if (!dot)
                {
                    dot = self.gameObject.AddComponent<DestroyOnTimer>();
                }
                dot.duration = cfgDestroyTimer.Value;
                dot.enabled = false;
            }
            orig(self);
        }

        private void ReleaseFinished_OnEnter(On.EntityStates.SurvivorPod.ReleaseFinished.orig_OnEnter orig, EntityStates.SurvivorPod.ReleaseFinished self)
        {
            orig(self);
            var dot = self.gameObject.GetComponent<DestroyOnTimer>();
            if (dot)
            {
                dot.enabled = true;
            }
        }

        private void CombatSquad_AddMember(On.RoR2.CombatSquad.orig_AddMember orig, CombatSquad self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);
            if (IsUmbra(memberMaster) && self.gameObject.name.StartsWith("ShadowCloneEncounter"))
            {
                Transform playerSpawnTransform = Stage.instance.GetPlayerSpawnTransform();
                var body = memberMaster.GetBody();
                Vector3 vector = body.footPosition;
                Quaternion quaternion = Quaternion.identity;
                if (playerSpawnTransform)
                {
                    vector = playerSpawnTransform.position;
                    quaternion = playerSpawnTransform.rotation;
                }
                TeleportHelper.TeleportBody(body, vector);
                Run.instance.HandlePlayerFirstEntryAnimation(body, vector, quaternion);
                //if (memberMaster.bodyPrefab.GetComponent<CharacterBody>()?.preferredPodPrefab != null)
            }
        }

        private bool IsUmbra(CharacterMaster characterMaster)
        {
            return characterMaster && characterMaster.inventory && characterMaster.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0;
        }

        private bool IsUmbra(CharacterBody characterBody)
        {
            return characterBody && IsUmbra(characterBody.master);
        }

        private void VehicleSeat_onPassengerExitGlobal(VehicleSeat vehicleSeat, GameObject passenger)
        {
            var characterbody = passenger.GetComponent<CharacterBody>();
            if (IsUmbra(characterbody))
            {
                characterbody.master?.GetComponent<RoR2.CharacterAI.BaseAI>()?.ForceAcquireNearestEnemyIfNoCurrentEnemy();
            }
        } // maybe?

        private void ExitIfUmbra(On.EntityStates.SurvivorPod.Landed.orig_OnEnter orig, EntityStates.SurvivorPod.Landed self)
        {
            orig(self);
            if (self.vehicleSeat)
            {
                var currentPassenger = self.vehicleSeat.currentPassengerBody;
                if (IsUmbra(currentPassenger))
                {
                    bool? a = true;
                    self.HandleVehicleExitRequest(gameObject, ref a);
                }
            }
        }
    }
}