using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RiskOfOptions;
using RoR2;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using static ShareYourMoney.DoshContent;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ShareYourMoney
{
    [BepInPlugin("com.DestroyedClone.DoshDrop", "Dosh Drop", "1.0.5")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class DoshDropPlugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource _logger;
        public static PluginInfo PInfo { get; private set; }

        // CFG
        public static ConfigEntry<KeyboardShortcut> cfgCDropKey;

        public static ConfigEntry<bool> cfgCLanguageSwap;

        public static ConfigEntry<float> cfgSPercentToDrop;
        public static ConfigEntry<bool> cfgSPreventModUseOnStageEnd;
        public static ConfigEntry<bool> cfgSRefundOnStageEnd;

        public static int baseChestCost = 25;
        public static bool SGlobalPreventMoneyDrops = false; //Server Method

        public void Awake()
        {
            PInfo = Info;
        }

        public void Start()
        {
            _logger = Logger;
            DoshContent.LoadResources();
            DoshContent.CreateObjects();

            cfgCDropKey = Config.Bind("Client", "Keybind", new KeyboardShortcut(KeyCode.B), "Button to press to drop money");
            cfgCLanguageSwap = Config.Bind("Client", "English (England)", true, "Renames the English translation for Money to a more regionally appropriate term.");

            cfgSPercentToDrop = Config.Bind("Server", "Amount to Drop", 0.5f, "Drop money equivalent to this percentage of the cost of a small chest.");
            cfgSPreventModUseOnStageEnd = Config.Bind("Server", "Prevent On Stage End", true, "If true, then money will be prevented from being dropped on ending the stage.");
            cfgSRefundOnStageEnd = Config.Bind("Server", "Refund Drops On Stage End", true, "If true, then money will get refunded to owners upon ending the stage.");

            CharacterBody.onBodyStartGlobal += AddInputTrackerToCharacter;
            SetupLanguage();

            On.RoR2.SceneExitController.SetState += RefundMoneyDropsOnSceneExit;
            Stage.onServerStageBegin += OnStageBegin_ResetPreventMoneyDrops;
            On.RoR2.OutsideInteractableLocker.LockPurchasable += PreventInteractionLockPrefab;

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            // Sure would be a shame if this thing fell out of bounds.
            //On.RoR2.MapZone.OnTriggerEnter += MapZone_OnTriggerEnter;
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
            ModCompat.Initialize();
        }

        public bool CharacterBodyIsLocalUser(CharacterBody body)
        {
            return LocalUserManager.GetFirstLocalUser().cachedMaster == body.master;
        }

        private void AddInputTrackerToCharacter(CharacterBody body)
        {
            if (body.isPlayerControlled && body.master && CharacterBodyIsLocalUser(body))
            {
                var comp = body.gameObject.AddComponent<DoshDrop_InputCheckerComponent>();
                comp.userLocalUser = LocalUserManager.GetFirstLocalUser();
                //body.master.GetComponent<PlayerCharacterMasterController>().networkUser.localUser;
            }
        }

        public static void SetupLanguage()
        {
            var token = "DC_DOSH_PICKUP";
            LanguageAPI.Add(token, cfgCLanguageSwap.Value ? "Dosh" : "Money");
            LanguageAPI.Add(token, "Geld", "de");
            LanguageAPI.Add(token, "Dinero", "es-419");
            LanguageAPI.Add(token, cfgCLanguageSwap.Value ? "Fric" : "Argent", "FR");
            LanguageAPI.Add(token, "Soldi", "IT");
            LanguageAPI.Add(token, "お金", "ja");
            LanguageAPI.Add(token, "돈", "ko");
            LanguageAPI.Add(token, "Dinheiro", "pt-BR");
            LanguageAPI.Add(token, "Деньги", "RU");
            LanguageAPI.Add(token, "Para", "tr");
            LanguageAPI.Add(token, "钱", "zh-CN");
        }

        private void PreventInteractionLockPrefab(On.RoR2.OutsideInteractableLocker.orig_LockPurchasable orig, OutsideInteractableLocker self, PurchaseInteraction purchaseInteraction)
        {
            if (purchaseInteraction.GetComponent<MoneyPickupMarker>())
            {
                return;
            }
            orig(self, purchaseInteraction);
        }

        private void OnStageBegin_ResetPreventMoneyDrops(Stage obj)
        {
            SGlobalPreventMoneyDrops = false;
        }

        private void RefundMoneyDropsOnSceneExit(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            bool approved = newState != self.exitState;
            orig(self, newState);
            if (!approved) return;
            switch (self.exitState)
            {
                case SceneExitController.ExitState.Idle:
                    SGlobalPreventMoneyDrops = false;
                    break;

                case SceneExitController.ExitState.ExtractExp:
                    if (cfgSPreventModUseOnStageEnd.Value)
                        SGlobalPreventMoneyDrops = true;
                    if (cfgSRefundOnStageEnd.Value)
                        RefundMoneyPackPickups();
                    break;

                default:
                    if (cfgSPreventModUseOnStageEnd.Value)
                        SGlobalPreventMoneyDrops = true;
                    break;
            }
        }

        private static void RefundMoneyPackPickups()
        {
            foreach (var moneyPickup in new List<ModifiedMoneyPickup>(ModifiedMoneyPickup.instancesList))
            {
                if (moneyPickup)
                {
                    moneyPickup.Refund();
                }
            }
        }

        // needs to be able to be sent by clients so they can drop custom money amounts
        // dunno if this just does that tho
        [ConCommand(commandName = "doshdrop", flags = ConVarFlags.ExecuteOnServer, helpText = "doshdrop {positive amount}.")]
        private static void CCDropMoney(ConCommandArgs args)
        {
            if (!Run.instance)
            {
                Debug.Log("doshdrop: Can't drop any dosh without being in a run!");
                return;
            }
            if (!args.TryGetSenderBody())
            {
                Debug.Log("doshdrop: Can't drop any dosh without a body!");
                return;
            }
            var finalMoney = 0;
            if (args.Count > 0)
            {
                var moneyRequested = args.TryGetArgInt(0);
                if (!moneyRequested.HasValue)
                {
                    Debug.Log("doshdrop: Couldn't parse the the value as an integer.");
                    return;
                }
                if (moneyRequested.Value < 0)
                {
                    Debug.Log("doshdrop: Can't drop negative money!");
                    return;
                }
                finalMoney = moneyRequested.Value;
            }
            Server_ReleaseMoney(args.senderMaster, (uint)finalMoney);
        }

        // Server Method
        public static void Server_ReleaseMoney(CharacterMaster master, uint goldReward = 0)
        {
            if (!NetworkServer.active)
            {
                _logger.LogWarning("DoshDrop.Server_ReleaseMoney called on client!");
                return;
            }
            if (goldReward == 0)
            {
                goldReward = (uint)Mathf.CeilToInt(Run.instance.GetDifficultyScaledCost(baseChestCost) * cfgSPercentToDrop.Value * 1);
            }
            if (master)
            {
                var body = master.GetBody();
                if (!body)
                    return;

                if (goldReward > master.money)
                {
                    goldReward = master.money;
                }

                //to avoid dropping $0 items.
                if (goldReward <= 0)
                    return;

                GameObject pickup = Instantiate(ShareMoneyPack);
                pickup.transform.position = body.corePosition;
                ModifiedMoneyPickup moneyPickup = pickup.GetComponentInChildren<ModifiedMoneyPickup>();
                moneyPickup.goldReward = (int)goldReward;
                moneyPickup.owner = body;

                Rigidbody rigidBody = pickup.GetComponent<Rigidbody>();

                Vector3 direction;
                if (body.inputBank)
                {
                    Ray aimRay = body.inputBank.GetAimRay();
                    direction = aimRay.direction;
                    pickup.transform.position = aimRay.origin;  //set position to aimray if aimray is found
                }
                else
                {
                    direction = body.gameObject.transform.forward;
                }
                rigidBody.velocity = Vector3.up * 5f + (direction * 20f); // please fine tune

                // Figure out how to communicate to the client how much money was dropped.
                //Chat.AddMessage($"You have dropped ${(uint)goldReward}");
                //DamageNumberManager.instance.SpawnDamageNumber((int)goldReward, pickup.transform.position, false, TeamIndex.Player, DamageColorIndex.Item);

                NetworkServer.Spawn(pickup);
                master.money = (uint)(Mathf.Max(0f, master.money - goldReward));
            }
        }

        public static void ReleaseMoneyAuthority(CharacterMaster characterMaster, uint moneyAmount = 0U)
        {
            if (!RoR2.Console.instance) return;
            RoR2.Console.instance.SubmitCmd(characterMaster.GetComponent<PlayerCharacterMasterController>().networkUser, $"doshdrop {moneyAmount}", false);
            /*
            if (NetworkServer.active)
            {
                Server_ReleaseMoney(characterMaster, moneyAmount);
                return;
            }
            CallCmdReleaseMoney(characterMaster, moneyAmount);*/
        }

        public static void CallCmdReleaseMoney(CharacterMaster characterMaster, uint moneyToDrop = 0U)
        {
            if (!RoR2.Console.instance) return;
            RoR2.Console.instance.SubmitCmd(characterMaster.GetComponent<PlayerCharacterMasterController>().networkUser, "doshdrop", false);
        }

        public class DoshDrop_InputCheckerComponent : MonoBehaviour
        {
            public LocalUser userLocalUser;
            private bool wasPressed = false;

            public void Update()
            {
                bool isDropKeyPressed = cfgCDropKey.Value.IsPressedInclusive();
                if (isDropKeyPressed && wasPressed) return;
                if (!isDropKeyPressed) wasPressed = false;
                if (userLocalUser.isUIFocused) return;

                if (isDropKeyPressed)
                {
                    ReleaseMoneyAuthority(userLocalUser.cachedMaster);
                    wasPressed = true;
                }
            }
        }
    }
}