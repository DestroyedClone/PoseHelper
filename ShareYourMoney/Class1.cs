using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ShareYourMoney
{
    [BepInPlugin("com.DestroyedClone.DoshDrop", "Dosh Drop", "1.0.3")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(BuffAPI), nameof(LanguageAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public static GameObject ShareMoneyPack;
        internal static BepInEx.Logging.ManualLogSource _logger;

        // CFG
        public static KeyCode keyToDrop;

        public static float percentToDrop = 0.5f;
        public static bool performanceMode = true;
        public static bool preventModUseOnStageEnd = true;
        public static bool refundOnStageEnd = true;

        public static int baseChestCost = 25;
        public static bool preventMoneyDrops = false; //Server Method
        public static bool england = false;

        public static BuffDef pendingDoshBuff;    //Client adds the buff. If server detects buff, it removes it and triggers the money drop.

        public static AssetBundle MainAssets;
        public static GameObject moneyAsset;

        public void Awake()
        {
            _logger = Logger;

            SetupAssets();

            keyToDrop = Config.Bind("", "Keybind", KeyCode.B, "Button to press to drop money").Value;
            percentToDrop = Config.Bind("", "Amount to Drop (Server-Side)", 0.5f, "Drop money equivalent to this percentage of the cost of a small chest.").Value;
            performanceMode = Config.Bind("", "Performance Mode", true, "If true, then money dropped by clients will try to be combined to prevent clients flooding the map with dropped money objects." +
                "\nOnly applied to thrown money, otherwise normal.").Value;
            preventModUseOnStageEnd = Config.Bind("", "Prevent On Stage End", true, "If true, then money will be prevented from being dropped on ending the stage.").Value;
            refundOnStageEnd = Config.Bind("", "Refund Drops On Stage End", true, "If true, then money will get refunded to owners upon ending the stage.").Value;
            england = Config.Bind("", "English (England)", true, "Renames the English translation for Money to a more regionally appropriate term.").Value;

            SetupPrefab();

            On.RoR2.CharacterBody.Update += CharacterBody_Update;
            if (performanceMode)
            {
                On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate_PerformanceMode;
            }
            else
            {
                On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            }
            SetupMoneyBuff();
            SetupLanguage();

            On.RoR2.SceneExitController.SetState += SceneExitController_SetState;
            Stage.onServerStageBegin += Stage_onServerStageBegin;
            On.RoR2.OutsideInteractableLocker.LockPurchasable += OutsideInteractableLocker_LockPurchasable;

            //R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            // Sure would be a shame if this thing fell out of bounds.
            //On.RoR2.MapZone.OnTriggerEnter += MapZone_OnTriggerEnter;
        }

        private void SetupAssets()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ShareYourMoney.bigbluecash"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }
            moneyAsset = MainAssets.LoadAsset<GameObject>("Assets/bigbluecash/BBC.prefab");
        }

        private static void SetupPrefab()
        {
            //prevent rolling somehow?
            ShareMoneyPack = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/NetworkedObjects/BonusMoneyPack"), "ShareMoneyPack", true);
            var moneyPickup = ShareMoneyPack.transform.Find("PackTrigger").GetComponent<MoneyPickup>();
            var modMoneyPickup = moneyPickup.gameObject.AddComponent<ModifiedMoneyPickup>();
            modMoneyPickup.baseObject = moneyPickup.baseObject;
            modMoneyPickup.pickupEffectPrefab = moneyPickup.pickupEffectPrefab;
            modMoneyPickup.teamFilter = moneyPickup.teamFilter;
            Destroy(moneyPickup);
            Destroy(ShareMoneyPack.GetComponent<VelocityRandomOnStart>());
            ShareMoneyPack.transform.Find("GravityTrigger").gameObject.SetActive(false);

            //var moneyCopy = Instantiate(moneyAsset);
            ShareMoneyPack.GetComponentInChildren<MeshFilter>().sharedMesh = moneyAsset.GetComponentInChildren<MeshFilter>().sharedMesh;
            var meshRenderer = ShareMoneyPack.GetComponentInChildren<MeshRenderer>();
            //meshRenderer.material = moneyAsset.GetComponentInChildren<MeshRenderer>().material;
            meshRenderer.SetMaterials(new List<Material>() { moneyAsset.GetComponentInChildren<MeshRenderer>().material });
            meshRenderer.transform.localScale = Vector3.one * 9;
            Destroy(ShareMoneyPack.transform.Find("Display/Mesh/Particle System").gameObject);

            //var genericDisplay = ShareMoneyPack.AddComponent<GenericDisplayNameProvider>();
            //genericDisplay.displayToken = "Dosh";

            var purchaseInteraction = ShareMoneyPack.AddComponent<PurchaseInteraction>();
            purchaseInteraction.contextToken = "Pickup dosh?"; //shouldnt be visible
            purchaseInteraction.costType = CostTypeIndex.Money;
            purchaseInteraction.displayNameToken = "DC_DOSH_PICKUP";
            purchaseInteraction.setUnavailableOnTeleporterActivated = false;
            purchaseInteraction.automaticallyScaleCostWithDifficulty = false;
            purchaseInteraction.lockGameObject = null;

            purchaseInteraction.gameObject.AddComponent<MoneyPickupMarker>();

            var pingInfoProvider = ShareMoneyPack.AddComponent<PingInfoProvider>();
            pingInfoProvider.pingIconOverride = Resources.Load<Sprite>("textures/miscicons/texrulebonusstartingmoney");

            modMoneyPickup.purchaseInteraction = purchaseInteraction;
        }

        //Too lazy to set up a Networkbehaviour so here's a hacky workaround.
        private void SetupMoneyBuff()
        {
            pendingDoshBuff = ScriptableObject.CreateInstance<BuffDef>();
            pendingDoshBuff.buffColor = new Color(1f, 215f / 255f, 0f);
            pendingDoshBuff.canStack = true;
            pendingDoshBuff.isDebuff = false;
            pendingDoshBuff.name = "PendingDoshDrop";
            pendingDoshBuff.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffCloakIcon");
            BuffAPI.Add(new CustomBuff(pendingDoshBuff));
        }

        private void SetupLanguage()
        {
            var token = "DC_DOSH_PICKUP";
            LanguageAPI.Add(token, england ? "Dosh" : "Money");
            LanguageAPI.Add(token, "Geld", "de");
            LanguageAPI.Add(token, "Dinero", "es-419");
            LanguageAPI.Add(token, "Argent", "FR");
            LanguageAPI.Add(token, "Soldi", "IT");
            LanguageAPI.Add(token, "お金", "ja");
            LanguageAPI.Add(token, "돈", "ko");
            LanguageAPI.Add(token, "Dinheiro", "pt-BR");
            LanguageAPI.Add(token, "Деньги", "RU");
            LanguageAPI.Add(token, "Para", "tr");
            LanguageAPI.Add(token, "钱", "zh-CN");
        }

        private void OutsideInteractableLocker_LockPurchasable(On.RoR2.OutsideInteractableLocker.orig_LockPurchasable orig, OutsideInteractableLocker self, PurchaseInteraction purchaseInteraction)
        {
            if (purchaseInteraction.GetComponent<MoneyPickupMarker>())
            {
                return;
            }
            orig(self, purchaseInteraction);
        }

        private void Stage_onServerStageBegin(Stage obj)
        {
            preventMoneyDrops = false;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (preventMoneyDrops) return;
                int doshCount = Mathf.Min(self.GetBuffCount(pendingDoshBuff.buffIndex), 8);    //Can queue up to 8
                if (doshCount > 0)
                {
                    self.ClearTimedBuffs(pendingDoshBuff.buffIndex);
                    if (self.master)
                    {
                        for (int i = 0; i < doshCount; i++)
                        {
                            ReleaseMoney(self.master);
                        }
                    }
                }
            }
        }

        private void SceneExitController_SetState(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            bool approved = newState != self.exitState;
            orig(self, newState);
            if (!approved) return;
            switch (self.exitState)
            {
                case SceneExitController.ExitState.Idle:
                    preventMoneyDrops = false;
                    break;

                case SceneExitController.ExitState.ExtractExp:
                    if (preventModUseOnStageEnd)
                        preventMoneyDrops = true;
                    if (refundOnStageEnd)
                        RefundMoneyPackPickups();
                    break;

                default:
                    if (preventModUseOnStageEnd)
                        preventMoneyDrops = true;
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

        private void CharacterBody_Update(On.RoR2.CharacterBody.orig_Update orig, CharacterBody self)
        {
            orig(self);
            if (self.hasAuthority && self.isPlayerControlled && self.master
                && !LocalUserManager.readOnlyLocalUsersList[0].isUIFocused
                && Input.GetKeyDown(keyToDrop))
            {
                if (NetworkServer.active)
                {
                    if (preventMoneyDrops) return;
                    ReleaseMoney(self.master);
                }
                else
                {
                    self.AddTimedBuffAuthority(pendingDoshBuff.buffIndex, 1000000f);
                }
            }
        }

        private void CharacterBody_FixedUpdate_PerformanceMode(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (preventMoneyDrops) return;
                int doshCount = self.GetBuffCount(pendingDoshBuff.buffIndex);
                if (doshCount > 0)
                {
                    self.ClearTimedBuffs(pendingDoshBuff.buffIndex);
                    if (self.master)
                    {
                        ReleaseMoney(self.master, doshCount);
                    }
                }
            }
        }

        // needs to be able to be sent by clients so they can drop custom money amounts
        // dunno if this just does that tho
        /*[ConCommand(commandName = "dropmoney", flags = ConVarFlags.ExecuteOnServer, helpText = "dropmoney {amount}.")]
        private static void CMDDropMoney(ConCommandArgs args)
        {
            var amountOverride = args.Count > 0 ? args.GetArgInt(0) : -1;

            ReleaseMoney(args.senderMaster.playerCharacterMasterController, amountOverride);
        }*/

        // Server Method
        public static void ReleaseMoney(CharacterMaster master, int doshesDropped = 1)
        {
            if (!NetworkServer.active) return;
            uint goldReward = (uint)Mathf.CeilToInt(Run.instance.GetDifficultyScaledCost(baseChestCost) * percentToDrop * doshesDropped);
            if (master && master.GetBody())
            {
                // 15 - 25 = -10, so resulting money is 10 to drop
                if (goldReward > master.money)
                {
                    goldReward = master.money;
                }

                //goldReward <= 0 or goldReward < 1??
                if ((uint)goldReward <= 0)
                {
                    //to avoid dropping $0 items.
                    return;
                }

                GameObject pickup = Instantiate(ShareMoneyPack);
                pickup.transform.position = master.GetBody().corePosition;
                ModifiedMoneyPickup moneyPickup = pickup.GetComponentInChildren<ModifiedMoneyPickup>();
                moneyPickup.goldReward = (int)goldReward;
                moneyPickup.owner = master.GetBody() ?? null;

                Rigidbody component = pickup.GetComponent<Rigidbody>();

                Vector3 direction;
                if (master.GetBody().inputBank)
                {
                    Ray aimRay = master.GetBody().inputBank.GetAimRay();
                    direction = aimRay.direction;
                    pickup.transform.position = aimRay.origin;  //set position to aimray if aimray is found
                }
                else
                {
                    direction = master.GetBody().transform.forward;
                }
                component.velocity = Vector3.up * 5f + (direction * 20f); // please fine tune

                // Figure out how to communicate to the client how much money was dropped.
                //Chat.AddMessage($"You have dropped ${(uint)goldReward}");
                //DamageNumberManager.instance.SpawnDamageNumber((int)goldReward, pickup.transform.position, false, TeamIndex.Player, DamageColorIndex.Item);

                NetworkServer.Spawn(pickup);
                master.money = (uint)(Mathf.Max(0f, master.money - goldReward));
            }
        }

        private class MoneyPickupMarker : MonoBehaviour
        { }

        private class ModifiedMoneyPickup : MonoBehaviour
        {
            public static readonly List<ModifiedMoneyPickup> instancesList = new List<ModifiedMoneyPickup>();

            public PurchaseInteraction purchaseInteraction;

            private void OnEnable()
            {
                instancesList.Add(this);
            }

            private void OnDisable()
            {
                instancesList.Remove(this);
            }

            private void Start()
            {
                if (NetworkServer.active)
                    purchaseInteraction.Networkcost = goldReward;
            }

            public void Refund()
            {
                allowPickup = false;
                if (owner.master)
                {
                    owner.master.GiveMoney((uint)goldReward);
                    Destroy(this.baseObject);
                }
            }

            private void FixedUpdate()
            {
                // prevents early re-pickup by owner
                age += Time.fixedDeltaTime;
                if (age > durationBeforeOwnerPickup)
                {
                    ownerCanPickup = true;
                }
            }

            // Token: 0x060013E7 RID: 5095 RVA: 0x00052B84 File Offset: 0x00050D84
            private void OnTriggerStay(Collider other)
            {
                if (NetworkServer.active && this.alive)
                {
                    if (!allowPickup) return;

                    var characterBody = other.GetComponent<CharacterBody>();
                    if (characterBody && characterBody.isPlayerControlled && characterBody.master)
                    {
                        if (ownerCanPickup && characterBody == owner || characterBody != owner)
                        {
                            this.alive = false;
                            Vector3 position = base.transform.position;
                            characterBody.master.GiveMoney((uint)goldReward);
                            if (this.pickupEffectPrefab)
                            {
                                EffectManager.SimpleEffect(this.pickupEffectPrefab, position, Quaternion.identity, true);
                            }
                            UnityEngine.Object.Destroy(this.baseObject);
                        }
                    }
                }
            }

            // Token: 0x040011AF RID: 4527
            [Tooltip("The base object to destroy when this pickup is consumed.")]
            public GameObject baseObject;

            // Token: 0x040011B0 RID: 4528
            [Tooltip("The team filter object which determines who can pick up this pack.")]
            public TeamFilter teamFilter;

            // Token: 0x040011B1 RID: 4529
            public GameObject pickupEffectPrefab;

            // Token: 0x040011B4 RID: 4532
            private bool alive = true;

            // Token: 0x040011B5 RID: 4533
            public int goldReward;

            private float age = 0;

            private float durationBeforeOwnerPickup = 3f;

            public CharacterBody owner;

            private bool ownerCanPickup = false;

            private bool allowPickup = true;
        }
    }
}