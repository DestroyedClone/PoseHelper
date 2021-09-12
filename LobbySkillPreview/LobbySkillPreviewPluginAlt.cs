using System;
using BepInEx;
using R2API;
using UnityEngine;
using R2API.Utils;
using RoR2;
using EntityStates.Commando;
using System.Security;
using System.Security.Permissions;

using EntityStates.AI;
using static RoR2.RoR2Content;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace LobbySkillPreview
{
    [BepInPlugin("com.DestroyedClone.LobbySkillPreview", "Lobby Skill Preview", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class LobbySkillPreviewPlugin : BaseUnityPlugin
    {
        public static GameObject bodyPrefab;

        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();

            On.RoR2.UI.CharacterSelectController.SelectSurvivor += CharacterSelectController_SelectSurvivor;
            On.RoR2.UI.CharacterSelectController.Start += CharacterSelectController_Start;

            On.RoR2.TeamManager.GetTeamExperience += TeamManager_GetTeamExperience;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            //On.EntityStates.SpawnTeleporterState.OnEnter += SpawnTeleporterState_OnEnter1;
            //On.EntityStates.SpawnTeleporterState.OnExit += SpawnTeleporterState_OnExit;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
            {
                return;
            }
            orig(self);
        }

        private void SpawnTeleporterState_OnExit(On.EntityStates.SpawnTeleporterState.orig_OnExit orig, EntityStates.SpawnTeleporterState self)
        {
            orig(self);
            RoR2.Console.instance.SubmitCmd(null, "time_scale 1");
        }

        private void SpawnTeleporterState_OnEnter1(On.EntityStates.SpawnTeleporterState.orig_OnEnter orig, EntityStates.SpawnTeleporterState self)
        {
            orig(self);
            RoR2.Console.instance.SubmitCmd(null, "time_scale 10");
        }

        private ulong TeamManager_GetTeamExperience(On.RoR2.TeamManager.orig_GetTeamExperience orig, TeamManager self, TeamIndex teamIndex)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby" )
            {
                return 0;
            }
            return orig(self, teamIndex);
        }

        private void SpawnTeleporterState_OnEnter(On.EntityStates.SpawnTeleporterState.orig_OnEnter orig, EntityStates.SpawnTeleporterState self)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
            {
                self.outer.SetNextStateToMain();
                return;
            }
            orig(self);
        }

        private void CharacterSelectController_Start(On.RoR2.UI.CharacterSelectController.orig_Start orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            self.gameObject.AddComponent<SkillPreview>().characterSelect = self;
        }

        private void CharacterSelectController_SelectSurvivor(On.RoR2.UI.CharacterSelectController.orig_SelectSurvivor orig, RoR2.UI.CharacterSelectController self, SurvivorIndex survivor)
        {
            orig(self, survivor);

            var skillPreview = UnityEngine.Object.FindObjectOfType<SkillPreview>();
            skillPreview.UpdateInstance(SurvivorCatalog.GetSurvivorDef(survivor));
        }


        public class SkillPreview : MonoBehaviour
        {
            public GameObject bodyInstance;
            public SurvivorDef survivorDefInstance;
            public SkillLocator skillLocator;
            public InputBankTest inputBankTest;
            public RoR2.UI.CharacterSelectController characterSelect;

            bool showPreview = false;

            public void Start()
            {
                UpdateInstance(RoR2Content.Survivors.Commando);
            }

            public void UpdateInstance(SurvivorDef survivorDef)
            {
                if (survivorDefInstance != survivorDef)
                {
                    Destroy(bodyInstance);
                    survivorDefInstance = survivorDef;
                    SpawnInstance();
                }
            }

            private void SpawnInstance()
            {
                var bodyPrefab = survivorDefInstance.bodyPrefab;
                bodyInstance = Instantiate<GameObject>(bodyPrefab);
                //bodyInstance.GetComponent<CharacterBody>().enabled = false;
                //bodyInstance.GetComponent<HealthComponent>().enabled = false;
                bodyInstance.GetComponent<CharacterMotor>().useGravity = false;
                bodyInstance.transform.position = characterSelect.characterDisplayPads[0].displayInstance.transform.position;
                //bodyInstance.transform.LookAt(GameObject.Find("Main Camera/Scene Camera").transform.position);
                bodyInstance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                bodyInstance.SetActive(false);

                skillLocator = bodyInstance.GetComponent<SkillLocator>();
                inputBankTest = bodyInstance.GetComponent<InputBankTest>();
            }

            private void TogglePreview()
            {
                showPreview = !showPreview;
                if (bodyInstance) bodyInstance.SetActive(showPreview);
                if (characterSelect.characterDisplayPads[0].displayInstance) characterSelect.characterDisplayPads[0].displayInstance.SetActive(!showPreview);
                Chat.AddMessage("Body Instance Active: "+showPreview);
            }

            public void Update()
            {

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        TogglePreview();
                    }
                    inputBankTest.skill1.down = Input.GetKey(KeyCode.Alpha1);
                    inputBankTest.skill2.down = Input.GetKey(KeyCode.Alpha2);
                    inputBankTest.skill3.down = Input.GetKey(KeyCode.Alpha3);
                    inputBankTest.skill4.down = Input.GetKey(KeyCode.Alpha4);
                }
            }
        }
    }
}
