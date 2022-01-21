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
    [BepInPlugin("com.DestroyedClone.HideHuntressTracker", "HideHuntressTracker", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public void Start()
        {
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.baseNameToken == "HUNTRESS_BODY_NAME")
            {
                var com = obj.gameObject.AddComponent<HideHuntressTrackerComp>();
                com.inventory = obj.inventory;
                com.huntressTracker = obj.GetComponent<HuntressTracker>();
            }
        }

        public class HideHuntressTrackerComp : MonoBehaviour
        {
            public Inventory inventory;
            public HuntressTracker huntressTracker;

            public void Start()
            {
                if (!inventory)
                {
                    inventory = gameObject.GetComponent<CharacterBody>().inventory;
                }
                if (!huntressTracker)
                {
                    huntressTracker = gameObject.GetComponent<HuntressTracker>();
                }

                inventory.onInventoryChanged += Inventory_onInventoryChanged;
            }

            private void Inventory_onInventoryChanged()
            {
                huntressTracker.enabled = !(inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) > 0
                    && inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement) > 0);
            }
        }
    }
}