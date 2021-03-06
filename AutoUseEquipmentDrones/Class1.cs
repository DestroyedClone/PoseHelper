using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.EquipmentIndex;
using System.Collections.ObjectModel;
using UnityEngine.Networking;

namespace AutoUseEquipmentDrones
{
    [BepInPlugin("com.DestroyedClone.BetterEquipmentDroneUse", "Better Equipment Drone Use", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class AUEDPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<string> BannedIDS { get; set; }
        public void Awake()
        {
            On.RoR2.EquipmentSlot.FixedUpdate += EquipmentSlot_FixedUpdate;
        }

        private void EquipmentSlot_FixedUpdate(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, EquipmentSlot self)
        {
            orig(self);
            TeamIndex enemyTeamIndex = self.teamComponent.teamIndex == TeamIndex.Player ? TeamIndex.Monster : TeamIndex.Player;


            switch (self.equipmentIndex)
            {
                // Enemy On Map
                // If there are enemies alive, use.
                case CommandMissile:
                case Meteor:
                    if (CheckForEnemies(enemyTeamIndex))
                    {
                        ActivateWhenReady(self);
                    }
                    break;
                //Priority Target
                case Blackhole:
                case BFG:
                case Lightning:
                case CrippleWard:
                    break;
                //Evade or Aggro
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
                case Gateway:
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
                //Chase
                case BurnNearby:
                case QuestVolatileBattery:
                    break;
                //(FireBallDash)

                //Valid interactables
                case Scanner:
                    break;
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
    }
}
