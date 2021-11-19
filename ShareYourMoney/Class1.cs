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
    [BepInPlugin("com.DestroyedClone.DoshDrop", "Dosh Drop", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public static GameObject ShareMoneyPack;
        internal static BepInEx.Logging.ManualLogSource _logger;

        public static KeyCode keyToDrop;
        public static int baseChestCost = 25;

        public static float percentToDrop = 0.5f;

        public static BuffDef pendingDoshBuff;    //Client adds the buff. If server detects buff, it removes it and triggers the money drop.

        public void Awake()
        {
            _logger = Logger;

            keyToDrop = Config.Bind("", "Keybind", KeyCode.B, "Button to press to drop money").Value;
            percentToDrop = Config.Bind("", "Amount to Drop (Server-Side)", 0.5f, "Drop money equivalent to this percentage of the cost of a small chest.").Value;

            CreatePrefab();

            On.RoR2.CharacterBody.Update += CharacterBody_Update;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            SetupMoneyBuff();

            //R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            // Sure would be a shame if this thing fell out of bounds.
            //On.RoR2.MapZone.OnTriggerEnter += MapZone_OnTriggerEnter;
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
                    ReleaseMoney(self.master);
                }
                else
                {
                    self.AddTimedBuffAuthority(pendingDoshBuff.buffIndex, 1000000f);
                }
            }
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
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

        // needs to be able to be sent by clients so they can drop custom money amounts
        // dunno if this just does that tho
        /*[ConCommand(commandName = "dropmoney", flags = ConVarFlags.ExecuteOnServer, helpText = "dropmoney {amount}.")]
        private static void CMDDropMoney(ConCommandArgs args)
        {
            var amountOverride = args.Count > 0 ? args.GetArgInt(0) : -1;

            ReleaseMoney(args.senderMaster.playerCharacterMasterController, amountOverride);
        }*/

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

        // Server Method
        public static void ReleaseMoney(CharacterMaster master)
        {
            if (!NetworkServer.active) return;
            uint goldReward = (uint)Mathf.CeilToInt(Run.instance.GetDifficultyScaledCost(baseChestCost) * percentToDrop);
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