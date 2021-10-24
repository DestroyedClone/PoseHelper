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
        public static string[] deathMessages = new string[]
        {
            "shak pls.",
            "You are dead.",
            "You embrace the void.",
            "You had a lot more to live for.",
            "Your internal organs have failed.",
            "Your body was gone an hour later.",
            "Your family will never know how you died.",
            "You died painlessly.",
            "Your death was extremely painful.",
            "You have broken every bone in your body.",
            "You die a slightly embarrassing death.",
            "You die in a hilarious pose.",
            "You really messed up.",
            "You have died. Maybe next time..",
            "You have passed away. Try again?",
            "Choose a new character?",
            "Remember to activate use items.",
            "Remember that as time increases, so does difficulty.",
            "This planet has killed you.",
            "It wasn't your time to die...",
            "That was definitely not your fault.",
            "That was absolutely your fault.",
            "They will surely feast on your flesh.",
            "..the harder they fall.",
            "Beep.. beep.. beeeeeeeeeeeeeeeee",
            "Close!",
            "Come back soon!",
            "Crushed.",
            "Smashed.",
            "DEAD",
            "Get styled upon.",
            "Dead from blunt trauma to the face.",
            "ded",
            "rekt",
            "ur dead LOL get rekt",
            "Sucks to suck.",
            "You walk towards the light.",

            // TODO: Seperate based on difficulty (Run.instance.selectedDifficulty)
            "Try playing on \"Drizzle\" mode for an easier time.",
            "Consider lowering the difficulty.",
        };

        public static string[] deathMessagesStarstorm = new string[]
        {"Oh no...",
        "Oopsie.",
        "Should have seen that coming.",
        "Destiny sealed.",
        "That was depressing.",
        "Anticlimatic.",
        "Yeet.",
        "That wasn't cool.",
        "Dying, again.",
        "Could have been worse.",
        "At least you tried.",
        "Heartbroken.",
        "Not OP.",
        "Should have gotten Dio's Friend.",
        "What?",
        "Yeah... nice.",
        "Are you okay?",
        "Let's try again sometime soon.",
        "Rude, didn't even say goodbye.",
        "Another one bites the dust.",
        "Stinky.",
        "Get some rest.",
        "I don't get it.",
        ":(",
        "To be honest, I expected that.",
        "You are no more.",
        "You died painfully.",
        "Let's pretend that didn't happen.",
        "Bye bye.",
        "You cried before losing consciousness.",
        "You will be remembered.",
        "You didn't stand a chance.",
        "Very, very dead.",
        "Sorry mom.",
        "Mom says it's my turn now.",
        "What's the point?",
        "Relatable.",
        "Try harder.",
        "Boom.",
        "ouchies owo",
        "F.",
        "This is the part where you quit.",
        "You weren't strong enough.",
        "Continue?",
        "You have perished.",
        "The planet has claimed your life",
        "You fought valiantly... But to no avail.",
        "You didn't know how to live.",
        "The end.",
        "FIN.",
        "Was that it?",
        "Give up?",
        "The end?",
        "Nooooooooooo!",
        "Good enough.",
        "Fair game.",
        "Need a tutorial?",
        "Help is not coming.",
        "Medic!",
        "C-c-c-combo breaker!",
        "It was fun while it lasted.",
        "An attempt was made.",
        "You had one job.",
        "Good job!",
        "Need a hug?",
        "Hnng.",
        "Life comes and goes.",
        "It's just a game.",
        "You'll win! ...Someday.",
        "You could do it.",
        "dead.exe",
        "Imagine living",
        "What?!",
        "What was that?",
        "Don't skip leg day.",
        "Wow! Okay...",
        "Whew.",
        "Yes, you just died.",
        "Free ticket to hell.",
        "Free ticket to heaven.",
        "The void welcomes you.",
        "Weee wooo weee wooo."
        };

        public static string[] deathMessagesResolved = new string[] { };

        public static ConfigEntry<float> cfgDuration;
        public static ConfigEntry<bool> cfgUseSSMessages;
        public static ConfigEntry<bool> cfgOnlyLastLife;
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
                if (LocalUserManager.readOnlyLocalUsersList[0].cachedBody.GetComponent<NetworkIdentity>() == self.GetComponent<NetworkIdentity>())
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
            return (!body || !body.healthComponent.alive) && characterMaster.inventory.GetItemCount(RoR2Content.Items.ExtraLife) <= 0 && !characterMaster.IsInvoking("RespawnExtraLife");
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
            //cfgFinalSurvivorCorpseKept = Config.Bind("", "Keep Final Corpse Alive", true, "If true, keeps the player's final/last-life corpse from getting deleted until the message is finished.");
        }

        public void ReadConfig()
        {
            if (cfgUseSSMessages.Value)
            {
                deathMessagesResolved = new string[deathMessages.Length + deathMessagesStarstorm.Length];
                deathMessages.CopyTo(deathMessagesResolved, 0);
                deathMessagesStarstorm.CopyTo(deathMessagesResolved, deathMessages.Length);
            }
            else
            {
                deathMessagesResolved = (string[])deathMessages.Clone();
            }
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
            public float timeBeforeDisplay = 3f;
            public bool stoppedMoving;

            public Vector3 lastPosition = Vector3.zero;

            public CameraRigController cameraRig;
            public GameObject target;

            public Transform modelTransform;

            private readonly float lenience = 0.15f;

            private void Start()
            {
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

                    new Networking.DeathQuoteMessageToServer(positionToSend).Send(NetworkDestination.Server);
                    //Chat.AddMessage("Sent message to server!");
                    //if (modelTransform && !modelTransform.gameObject.GetComponent<Corpse>()) modelTransform.gameObject.AddComponent<Corpse>();
                    Destroy(gameObject);
                }
            }
        }

        public class DeathMessageLocator : MonoBehaviour
        {
            public TextMeshPro textMeshPro;
            public LanguageTextMeshController languageTextMeshController;
            public DestroyOnTimer destroyOnTimer;
            public int quoteIndex = 0;

            public void Start()
            {
                //Chat.AddMessage(""+quoteIndex);
                if (quoteIndex > deathMessagesResolved.Length - 1)
                {
                    quoteIndex = deathMessagesResolved.Length - 1;
                }
                var deathQuote = deathMessagesResolved[quoteIndex];
                languageTextMeshController.token = deathQuote;
            }

            public void ShowMessageOnHud()
            {
                var hudSimple = GameObject.Find("HUDSimple(Clone)");
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