using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using EntityStates;
using System.Collections.Generic;
using UnityEngine.Networking;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace SurvivorTaunts
{
    [BepInPlugin("com.DestroyedClone.SurvivorTaunts", "Survivor Taunts", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency("LanguageAPI")]
    public class STPlugin : BaseUnityPlugin
    {

        public static STPlugin instance;

        // soft dependency stuff
        public static bool starstormInstalled = false;
        public static RuntimeAnimatorController introAnimatorController;

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
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (arg0.name == "intro")
            {
                introAnimatorController = GameObject.Find("Set 4 - Cargo/CargoPosition/mdlCommandoDualies").GetComponent<Animator>().runtimeAnimatorController;
                if (IntroCutsceneController.shouldSkip) RoR2.Console.instance.SubmitCmd(null, "set_scene title");
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            }
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            Modules.Prefabs.CacheDisplayPrefabs();
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.isPlayerControlled)
            {
                var com = obj.gameObject.AddComponent<SurvivorTauntController>();
                com.characterBody = obj;
            }
        }

        public class SurvivorTauntController : MonoBehaviour
        {
            public LocalUser localUser;
            public CharacterBody characterBody;
            EntityStateMachine outer;
            public SurvivorDef survivorDef;
            bool isNetwork = false;

            bool canTaunt = true;

            public void Awake()
            {
                if (!characterBody)
                    characterBody = gameObject.GetComponent<CharacterBody>();
                isNetwork = NetworkServer.active;

                this.localUser = LocalUserManager.readOnlyLocalUsersList[0];

                foreach (var entityStateMachine in gameObject.GetComponents<EntityStateMachine>())
                {
                    if (entityStateMachine.customName == "Body")
                    {
                        outer = entityStateMachine;
                        break;
                    }
                }

                survivorDef = SurvivorCatalog.GetSurvivorDef(SurvivorCatalog.GetSurvivorIndexFromBodyIndex(characterBody.bodyIndex));
            }

            public void Update()
            {
                // emotes
                if (isNetwork && characterBody.characterMotor.isGrounded && !this.localUser.isUIFocused)
                {
                    if (Input.GetKeyDown(Modules.Config.disablePoseKeybind.Value))
                    {
                        canTaunt = !canTaunt;
                        Chat.AddMessage($"Survivor Taunts: {(canTaunt ? "En" : "Dis")}abled");
                    }
                    else if (Input.GetKeyDown(Modules.Config.displayKeybind.Value))
                    {
                        this.outer.SetInterruptState(EntityStateCatalog.InstantiateState(new SerializableEntityStateType(typeof(Emotes.Display))), InterruptPriority.Any);
                        return;
                    }
                    else if (Input.GetKeyDown(Modules.Config.poseKeybind.Value))
                    {
                        this.outer.SetInterruptState(EntityStateCatalog.InstantiateState(new SerializableEntityStateType(typeof(Emotes.Pose))), InterruptPriority.Any);
                        return;
                    }
                }
            }
        }
    }
}
