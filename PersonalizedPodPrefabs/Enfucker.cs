using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using RoR2.Projectile;
using static PersonalizedPodPrefabs.Main;

namespace PersonalizedPodPrefabs
{
    public class Enfucker
    {
        private static InteractableSpawnCard iscShieldCard;
        private static GameObject ShieldInteractablePrefab;
        private static DirectorPlacementRule placementRule;

        private static InteractableSpawnCard iscSwagCard;
        private static GameObject SwagInteractablePrefab;

        public static void Init(SurvivorDef survivorDef)
        {
            ModifyBody(survivorDef);
            SetupInteractable(survivorDef);
            Hooks();
            AddLang();
            ModifyPod(survivorDef);
        }

        private static void AddLang()
        {
            LanguageAPI.Add("INTERACTABLE_SHIELD_NAME", "Shield");
            LanguageAPI.Add("INTERACTABLE_SHIELD_CONTEXT", "Protect and Serve");
            LanguageAPI.Add("INTERACTABLE_SWAG_NAME", "Swag");
            LanguageAPI.Add("INTERACTABLE_SWAG_CONTEXT", "Protect and Swerve");
        }

        private static void ModifyBody(SurvivorDef survivorDef)
        {
            var heresy = survivorDef.bodyPrefab.GetComponent<EnforcerHideSpecialsIfHeresy>();
            if (!heresy)
                heresy = survivorDef.bodyPrefab.AddComponent<EnforcerHideSpecialsIfHeresy>();

            var mdlBody = survivorDef.bodyPrefab.GetComponent<ModelLocator>().modelTransform;
            heresy.shield = mdlBody.Find("meshSpecials/meshEnforcerShield").gameObject;
            heresy.skateboard = mdlBody.Find("meshSpecials/meshEnforcerSkamteBord").gameObject;
            heresy.shieldRender = heresy.shield.GetComponent<SkinnedMeshRenderer>();
            heresy.swagRender = heresy.skateboard.GetComponent<SkinnedMeshRenderer>();

        }

        private static void Hooks()
        {
            On.RoR2.BarrelInteraction.OnInteractionBegin += BarrelInteraction_OnInteractionBegin;
        }

        private static void ModifyPod(SurvivorDef survivorDef)
        {
            var enforcerPodPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, "EnforcerSurvivorPod");
            //var pod = enforcerPodPrefab.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/");
            //var attachedDoor = pod.Find("Door");
            //var fallDoor = pod.Find("ReleaseExhaustFX/Door,Physics").gameObject;


            enforcerPodPrefab.AddComponent<PodComponentEnforcer>();

            survivorDef.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab = enforcerPodPrefab;
        }

        private static void BarrelInteraction_OnInteractionBegin(On.RoR2.BarrelInteraction.orig_OnInteractionBegin orig, BarrelInteraction self, Interactor activator)
        {
            orig(self, activator);
            var comp = self.gameObject.GetComponent<EnforcerSelectSpecialComponent>();
            if (self.gameObject.GetComponent<EnforcerSelectSpecialComponent>())
            {
                SelectSpecialType(activator, comp.slotIndex);
            }
        }

        private static void SelectSpecialType(Interactor interactor, int index)
        {
            _logger.LogMessage("Attempting to select special type for index: "+index);
            interactor.gameObject.GetComponent<CharacterBody>().inventory.RemoveItem(RoR2Content.Items.LunarSpecialReplacement);
            Console.instance.SubmitCmd(Util.LookUpBodyNetworkUser(interactor.gameObject.GetComponent<CharacterBody>()), $"loadout_set_skill_variant EnforcerBody 3 {index}");
        }

