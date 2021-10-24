using RoR2;
using System;
using System.Collections.ObjectModel;
using UnityEngine;
using static BetterEquipmentDroneUse.Main;

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
                        //Debug.Log("interactable check 1");
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
                        //Debug.Log("debuffs!");
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

        public static GameObject GetPriorityTarget(TeamIndex viewerTeamIndex)
        {
            ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
            int i = 0;
            int count = readOnlyInstancesList.Count;
            GameObject target = null;
            int highestPriority = 0;
            while (i < count)
            {
                CharacterMaster characterMaster = readOnlyInstancesList[i];
                if (characterMaster.teamIndex != viewerTeamIndex && characterMaster.hasBody && characterMaster.GetBody().healthComponent && characterMaster.GetBody().healthComponent.alive)
                {
                    int priority = 0;
                    var body = characterMaster.GetBody();
                    if (body.healthComponent.godMode)
                    {
                        continue;
                    }

                    if (body.isBoss)
                    {
                        priority += 3;
                    }
                    if (body.isChampion)
                    {
                        priority += 1;
                    }
                    if (body.isElite)
                    {
                        priority += 1;
                    }
                    if (body.isGlass)
                    {
                        priority += 2;
                    }
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        target = body.gameObject;
                    }
                }
                i++;
            }

            return target;
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


        public static DroneMode EvaluateDroneMode(EquipmentIndex equipmentIndex)
        {
            //_logger.LogMessage($"Trying out EquipmentIndex {equipmentIndex}");
            DroneMode droneMode;
            if (DroneModeDictionary.TryGetValue(equipmentIndex, out DroneMode newDroneMode))
            {
                droneMode = newDroneMode;
            }
            else
            {
                droneMode = DroneMode.None;
            }
            return droneMode;
        }
    }
}