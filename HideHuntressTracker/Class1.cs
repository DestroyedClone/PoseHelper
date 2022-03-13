using BepInEx;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace HideHuntressTracker
{
    [BepInPlugin("com.DestroyedClone.HideHuntressTracker", "HideHuntressTracker", "1.0.2")]
    public class Class1 : BaseUnityPlugin
    {

        [RoR2.SystemInitializer(dependencies: new Type[] {typeof(RoR2.BodyCatalog), typeof(SurvivorCatalog)})]
        public static void SetupHuntress()
        {
            var prefab = RoR2Content.Survivors.Huntress.bodyPrefab;
            var com = prefab.AddComponent<HideHuntressTrackerComp>();
            com.characterBody = prefab.GetComponent<CharacterBody>();
            com.inventory = prefab.GetComponent<CharacterBody>().inventory;
            com.huntressTracker = prefab.GetComponent<HuntressTracker>();
        }


        public class HideHuntressTrackerComp : MonoBehaviour
        {
            public CharacterBody characterBody;
            public Inventory inventory;
            public HuntressTracker huntressTracker;

            public void Start()
            {
                if (!characterBody)
                {
                    characterBody = gameObject.GetComponent<CharacterBody>();
                }
                if (!inventory)
                {
                    inventory = characterBody.inventory;
                }
                if (!huntressTracker)
                {
                    huntressTracker = gameObject.GetComponent<HuntressTracker>();
                }

                inventory.onInventoryChanged += Inventory_onInventoryChanged;
            }

            private void Inventory_onInventoryChanged()
            {
                bool hasPrimary = inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) > 0;
                bool hasSecondary = inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement) > 0;

                if (huntressTracker)
                {
                    huntressTracker.enabled = !(hasPrimary && hasSecondary);
                }

                //huntressTracker.enabled = !(inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) > 0
                //&& inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement) > 0);
            }
        }
    }
}