        private static void SetupInteractable(SurvivorDef survivorDef)
        {
            // getting his model references
            var mdlBody = survivorDef.bodyPrefab.GetComponent<ModelLocator>().modelTransform;
            var shield = mdlBody.Find("meshSpecials/meshEnforcerShield").gameObject;
            var skateboard = mdlBody.Find("meshSpecials/meshEnforcerSkamteBord").gameObject;
            void setupShield()
            {
                var iscBarrel = (InteractableSpawnCard)Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscBarrel1");
                iscShieldCard = UnityEngine.Object.Instantiate(iscBarrel); //remove?
                //ShieldInteractablePrefab = iscShieldCard.prefab;
                ShieldInteractablePrefab = R2API.PrefabAPI.InstantiateClone(iscShieldCard.prefab, $"Personalized_Enforcer_SelectShield", true);
                BarrelInteraction barrelInteraction = ShieldInteractablePrefab.GetComponent<BarrelInteraction>();
                barrelInteraction.expReward = 0;
                barrelInteraction.goldReward = 0;
                barrelInteraction.displayNameToken = "INTERACTABLE_SHIELD_NAME";
                barrelInteraction.contextToken = "INTERACTABLE_SHIELD_CONTEXT";
                EnforcerSelectSpecialComponent enforcerSelectSpecialComponent = ShieldInteractablePrefab.AddComponent<EnforcerSelectSpecialComponent>();
                enforcerSelectSpecialComponent.slotIndex = 0;
                iscShieldCard.prefab = ShieldInteractablePrefab;
                ShieldInteractablePrefab.GetComponent<GenericDisplayNameProvider>().displayToken = "INTERACTABLE_SHIELD_NAME";
                //EntityLocator entityLocator = ShieldInteractablePrefab.GetComponentInChildren<EntityLocator>();
                //var newShield = Object.Instantiate(shield, entityLocator.transform.parent);
                //newShield.AddComponent<EntityLocator>().entity = entityLocator.entity;
                //Object.Destroy(entityLocator.gameObject);
                //newShield.AddComponent<MeshCollider>().sharedMesh = newShield.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            void setupSwag()
            {
                var iscBarrel = (InteractableSpawnCard)Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscBarrel1");
                iscSwagCard = UnityEngine.Object.Instantiate(iscBarrel); //remove?
                                                                           //ShieldInteractablePrefab = iscShieldCard.prefab;
                SwagInteractablePrefab = R2API.PrefabAPI.InstantiateClone(iscSwagCard.prefab, $"Personalized_Enforcer_SelectSwag", true);
                BarrelInteraction barrelInteraction = SwagInteractablePrefab.GetComponent<BarrelInteraction>();
                barrelInteraction.expReward = 0;
                barrelInteraction.goldReward = 0;
                barrelInteraction.displayNameToken = "INTERACTABLE_SWAG_NAME";
                barrelInteraction.contextToken = "INTERACTABLE_SWAG_CONTEXT";
                EnforcerSelectSpecialComponent enforcerSelectSpecialComponent = SwagInteractablePrefab.AddComponent<EnforcerSelectSpecialComponent>();
                enforcerSelectSpecialComponent.slotIndex = 1;
                iscSwagCard.prefab = SwagInteractablePrefab;
                SwagInteractablePrefab.GetComponent<GenericDisplayNameProvider>().displayToken = "INTERACTABLE_SWAG_NAME";
                //EntityLocator entityLocator = ShieldInteractablePrefab.GetComponentInChildren<EntityLocator>();
                //var newShield = Object.Instantiate(skateboard, entityLocator.transform.parent);
                //newShield.AddComponent<EntityLocator>().entity = entityLocator.entity;
                //Object.Destroy(entityLocator.gameObject);
                //newShield.AddComponent<MeshCollider>().sharedMesh = newShield.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            //sfxlocator.openSound = Play_UI_barrel_open

            //if (BarrelPrefab) PrefabAPI.RegisterNetworkPrefab(BarrelPrefab);
            placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                maxDistance = 10f,
                minDistance = 5f,
                preventOverhead = true
            };

            setupShield();
            setupSwag();
            /*
            iscSwagCard = Object.Instantiate(iscShieldCard);
            SwagInteractablePrefab = R2API.PrefabAPI.InstantiateClone(iscSwagCard.prefab, $"Personalized_Enforcer_SelectSwag", true);
            SwagInteractablePrefab.GetComponent<EnforcerSelectSpecialComponent>().slotIndex = 1;
            var swagbarrelInteraction = SwagInteractablePrefab.GetComponent<BarrelInteraction>();
            swagbarrelInteraction.displayNameToken = "INTERACTABLE_SWAG_NAME";
            swagbarrelInteraction.contextToken = "INTERACTABLE_SWAG_CONTEXT";
            SwagInteractablePrefab.GetComponent<GenericDisplayNameProvider>().displayToken = "INTERACTABLE_SWAG_NAME";
            var swagentityLocator = SwagInteractablePrefab.GetComponentInChildren<EntityLocator>();
            var newSwag = Object.Instantiate(skateboard, swagentityLocator.transform.parent);
            newSwag.AddComponent<EntityLocator>().entity = swagentityLocator.entity;
            Object.Destroy(swagentityLocator.gameObject);
            newSwag.AddComponent<MeshCollider>().sharedMesh = newSwag.GetComponent<SkinnedMeshRenderer>().sharedMesh;*/
        }

        private class EnforcerSelectSpecialComponent : MonoBehaviour
        {
            public int slotIndex = 0;
        }

        private class PodComponentEnforcer : PodComponent
        {

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                var spawnShield = iscShieldCard.DoSpawn(passenger.transform.position, Quaternion.identity, new DirectorSpawnRequest(
                        iscShieldCard, placementRule, RoR2Application.rng));

                var spawnSwag = iscSwagCard.DoSpawn(passenger.transform.position + Vector3.forward * 2f, Quaternion.identity, new DirectorSpawnRequest(
                        iscSwagCard, placementRule, RoR2Application.rng));

                passenger.GetComponent<CharacterBody>().inventory.GiveItem(RoR2Content.Items.LunarSpecialReplacement);
            }
        }

        private class EnforcerHideSpecialsIfHeresy : MonoBehaviour
        {
            public CharacterBody characterBody;
            public Inventory inventory;
            public GameObject shield;
            public GameObject skateboard;

            public SkinnedMeshRenderer shieldRender;
            public SkinnedMeshRenderer swagRender;
            public int oldIndex;

            public void Start()
            {
                if (!characterBody)
                    characterBody = gameObject.GetComponent<CharacterBody>();
                inventory = characterBody.inventory;

                gameObject.GetComponent<CharacterBody>().onInventoryChanged += EnforcerHideSpecialsIfHeresy_onInventoryChanged;
                var bodyLoadoutManager = Util.LookUpBodyNetworkUser(characterBody).networkLoadout.loadout.bodyLoadoutManager;
                oldIndex = (int)bodyLoadoutManager.GetReadOnlyBodyLoadout(characterBody.bodyIndex).skillPreferences[3];
                
                UpdateVisual();
            }

            public void OnDestroy()
            {
                gameObject.GetComponent<CharacterBody>().onInventoryChanged -= EnforcerHideSpecialsIfHeresy_onInventoryChanged;
            }

            private void EnforcerHideSpecialsIfHeresy_onInventoryChanged()
            {
                UpdateVisual();
            }

            public void UpdateVisual()
            {
                if (hasHeresy)
                {
                    shieldRender.transform.parent.gameObject.SetActive(false);
                } else
                {//switch oldIndex

                    shieldRender.transform.parent.gameObject.SetActive(true);
                }
            }

            bool hasHeresy
            {
                get
                {
                    if (inventory)
                    {
                        return inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement) > 0;
                    }
                    return false;
                }
            }
        }
    }
}
