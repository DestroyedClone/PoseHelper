using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using EntityStates;

namespace SurvivorTaunts
{
    [BepInPlugin("com.DestroyedClone.SurvivorTaunts", "Survivor Taunts", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class STPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> Key_1_Pose { get; set; }
        public static ConfigEntry<bool> Key_2_Pose { get; set; }

        public static STPlugin instance;

        // soft dependency stuff
        public static bool starstormInstalled = false;

        public void Awake()
        {
            instance = this;

            // check for soft dependencies
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.Starstorm2")) starstormInstalled = true;

            // load assets and read config
            Modules.Config.ReadConfig();
            Modules.States.RegisterStates(); // register states for networking
            Modules.Tokens.AddTokens(); // register name tokens

            // have to setup late so the catalog can populate
            On.RoR2.SurvivorCatalog.Init += SurvivorCatalog_Init;
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.isPlayerControlled)
                obj.gameObject.AddComponent<SurvivorTauntController>();
        }

        private void SurvivorCatalog_Init(On.RoR2.SurvivorCatalog.orig_Init orig)
        {
            orig();
            Modules.Prefabs.CacheDisplays();
        }

        public class SurvivorTauntController : MonoBehaviour
        {
            public LocalUser localUser;
            private CharacterBody characterBody;
            bool isAuthority = true;
            EntityStateMachine outer;
            public SurvivorIndex survivorIndex;

            public void Awake()
            {
                Debug.Log("Getting localuser");
                this.localUser = LocalUserManager.readOnlyLocalUsersList[0];
                Debug.Log("cachedbody");
                characterBody = this.localUser.cachedBody;
                Debug.Log("entitystatemachine");
                outer = this.gameObject.GetComponent<EntityStateMachine>();
                Debug.Log("survivorindex");
                survivorIndex = SurvivorCatalog.GetSurvivorIndexFromBodyIndex(characterBody.bodyIndex);

                if ((int)survivorIndex == -1) Debug.LogWarning("SurvivorIndex is at -1, may cause errors.");
            }

            public void Update()
            {
                // emotes
                if (isAuthority && characterBody.characterMotor.isGrounded && !this.localUser.isUIFocused)
                {
                    if (Input.GetKeyDown(Modules.Config.displayKeybind.Value))
                    {
                        this.outer.SetInterruptState(EntityState.Instantiate(new SerializableEntityStateType(typeof(Emotes.Display))), InterruptPriority.Any);
                        return;
                    }
                    else if (Input.GetKeyDown(Modules.Config.poseKeybind.Value))
                    {
                        this.outer.SetInterruptState(EntityState.Instantiate(new SerializableEntityStateType(typeof(Emotes.Pose))), InterruptPriority.Any);
                        return;
                    }
                }
            }
        }
    }
}
