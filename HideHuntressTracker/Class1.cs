using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HideHuntressTracker
{
    [BepInPlugin("com.DestroyedClone.HideHuntressTracker", "HideHuntressTracker", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public void Start()
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