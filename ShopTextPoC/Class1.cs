using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using static RoR2.RoR2Content;

namespace ShopTextPoC
{
    [BepInPlugin("com.DestroyedClone.RadarEffectToggle", "Radar Effect Toggle", "1.1.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.BarrelInteraction.OnInteractionBegin += BarrelInteraction_OnInteractionBegin;
        }

        private void BarrelInteraction_OnInteractionBegin(On.RoR2.BarrelInteraction.orig_OnInteractionBegin orig, BarrelInteraction self, Interactor activator)
        {
            orig(self, activator);
            var comp = self.gameObject.AddComponent<ShopTalker>();
            comp.StartConvo();
            comp.currentUser = activator;

        }

        public class ShopTalker : MonoBehaviour
        {
            bool isListening = false;
            string region = "Ask";
            public Interactor currentUser = null;
            public Inventory inventory = null;
            public ItemDef[] requiredItems =
            {
                Items.AlienHead,
                Items.ArmorPlate,
                Items.ArtifactKey
            };

            public void StartConvo()
            {
                Say("a. Give me the fucking bullet\n" +
                "b. fuck off");
                inventory = currentUser.gameObject.GetComponent<CharacterBody>()?.inventory;
                if (!inventory)
                {
                    Say("cock");
                }
                isListening = true;
            }

            public void Say(string t)
            {
                Chat.AddMessage(t);
            }

            public bool HasItems(ItemDef[] itemDefs)
            {
                if (!inventory) return false;
                foreach (var itemDef in itemDefs)
                {
                    if (inventory.GetItemCount(itemDef) <= 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            public void RemoveItems(ItemDef[] itemDefs)
            {
                foreach (var itemDef in itemDefs)
                {
                    inventory.RemoveItem(itemDef);
                }
            }

            public void Update()
            {
                if (!isListening || !currentUser)
                    return;

                if (Input.GetKey("a"))
                {
                    if (currentUser && HasItems(requiredItems))
                    {
                        Say("Here's your fucking item");
                        RemoveItems(requiredItems);
                        inventory.GiveItem(Items.Behemoth);
                        isListening = false;
                    }
                }
            }
        }
    }
}
