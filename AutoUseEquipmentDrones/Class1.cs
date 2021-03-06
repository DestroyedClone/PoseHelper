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

namespace AutoUseEquipmentDrones
{
    [BepInPlugin("com.DestroyedClone.BetterEquipmentDroneUse", "Better Equipment Drone Use", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class AUEDPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<string> TargetPriority { get; set; }
        public int EquipmentDroneBodyIndex = -1;
        public Type[] allowedTypesToScan = new Type[] { };
        public void Awake()
        {
            On.RoR2.EquipmentSlot.FixedUpdate += EquipmentSlot_FixedUpdate;
            var body = Resources.Load<GameObject>("prefabs/characterbodies/EquipmentDroneBody");
            EquipmentDroneBodyIndex = body.GetComponent<CharacterBody>().bodyIndex;
            On.RoR2.ChestRevealer.Init += GetAllowedTypes;


        }

        private void GetAllowedTypes(On.RoR2.ChestRevealer.orig_Init orig)
        {
            orig();
            allowedTypesToScan = ChestRevealer.typesToCheck;
        }

        private void EquipmentSlot_FixedUpdate(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, EquipmentSlot self)
        {
            orig(self);
            TeamIndex enemyTeamIndex = self.teamComponent.teamIndex == TeamIndex.Player ? TeamIndex.Monster : TeamIndex.Player;
            bool forceActive = false;

            switch (self.equipmentIndex)
            {
                // Enemy On Map
                // If there are enemies alive, use.
                case CommandMissile:
                case Meteor:
                    forceActive = CheckForEnemies(enemyTeamIndex);
                    break;
                // Priority Target
                // Prioritizes a certain enemy rather than firing blindly
                // Overrides BaseAI
                case Blackhole:
                case BFG:
                case Lightning:
                case CrippleWard:
                    break;
                // Evade or Aggro
                // Attempts to draw enemy attention
                case Jetpack:
                    break;
                case GainArmor:
                    break;
                case Tonic:
                    break;
                //Custom Logic
                case GoldGat: //Forced Equipment State
                    break;
                case PassiveHealing: //Target damaged ally
                    break;
                case Gateway: // Target Interactables or nearby if damaged
                    break;
                case Cleanse: //Prioritize projectiles
                    break;
                case Saw: //get close
                    break;
                case Recycle: //look at polyp
                    break;

                //Health Requirement
                case Fruit:
                    break;
                //Chase Priority
                case BurnNearby:
                case QuestVolatileBattery:
                    break;
                //(FireBallDash)

                //Valid interactables
                case Scanner:
                    if (CheckForInteractables())
                    {
                        forceActive = true;
                    }
                    break;
            }

            if (forceActive)
            {
                ActivateWhenReady(self);
            }
        }

        private bool CheckForEnemies(TeamIndex teamIndex)
        {
            ReadOnlyCollection<TeamComponent> teamComponents = TeamComponent.GetTeamMembers(teamIndex);
            return teamComponents.Count > 0;
        }

        private void ActivateWhenReady(EquipmentSlot equipmentSlot)
        {
            //equipmentslot L299
            bool isEquipmentActivationAllowed = equipmentSlot.characterBody.isEquipmentActivationAllowed;
            if (isEquipmentActivationAllowed && equipmentSlot.hasEffectiveAuthority)
            {
                if (NetworkServer.active)
                {
                    equipmentSlot.ExecuteIfReady();
                    return;
                }
                equipmentSlot.CallCmdExecuteIfReady();
            }
        }

        private bool CheckForInteractables()
        {
            Type[] validInteractables = new Type[] {  };
            foreach (var valid in validInteractables)
            {
                InstanceTracker.FindInstancesEnumerable(valid);
                if (((IInteractable)valid).ShouldShowOnScanner())
                {
                    return true;
                }
            }
            return false;
        }

        private void ForceEquipmentUse(BaseAI baseAI)
        {
            baseAI.bodyInputBank.activateEquipment.PushState(true);
        }

    }
}
