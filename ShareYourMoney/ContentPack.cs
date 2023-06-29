using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ShareYourMoney
{
    public class DoshContent : IContentPackProvider
    {
        internal static ContentPack contentPack = new ContentPack();
        public static AssetBundle mainAssetBundle;
        public const string bundleName = "bigbluecash";
        public const string assetBundleFolder = "AssetBundles";

        public static string AssetBundlePath
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(DoshDropPlugin.PInfo.Location), assetBundleFolder, bundleName);
            }
        }

        public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();
        public static GameObject ShareMoneyPack;
        public static GameObject moneyAsset;

        public string identifier => "DoshDrop.content";

        public static void LoadResources()
        {
            mainAssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            //CreateBuffs();
            //CreateObjects();
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

        public static void CreateObjects()
        {//prevent rolling somehow?
            moneyAsset = DoshContent.mainAssetBundle.LoadAsset<GameObject>("Assets/bigbluecash/BBC.prefab");
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
            pingInfoProvider.pingIconOverride = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texRuleBonusStartingMoney.png").WaitForCompletion();

            modMoneyPickup.purchaseInteraction = purchaseInteraction;

            networkedObjectPrefabs.Add(ShareMoneyPack);
            DoshContent.ShareMoneyPack = ShareMoneyPack;
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
                ConsumePickup(owner.master);
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
                            ConsumePickup(characterBody.master, true);
                        }
                    }
                }
            }

            private void ConsumePickup(CharacterMaster collectorMaster, bool showEffect = false)
            {
                allowPickup = false;
                this.alive = false;
                Vector3 position = base.transform.position;
                if (collectorMaster)
                    collectorMaster.GiveMoney((uint)goldReward);
                if (this.pickupEffectPrefab && showEffect)
                    EffectManager.SimpleEffect(this.pickupEffectPrefab, position, Quaternion.identity, true);
                UnityEngine.Object.Destroy(this.baseObject);
            }

            [Tooltip("The base object to destroy when this pickup is consumed.")]
            public GameObject baseObject;

            [Tooltip("The team filter object which determines who can pick up this pack.")]
            public TeamFilter teamFilter;

            public GameObject pickupEffectPrefab;

            private bool alive = true;

            public int goldReward;

            private float age = 0;

            private readonly float durationBeforeOwnerPickup = 3f;

            public CharacterBody owner;

            private bool ownerCanPickup = false;

            private bool allowPickup = true;
        }
    }
}