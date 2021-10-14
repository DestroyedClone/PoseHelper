using System;
using System.Collections.Generic;
using System.Text;
using BetterEquipmentDroneUse;
using RoR2;
using UnityEngine;
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
    }
}
