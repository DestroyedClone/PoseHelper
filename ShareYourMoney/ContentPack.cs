using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace ShareYourMoney
{
    public class DoshContent : IContentPackProvider
    {
        internal static ContentPack contentPack = new ContentPack();

        public static GameObject ShareMoneyPack;

        public static AssetBundle MainAssets;
        public static GameObject moneyAsset;

        public static BuffDef pendingDoshBuff;    //Client adds the buff. If server detects buff, it removes it and triggers the money drop.

        //public static UnlockableDef masteryUnlock;

        //public static SurvivorDef banditReloadedSurvivor;

        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();

        public string identifier => "DoshDrop.content";

        public static void LoadResources()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ShareYourMoney.bigbluecash"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            CreateBuffs();
            //CreateObjects();
            contentPack.buffDefs.Add(buffDefs.ToArray());
            contentPack.networkedObjectPrefabs.Add(networkedObjectPrefabs.ToArray());
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void FixScriptableObjectName(BuffDef buff)
        {
            (buff as ScriptableObject).name = buff.name;
        }

        public static void CreateObjects()
        {//prevent rolling somehow?
            moneyAsset = DoshContent.MainAssets.LoadAsset<GameObject>("Assets/bigbluecash/BBC.prefab");
            var ShareMoneyPack = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BonusGoldPackOnKill/BonusMoneyPack.prefab").WaitForCompletion(), "ShareMoneyPack", true);
            var moneyPickup = ShareMoneyPack.transform.Find("PackTrigger").GetComponent<MoneyPickup>();
            var modMoneyPickup = moneyPickup.gameObject.AddComponent<ModifiedMoneyPickup>();
            modMoneyPickup.baseObject = moneyPickup.baseObject;
            modMoneyPickup.pickupEffectPrefab = moneyPickup.pickupEffectPrefab;
            modMoneyPickup.teamFilter = moneyPickup.teamFilter;
            UnityEngine.Object.Destroy(moneyPickup);
            UnityEngine.Object.Destroy(ShareMoneyPack.GetComponent<VelocityRandomOnStart>());
            ShareMoneyPack.transform.Find("GravityTrigger").gameObject.SetActive(false);

            //var moneyCopy = Instantiate(moneyAsset);
            ShareMoneyPack.GetComponentInChildren<MeshFilter>().sharedMesh = moneyAsset.GetComponentInChildren<MeshFilter>().sharedMesh;
            var meshRenderer = ShareMoneyPack.GetComponentInChildren<MeshRenderer>();
            //meshRenderer.material = moneyAsset.GetComponentInChildren<MeshRenderer>().material;
            meshRenderer.SetMaterials(new List<Material>() { moneyAsset.GetComponentInChildren<MeshRenderer>().material });
            meshRenderer.transform.localScale = Vector3.one * 9;
            UnityEngine.Object.Destroy(ShareMoneyPack.transform.Find("Display/Mesh/Particle System").gameObject);

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
            pingInfoProvider.pingIconOverride = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("textures/miscicons/texrulebonusstartingmoney").WaitForCompletion();

            modMoneyPickup.purchaseInteraction = purchaseInteraction;

            networkedObjectPrefabs.Add(ShareMoneyPack);
            DoshContent.ShareMoneyPack = ShareMoneyPack;
        }

        public void CreateBuffs()
        {
            BuffDef PendingDoshBuff = ScriptableObject.CreateInstance<BuffDef>();
            PendingDoshBuff.buffColor = new Color(1f, 215f / 255f, 0f);
            PendingDoshBuff.canStack = true;
            PendingDoshBuff.isDebuff = false;
            PendingDoshBuff.name = "PendingDoshDrop";
            PendingDoshBuff.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffCloakIcon");
            FixScriptableObjectName(PendingDoshBuff);
            DoshContent.buffDefs.Add(PendingDoshBuff);
            DoshContent.pendingDoshBuff = PendingDoshBuff;
        }

        public class MoneyPickupMarker : MonoBehaviour
        { }

        public class ModifiedMoneyPickup : MonoBehaviour
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