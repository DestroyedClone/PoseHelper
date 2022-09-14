using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using static ShareYourFood.Main;

namespace ShareYourFood
{
    public class SharedFoodContent : IContentPackProvider
    {
        internal static ContentPack contentPack = new ContentPack();

        public static GameObject fruitPickup;

        public static BuffDef modifierKeyBuff;

        //public static UnlockableDef masteryUnlock;

        //public static SurvivorDef banditReloadedSurvivor;

        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();

        public string identifier => "ShareYourFood.content";

        public static void LoadResources()
        {
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            CreateBuffs();
            CreateObjects();
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

        public void CreateObjects()
        {//prevent rolling somehow?
            fruitPickup = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/NetworkedObjects/HealPack"), "FruitPack", true);
            fruitPickup.transform.eulerAngles = Vector3.zero;

            if (destroyOnTimerLength <= 0)
            {
                Object.Destroy(fruitPickup.GetComponent<DestroyOnTimer>());
                Object.Destroy(fruitPickup.GetComponent<BeginRapidlyActivatingAndDeactivating>());
            }
            else
            {
                fruitPickup.GetComponent<DestroyOnTimer>().duration = destroyOnTimerLength;
                fruitPickup.GetComponent<BeginRapidlyActivatingAndDeactivating>().delayBeforeBeginningBlinking = destroyOnTimerLength * 0.85f;
            }
            Object.Destroy(fruitPickup.GetComponent<VelocityRandomOnStart>());
            Object.Destroy(fruitPickup.transform.Find("GravitationController").gameObject);

            var healthPickup = fruitPickup.transform.Find("PickupTrigger").GetComponent<HealthPickup>();
            var foodPickup = fruitPickup.transform.Find("PickupTrigger").gameObject.AddComponent<FoodPickup>();
            foodPickup.baseObject = healthPickup.baseObject;
            foodPickup.teamFilter = healthPickup.teamFilter;
            Object.Destroy(healthPickup);

            fruitPickup.GetComponent<Rigidbody>().freezeRotation = true;

            //fruitPickup.AddComponent<ConstantForce>();

            var fruitModel = Object.Instantiate(Resources.Load<GameObject>("prefabs/pickupmodels/PickupFruit"), fruitPickup.transform.Find("HealthOrbEffect"));

            var jar = Resources.Load<GameObject>("prefabs/pickupmodels/PickupWilloWisp");

            var jarLid = Object.Instantiate(jar.transform.Find("mdlGlassJar/GlassJarLid"));
            jarLid.parent = fruitModel.transform;
            jarLid.localScale = new Vector3(1f, 1f, 0.3f);
            jarLid.localPosition = new Vector3(0f, -0.9f, 0f);

            var marble = Resources.Load<GameObject>("prefabs/pickupmodels/PickupMask");

            jarLid.GetComponent<MeshRenderer>().material = marble.GetComponentInChildren<MeshRenderer>().material;

            foodPickup.modelObject = fruitModel;

            networkedObjectPrefabs.Add(fruitPickup);
        }

        public void CreateBuffs()
        {
            BuffDef ModifierKeyBuff = ScriptableObject.CreateInstance<BuffDef>();
            ModifierKeyBuff.buffColor = new Color(1f, 215f / 255f, 0f);
            ModifierKeyBuff.canStack = false;
            ModifierKeyBuff.isDebuff = false;
            ModifierKeyBuff.name = "Activate your Foreign Fruit to throw!";
            ModifierKeyBuff.iconSprite = RoR2Content.Equipment.Fruit.pickupIconSprite;
            FixScriptableObjectName(ModifierKeyBuff);
            buffDefs.Add(ModifierKeyBuff);
            modifierKeyBuff = ModifierKeyBuff;
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