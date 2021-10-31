using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using static DeathMessageAboveCorpse.Quotes;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace DeathMessageAboveCorpse
{
    [BepInPlugin("com.DestroyedClone.DeathMessageAboveCorpse", "Death Message Above Corpse", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI), nameof(NetworkingAPI))]
    public class DeathMessageAboveCorpsePlugin : BaseUnityPlugin
    {
        public static string[] deathMessagesResolved = new string[] { };

        public static ConfigEntry<float> cfgDuration;
        public static ConfigEntry<bool> cfgUseSSMessages;
        public static ConfigEntry<bool> cfgOnlyLastLife;
        public static ConfigEntry<float> cfgDelayMultiplayer;
        public static ConfigEntry<bool> cfgShowQuoteOnScreenSingleplayer;
        public static float fontSize = 10f;

        public static GameObject defaultTextObject;
        public static GameObject defaultTrackerObject;

        // Text displays larger for the client in the middle of the screen (https://youtu.be/vQRPpSx5WLA?t=1336)
        // 3 second delay after the corpse is on the ground before showing either client or server message
        //

        public void Awake()
        {
            SetupConfig();
            ReadConfig();
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            //On.RoR2.ModelLocator.OnDestroy += ModelLocator_OnDestroy;
            NetworkingAPI.RegisterMessageType<Networking.DeathQuoteMessageToServer>();
            NetworkingAPI.RegisterMessageType<Networking.DeathQuoteMessageToClients>();
            defaultTextObject = CreateDefaultTextObject();
            defaultTrackerObject = CreateTrackerObject();
        }

        private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            orig(self);
            //self.master.IsDeadAndOutOfLivesServer()
            bool lastLifeCheck = cfgOnlyLastLife.Value == false || (cfgOnlyLastLife.Value && self.master && IsDeadAndOutOfLives(self.master));
            if (self.isPlayerControlled && lastLifeCheck)
            {
                if (LocalUserManager.readOnlyLocalUsersList[0].cachedBody?.GetComponent<NetworkIdentity>() == self.GetComponent<NetworkIdentity>())
                {
                    var trackerObject = Instantiate<GameObject>(defaultTrackerObject);
                    trackerObject.name = $"Tracking Corpse: {self.GetDisplayName()}";
                    trackerObject.GetComponent<TrackCorpseClient>().modelTransform = self.modelLocator.modelTransform.transform;
                    trackerObject.GetComponent<TrackCorpseClient>().lastPosition = self.transform.position;
                }
            }
        }

        private bool IsDeadAndOutOfLives(CharacterMaster characterMaster)
        {
            CharacterBody body = characterMaster.GetBody();
            return (!body || !body.healthComponent.alive) && characterMaster.inventory.GetItemCount(RoR2Content.Items.ExtraLife) <= 0;
        }

        private void ModelLocator_OnDestroy(On.RoR2.ModelLocator.orig_OnDestroy orig, ModelLocator self)
        {
            //self.characterMotor.body.master.IsDeadAndOutOfLivesServer()
            if (self?.characterMotor?.body?.master && self.characterMotor.body.healthComponent && !self.characterMotor.body.healthComponent.alive)
            {
                self.preserveModel = true;
                self.noCorpse = true;
            }
            orig(self);
        }

        public void SetupConfig()
        {
            cfgDuration = Config.Bind("", "Duration", 60f, "Length of time in seconds the message stays out. Set to zero/negative for 100 hours.");
            cfgUseSSMessages = Config.Bind("Match with Host", "Starstorm Strings", false, "If enabled, then the mod will include death messages from Starstorm." +
                "\nEnsure this setting matches with the host of the server.");
            cfgOnlyLastLife = Config.Bind("Match with Host", "Only Show On True Death", true, "If enabled, then the message will only show up on the player's last life, to mirror Risk of Rain 1." +
                "\nEnsure this setting matches with the host of the server.");
            cfgDelayMultiplayer = Config.Bind("Delay", "Duration", 3f, "Length of time in seconds before the message displays after the player has stopped moving.");
            cfgShowQuoteOnScreenSingleplayer = Config.Bind("Client", "Show Quote On Screen In Singleplayer", true, "If true, then the death quote will show on the game's end report panel in singleplayer." +
                "\nAlso sets the delay to 0 seconds in singleplayer.");
            //cfgFinalSurvivorCorpseKept = Config.Bind("", "Keep Final Corpse Alive", true, "If true, keeps the player's final/last-life corpse from getting deleted until the message is finished.");
        }

        public void ReadConfig()
        {
            List<string> list = new List<string>(deathMessagesResolved);
            list.AddRange(deathMessages);
            if (cfgUseSSMessages.Value) list.AddRange(deathMessagesStarstorm);
            deathMessagesResolved = list.ToArray();
        }

        public void ICantRead()
        {
            // use a dictionary?
            // <bool, string[]>
            // iterate through, if true then add to a string[][] object
            // var length = foreach string[] in string[][].length++;
            // new string[length].
            // foreach string[] in string[][]
            // Copy to based off last or something

            // WHAT THE FUCK DOES THIS MEAN I CANT REMEMBER
        }

        public static bool IsSingleplayer()
        {
            return NetworkUser.readOnlyInstancesList.Count <= NetworkUser.readOnlyLocalPlayersList.Count;
        }

        public static GameObject CreateTrackerObject()
        {
            var trackerObject = new GameObject();
            trackerObject.name = "trackingprefab";
            trackerObject.AddComponent<NetworkIdentity>();
            trackerObject.AddComponent<TrackCorpseClient>();
            var prefab = PrefabAPI.InstantiateClone(trackerObject, "DeathMessageAboveCorpse_DefaultTrackerObject");

            PrefabAPI.RegisterNetworkPrefab(prefab);
            return prefab;
        }

        public static GameObject CreateDefaultTextObject()
        {
            var textPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/DamageRejected"), "DeathMessageAboveCorpse_DefaultTextObjectChild");
            textPrefab.name = "DeathMessageAboveCorpse_DefaultTextObject";
            UnityEngine.Object.Destroy(textPrefab.GetComponent<EffectComponent>());
            textPrefab.GetComponent<ObjectScaleCurve>().overallCurve = AnimationCurve.Constant(0f, 1f, 1f);
            UnityEngine.Object.Destroy(textPrefab.GetComponent<VelocityRandomOnStart>());
            UnityEngine.Object.Destroy(textPrefab.GetComponent<ConstantForce>());
            UnityEngine.Object.Destroy(textPrefab.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(textPrefab.transform.Find("TextMeshPro").gameObject.GetComponent<ScaleSpriteByCamDistance>());
            textPrefab.AddComponent<NetworkIdentity>();

            DeathMessageLocator deathMessageLocator = textPrefab.AddComponent<DeathMessageLocator>();
            deathMessageLocator.textMeshPro = textPrefab.transform.Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>();
            deathMessageLocator.textMeshPro.fontSize = fontSize;
            deathMessageLocator.languageTextMeshController = textPrefab.transform.Find("TextMeshPro").gameObject.GetComponent<LanguageTextMeshController>();
            deathMessageLocator.destroyOnTimer = textPrefab.GetComponent<DestroyOnTimer>();
            textPrefab.GetComponent<DestroyOnTimer>().duration = cfgDuration.Value > 0 ? cfgDuration.Value : 360000;

            PrefabAPI.RegisterNetworkPrefab(textPrefab);
            defaultTextObject = textPrefab;
            return textPrefab;
        }

        public class TrackCorpseClient : MonoBehaviour
        {
            private float age;
            public float timeBeforeDisplay = cfgDelayMultiplayer.Value;
            public bool stoppedMoving;

            public Vector3 lastPosition = Vector3.zero;

            public CameraRigController cameraRig;
            public GameObject target;

            public Transform modelTransform;

            private readonly float lenience = 0.15f;

            bool isSinglePlayer = false;

            private void Start()
            {
                isSinglePlayer = IsSingleplayer();

                if (isSinglePlayer && cfgShowQuoteOnScreenSingleplayer.Value)
                {
                    timeBeforeDisplay = 0f;
                }

                cameraRig = CameraRigController.readOnlyInstancesList[0];
                if (cameraRig) target = cameraRig.target;
            }

            private void FixedUpdate()
            {
                if (cameraRig && cameraRig.targetParams && cameraRig.targetParams.cameraPivotTransform && cameraRig.target == target)
                {
                    lastPosition = cameraRig.targetParams.cameraPivotTransform.position;
                    //EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/DamageRejected"), new EffectData(){origin = lastPosition}, true);
                    if (Mathf.Abs(Vector3.Distance(cameraRig.targetParams.cameraPivotTransform.position, lastPosition)) > lenience)
                    {
                        age = 0f;
                        return;
                    }
                }
                age += Time.fixedDeltaTime;

                if (age > timeBeforeDisplay)
                {
                    Physics.Raycast(lastPosition, Vector3.down, out RaycastHit raycastHit, 1000f, LayerIndex.world.mask);
                    if (Vector3.Distance(raycastHit.point, lastPosition) >= 3f)
                    {
                        lastPosition = raycastHit.point;
                    }
                    var positionToSend = lastPosition + Vector3.up * 3f;

                    if (!isSinglePlayer)
                    {
                        new Networking.DeathQuoteMessageToServer(positionToSend).Send(NetworkDestination.Server);
                    } else
                    {
                        PerformSingleplayerAction(positionToSend);
                    }
                    //Chat.AddMessage("Sent message to server!");
                    //if (modelTransform && !modelTransform.gameObject.GetComponent<Corpse>()) modelTransform.gameObject.AddComponent<Corpse>();
                    Destroy(gameObject);
                }
            }

            private void PerformSingleplayerAction(Vector3 position)
            {
                var quoteIndex = UnityEngine.Random.Range(0, deathMessagesResolved.Length);
                var typingText = UnityEngine.Object.Instantiate(defaultTextObject);
                typingText.transform.position = position;
                DeathMessageLocator deathMessageLocator = typingText.GetComponent<DeathMessageLocator>();
                deathMessageLocator.quoteIndex = quoteIndex;
            }
        }

        public class DeathMessageLocator : MonoBehaviour
        {
            public TextMeshPro textMeshPro;
            public LanguageTextMeshController languageTextMeshController;
            public DestroyOnTimer destroyOnTimer;
            public int quoteIndex = 0;

            public GameObject HUDDisplayInstance = null;

            public void Start()
            {
                //Chat.AddMessage(""+quoteIndex);
                if (quoteIndex > deathMessagesResolved.Length - 1)
                {
                    quoteIndex = deathMessagesResolved.Length - 1;
                }
                var deathQuote = deathMessagesResolved[quoteIndex];
                languageTextMeshController.token = deathQuote;

                if (IsSingleplayer() && cfgShowQuoteOnScreenSingleplayer.Value)
                    ShowMessageOnHud();
                On.RoR2.UI.GameEndReportPanelController.Awake += GameEndReportPanelController_Awake;
            }

            private void GameEndReportPanelController_Awake(On.RoR2.UI.GameEndReportPanelController.orig_Awake orig, GameEndReportPanelController self)
            {
                orig(self);

                if (HUDDisplayInstance)
                {
                    RectTransform trans = (RectTransform)HUDDisplayInstance.transform;
                    trans.SetParent(self.transform);
                    trans.localPosition = new Vector3(0f, 450f, 0f);
                }
                On.RoR2.UI.GameEndReportPanelController.Awake -= GameEndReportPanelController_Awake;
            }

            private void OnDestroy()
            {
                On.RoR2.UI.GameEndReportPanelController.Awake -= GameEndReportPanelController_Awake;
            }

            public void ShowMessageOnHud()
            {
                var hudSimple = GameObject.Find("HUDSimple(Clone)");
                var SteamBuildLabel = hudSimple.transform.Find("MainContainer/SteamBuildLabel");
                RectTransform trans = (RectTransform)Instantiate(SteamBuildLabel);
                trans.SetParent(SteamBuildLabel);
                Object.Destroy(trans.GetComponent<SteamBuildIdLabel>());
                var comp = trans.GetComponent<HGTextMeshProUGUI>();
                comp.text = $"{languageTextMeshController.token}";
                comp.color = new Color32(255, 255, 255, 255);
                comp.fontSize = 1f;
                comp.alignment = TextAlignmentOptions.Center;
                trans.localPosition = new Vector3(0f, 450f, 0f);
                HUDDisplayInstance = trans.gameObject;
                //hasShownHUDMessage = true;
            }
        }

        /*
        public class ShowDeathMessageComponent : MonoBehaviour
        {
            public DestroyOnTimer destroyOnTimer;
            public GameObject gameObjectToEnable;

            private float age;
            public float timeBeforeDisplay = 3f;
            public bool stoppedMoving;

            public TextMeshPro textMeshPro;
            public LanguageTextMeshController languageTextMeshController;

            public Transform transformToWatch;

            public Vector3 lastPosition = Vector3.zero;

            public CameraRigController cameraRig;
            public GameObject target;

            private float lenience = 0.1f;

            private void Awake()
            {
                cameraRig = CameraRigController.readOnlyInstancesList[0];
                if (cameraRig) target = cameraRig.target;
            }

            private void Start()
            {
                if (destroyOnTimer) destroyOnTimer.enabled = false;
                gameObjectToEnable.SetActive(false);
            }

            private void FixedUpdate()
            {
                if (cameraRig && cameraRig.targetParams && cameraRig.targetParams.cameraPivotTransform && cameraRig.target == target)
                {
                    lastPosition = cameraRig.targetParams.cameraPivotTransform.position;
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/DamageRejected"), new EffectData()
                    {
                        origin = lastPosition
                    }, true);
                    if (Mathf.Abs(Vector3.Distance(cameraRig.targetParams.cameraPivotTransform.position, lastPosition)) > lenience)
                    {
                        this.age = 0f;
                        return;
                    }
                }
                this.age += Time.fixedDeltaTime;

                if (this.age > this.timeBeforeDisplay)
                {
                    //bool grounded = false;
                    Physics.Raycast(lastPosition, Vector3.down, out RaycastHit raycastHit, 1000f, LayerIndex.world.mask);
                    if (Vector3.Distance(raycastHit.point, lastPosition) >= 3f || !transformToWatch)
                    {
                        //grounded = true;
                        lastPosition = raycastHit.point;
                    }
                    gameObject.transform.position = lastPosition + Vector3.up * 3f;
                    if (destroyOnTimer) destroyOnTimer.enabled = true;
                    if (gameObjectToEnable) gameObjectToEnable.SetActive(true);
                    if (transformToWatch && !transform.gameObject.GetComponent<Corpse>()) transformToWatch.gameObject.AddComponent<Corpse>();
                    enabled = false;
                }
            }
        }*/
    }
}