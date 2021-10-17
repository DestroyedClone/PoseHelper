using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.RoR2Content.Equipment;
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
using static BetterEquipmentDroneUse.Methods;
using EntityStates.AI;
using static AutoUseEquipmentDrones.AUEDPlugin;

namespace BetterEquipmentDroneUse
{
    public class Methods
    { 
        public static bool CheckForValidInteractables()
        {
            Type[] array = ChestRevealer.typesToCheck;
            for (int i = 0; i < array.Length; i++)
            {
                foreach (MonoBehaviour monoBehaviour in InstanceTracker.FindInstancesEnumerable(array[i]))
                {
                    if (((IInteractable)monoBehaviour).ShouldShowOnScanner())
                    {
                        Debug.Log("interactable check 1");
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckForValidInteractables2()
        {
            ///Type[] validInteractables = new Type[] {  };
            foreach (var valid in allowedTypesToScan)
            {
                InstanceTracker.FindInstancesEnumerable(valid);
                if (((IInteractable)valid).ShouldShowOnScanner())
                {
                    Debug.Log("interactable check 2");
                    return true;
                }
            }
            return false;
        }

        public static bool CheckForDebuffs(CharacterBody characterBody)
        {
            BuffIndex buffIndex = 0;
            BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
            while (buffIndex < buffCount)
            {
                BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                if (buffDef.isDebuff && characterBody.HasBuff(buffIndex))
                {
                    if (characterBody.HasBuff(buffIndex))
                    {
                        Debug.Log("debuffs!");
                        return true;
                    }
                }
                buffIndex++;
            }
            Debug.Log("no debuffs?!");
            return false;
        }

        //Util.GetItemCountForTea,
        public static bool CheckForAliveOnTeam(TeamIndex teamIndex, bool requiresAlive = true, bool requiresConnected = false)
        {
            ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
            int i = 0;
            int count = readOnlyInstancesList.Count;
            while (i < count)
            {
                CharacterMaster characterMaster = readOnlyInstancesList[i];
                if (characterMaster.teamIndex == teamIndex && (!requiresAlive || characterMaster.hasBody) && (!requiresConnected || !characterMaster.playerCharacterMasterController || characterMaster.playerCharacterMasterController.isConnected))
                {
                    return true;
                }
                i++;
            }
            return false;
        }

        public static GameObject GetMostHurtTeam(TeamIndex teamIndex)
        {
            ReadOnlyCollection<TeamComponent> teamComponents = TeamComponent.GetTeamMembers(teamIndex);
            //Dictionary<TeamComponent, float> keyValuePairs = new Dictionary<TeamComponent, float>();

            var lowestHealthFraction = 1f;
            GameObject lowestHealthObject = null;
            foreach (var ally in teamComponents)
            {
                if (ally.body?.healthComponent)
                {
                    //keyValuePairs.Add(ally, ally.body.healthComponent.health / ally.body.healthComponent.fullHealth);
                    var calculatedHealthFraction = ally.body.healthComponent.health / ally.body.healthComponent.fullHealth;
                    if (calculatedHealthFraction < lowestHealthFraction)
                    {
                        lowestHealthFraction = calculatedHealthFraction;
                        lowestHealthObject = ally.body.gameObject;
                    }
                }
            }
            return lowestHealthObject;

            // https://stackoverflow.com/questions/23734686/c-sharp-dictionary-get-the-key-of-the-min-value
            /*if (keyValuePairs.Count > 0)
            {
                var min = keyValuePairs.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
                return min.body.gameObject;
            }*/




            return null;
        }
    }
}
