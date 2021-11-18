using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ShareYourMoney
{
    [BepInPlugin("com.DestroyedClone.ShareYourMoney", "Share Your Money", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class Main : BaseUnityPlugin
    {
        public static GameObject ShareMoneyPack;
        internal static BepInEx.Logging.ManualLogSource _logger;

        public static KeyCode keyToDrop;
        public static int baseChestCost = 25;

        public void Start()
        {
            _logger = Logger;

            keyToDrop = Config.Bind("", "Keybind", KeyCode.B, "Button to press to drop money").Value;

            CreatePrefab();

            On.RoR2.PlayerCharacterMasterController.Update += PlayerCharacterMasterController_Update;

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            // Sure would be a shame if this thing fell out of bounds.
            //On.RoR2.MapZone.OnTriggerEnter += MapZone_OnTriggerEnter;
        }

        // needs to be able to be sent by clients so they can drop custom money amounts
        // dunno if this just does that tho
        [ConCommand(commandName = "dropmoney", flags = ConVarFlags.ExecuteOnServer, helpText = "dropmoney {amount}.")]
        private static void CMDDropMoney(ConCommandArgs args)
        {
            var amountOverride = args.Count > 0 ? args.GetArgInt(0) : -1;

            ReleaseMoney(args.senderMaster.playerCharacterMasterController, amountOverride);
        }

        private static void CreatePrefab()
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
        }

        private void PlayerCharacterMasterController_Update(On.RoR2.PlayerCharacterMasterController.orig_Update orig, PlayerCharacterMasterController self)
        {
            orig(self);
            // Using this method so i dont have to make a component's Update() input check shit
            // Probably network check thing here.

            //first part prevents fucking dropping shit while typing with uis open, like the console
            if (!LocalUserManager.readOnlyLocalUsersList[0].isUIFocused && Input.GetKeyDown(keyToDrop))
            {
                ReleaseMoney(self);
            }
        }

        // Server Method
        public static void ReleaseMoney(PlayerCharacterMasterController playerCharacterMasterController, float amountOverride = -1)
        {
            var goldReward = (amountOverride > 0 ? amountOverride : Run.instance.GetDifficultyScaledCost(baseChestCost) / 2);
            var master = playerCharacterMasterController.master;
            if (master && master.GetBody())
            {
                // 15 - 25 = -10, so resulting money is 10 to drop
                if (master.money - goldReward < 0)
                {
                    goldReward = master.money;
                }

                //goldReward <= 0 or goldReward < 1??
                if ((uint)goldReward <= 0)
                {
                    //to avoid dropping $0 items.
                    return;
                }

                var pickup = Instantiate(ShareMoneyPack);
                pickup.transform.position = master.GetBody().corePosition; //dunno how to set it head level, aimorigin?
                var moneyPickup = pickup.GetComponentInChildren<ModifiedMoneyPickup>();
                moneyPickup.goldReward = (int)goldReward;
                moneyPickup.owner = master.GetBody() ?? null;

                Rigidbody component = pickup.GetComponent<Rigidbody>();

                Vector3 direction;
                if (master.GetBody().equipmentSlot) //idk how else to do this
                {
                    var aimRay = master.GetBody().equipmentSlot.GetAimRay();
                    direction = aimRay.direction;
                }
                else
                {
                    direction = master.GetBody().transform.forward;
                }

                component.velocity = Vector3.up * 15f + (direction * 25f); // please fine tune

                // Figure out how to communicate to the client how much money was dropped.
                Chat.AddMessage($"You have dropped ${(uint)goldReward}");
                DamageNumberManager.instance.SpawnDamageNumber((int)goldReward, pickup.transform.position, false, TeamIndex.Player, DamageColorIndex.Item);

                NetworkServer.Spawn(pickup);
                master.money = (uint)(Mathf.Max(0f, master.money - goldReward));
            }
        }

        private class ModifiedMoneyPickup : MonoBehaviour
        {
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
        }
    }
}