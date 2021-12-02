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

namespace ShareYourFood
{
    [BepInPlugin("com.DestroyedClone.ShareYourFood", "Share Your Food", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public static GameObject fruitPickup;

        public static KeyCode keyToDrop;
        public static float healPercentage;
        public static float destroyOnTimerLength;
        public static float modifierKeyLeeWay;
        public static bool changeDescription;
        public static int maxAmount;
        //todo max amount per player?

        public static BuffDef modifierKeyBuff;

        //TILER2 shit
        //When a game runs with TILER2, the first selected language is null so we have to wait once
        public static int incremeter = 0;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;

            keyToDrop = Config.Bind("Client", "Modifier Keybind", KeyCode.LeftAlt, "Holding this button and pressing your equipment use button will drop the Fruit.").Value;
            healPercentage = Config.Bind("Sync w/ Server", "Heal Percentage", 0.5f, "The percentage of maximum health healed upon walking over.").Value;
            destroyOnTimerLength = Config.Bind("Sync w/ Server", "Duration", 45, "The amount of time in seconds that the pickup will be alive for. Set to -1 for infinite.").Value;
            changeDescription = Config.Bind("Client", "Change Description", true, "If true, then the description will have information about the modifier key in it.").Value;
            maxAmount = Config.Bind("Sync w/ Server", "Max Amount of Fruit in World", 200, "What is the maximum amount of thrown fruit in the world?").Value;

            if (changeDescription)
                Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;

            CreatePrefab();
            SetupModifierKeyBuff();

            On.RoR2.EquipmentSlot.FireFruit += EquipmentSlot_FireFruit;
            On.RoR2.CharacterBody.Update += CharacterBody_Update;
        }

        private void Language_onCurrentLanguageChanged()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.TILER2") && incremeter == 0)
            {
                incremeter++;
                return;
            }

            var keyName = "EQUIPMENT_FRUIT_DESC";

            void UpdateLanguage(string newString, string language)
            {
                LanguageAPI.Add(keyName, Language.GetString(keyName, language) + " " + newString, language);
                //_logger.LogMessage($"Updated Fruit Desc for {language}");
            }

            switch (Language.currentLanguageName)
            {
                case "de":
                    UpdateLanguage($"Wenn du {keyToDrop} gedrückt hältst und benutzt, wirfst du ihn, um einen Verbündeten zu heilen.", "de");
                    break;

                case "en":
                    UpdateLanguage($"Holding down {keyToDrop} and using will throw it to heal an ally.", "en");
                    break;

                case "es-419":
                    UpdateLanguage($"Si mantienes pulsada {keyToDrop} y la utilizas, la lanzarás para curar a un aliado.", "es-419");
                    break;

                case "fr":
                    UpdateLanguage($"En maintenant {keyToDrop} et en l'utilisant, vous le lancerez pour soigner un allié.", "fr");
                    break;

                case "it":
                    UpdateLanguage($"Tenendo premuto {keyToDrop} e usandolo lo lancerà per curare un alleato.", "it");
                    break;

                case "ja":
                    UpdateLanguage($"{keyToDrop}を押しながら使用すると、味方を回復するために投げられます。", "ja");
                    break;

                case "ko":
                    UpdateLanguage($"{keyToDrop}를 누르고 있으면 던져서 아군을 치료합니다.", "ko"); //google
                    break;

                case "pt-BR":
                    UpdateLanguage($"Segurando o {keyToDrop} e usando-o para curar um aliado.", "pt-BR");
                    break;

                case "RU":
                    UpdateLanguage($"Удерживая {keyToDrop} и используя, вы бросите его, чтобы вылечить союзника.", "RU");
                    break;

                case "tr":
                    UpdateLanguage($"{keyToDrop}'yi basılı tutmak ve kullanmak, bir müttefiki iyileştirmek için onu fırlatır.", "tr"); //google
                    break;

                case "zh-cn":
                    UpdateLanguage($"按住{keyToDrop}并使用会扔掉它来治疗一个盟友。", "zh-cn");
                    break;
            }
            //Language.onCurrentLanguageChanged -= Language_onCurrentLanguageChanged;
        }

        private void CharacterBody_Update(On.RoR2.CharacterBody.orig_Update orig, CharacterBody self)
        {
            orig(self);
            if (self.hasAuthority && self.isPlayerControlled
                && !LocalUserManager.readOnlyLocalUsersList[0].isUIFocused)
            {
                if (Input.GetKey(keyToDrop) && self.inventory?.currentEquipmentIndex == RoR2Content.Equipment.Fruit.equipmentIndex)
                {
                    self.AddTimedBuffAuthority(modifierKeyBuff.buffIndex, 0.5f);
                }
            }
        }

        private void SetupModifierKeyBuff()
        {
            modifierKeyBuff = ScriptableObject.CreateInstance<BuffDef>();
            modifierKeyBuff.buffColor = new Color(1f, 215f / 255f, 0f);
            modifierKeyBuff.canStack = false;
            modifierKeyBuff.isDebuff = false;
            modifierKeyBuff.name = "Activate your Foreign Fruit to throw!";
            modifierKeyBuff.iconSprite = RoR2Content.Equipment.Fruit.pickupIconSprite;
            BuffAPI.Add(new CustomBuff(modifierKeyBuff));
        }

        private static void CreatePrefab()
        {
            fruitPickup = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/NetworkedObjects/HealPack"), "FruitPack", true);
            fruitPickup.transform.eulerAngles = Vector3.zero;

            if (destroyOnTimerLength <= 0)
            {
                Object.Destroy(fruitPickup.GetComponent<DestroyOnTimer>());
                Destroy(fruitPickup.GetComponent<BeginRapidlyActivatingAndDeactivating>());
            }
            else
            {
                fruitPickup.GetComponent<DestroyOnTimer>().duration = destroyOnTimerLength;
                fruitPickup.GetComponent<BeginRapidlyActivatingAndDeactivating>().delayBeforeBeginningBlinking = destroyOnTimerLength * 0.85f;
            }
            Destroy(fruitPickup.GetComponent<VelocityRandomOnStart>());
            Destroy(fruitPickup.transform.Find("GravitationController").gameObject);

            var healthPickup = fruitPickup.transform.Find("PickupTrigger").GetComponent<HealthPickup>();
            var foodPickup = fruitPickup.transform.Find("PickupTrigger").gameObject.AddComponent<FoodPickup>();
            foodPickup.baseObject = healthPickup.baseObject;
            foodPickup.teamFilter = healthPickup.teamFilter;
            Destroy(healthPickup);

            fruitPickup.GetComponent<Rigidbody>().freezeRotation = true;

            //fruitPickup.AddComponent<ConstantForce>();

            var fruitModel = Instantiate(Resources.Load<GameObject>("prefabs/pickupmodels/PickupFruit"), fruitPickup.transform.Find("HealthOrbEffect"));

            var jar = Resources.Load<GameObject>("prefabs/pickupmodels/PickupWilloWisp");

            var jarLid = Object.Instantiate(jar.transform.Find("mdlGlassJar/GlassJarLid"));
            jarLid.parent = fruitModel.transform;
            jarLid.localScale = new Vector3(1f, 1f, 0.3f);
            jarLid.localPosition = new Vector3(0f, -0.9f, 0f);

            var marble = Resources.Load<GameObject>("prefabs/pickupmodels/PickupMask");

            jarLid.GetComponent<MeshRenderer>().material = marble.GetComponentInChildren<MeshRenderer>().material;

            foodPickup.modelObject = fruitModel;
        }

        //method is only called by server
        private bool ThrowFruit(EquipmentSlot equipmentSlot)
        {
            if (InstanceTracker.GetInstancesList<FoodPickup>()?.Count < maxAmount)
            {
                GameObject pickup = UnityEngine.Object.Instantiate<GameObject>(fruitPickup, equipmentSlot.characterBody.transform.position, UnityEngine.Random.rotation);
                pickup.GetComponent<TeamFilter>().teamIndex = equipmentSlot.teamComponent.teamIndex;
                FoodPickup foodPickup = pickup.GetComponentInChildren<FoodPickup>();
                foodPickup.owner = equipmentSlot.characterBody;
                pickup.transform.localScale = new Vector3(1f, 1f, 1f);

                Vector3 direction;
                if (equipmentSlot.characterBody.inputBank)
                {
                    Ray aimRay = equipmentSlot.characterBody.inputBank.GetAimRay();
                    direction = aimRay.direction;
                    pickup.transform.position = aimRay.origin;  //set position to aimray if aimray is found
                }
                else
                {
                    direction = equipmentSlot.transform.forward;
                }
                Rigidbody component = pickup.GetComponent<Rigidbody>();
                component.velocity = Vector3.up * 5f + (direction * 20f); // please fine tune
                pickup.transform.eulerAngles = Vector3.zero;

                NetworkServer.Spawn(pickup);
                return true;
            }
            return false;
        }

        private bool EquipmentSlot_FireFruit(On.RoR2.EquipmentSlot.orig_FireFruit orig, EquipmentSlot self)
        {
            //_logger.LogMessage($"{self.hasEffectiveAuthority} {Input.GetKey(keyToDrop)} {self.characterBody.HasBuff(modifierKeyBuff)}");
            if (self.characterBody.HasBuff(modifierKeyBuff))
            {
                return ThrowFruit(self);
            }

            return orig(self);
        }

        public class FoodPickup : MonoBehaviour
        {
            private void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            private void OnDisable()
            {
                InstanceTracker.Remove(this);
            }

            private void Update()
            {
                this.localTime += Time.deltaTime;
                if (localTime > durationBeforeOwnerPickup)
                {
                    ownerCanPickup = true;
                }
                if (modelObject)
                {
                    Transform transform = this.modelObject.transform;
                    Vector3 localEulerAngles = transform.localEulerAngles;
                    localEulerAngles.y = this.spinSpeed * this.localTime;
                    transform.localEulerAngles = localEulerAngles;
                }
            }

            private void FixedUpdate()
            {
                gameObject.transform.rotation = Quaternion.identity;
            }

            private void OnTriggerStay(Collider other)
            {
                if (NetworkServer.active && this.alive && TeamComponent.GetObjectTeam(other.gameObject) == this.teamFilter.teamIndex)
                {
                    CharacterBody body = other.GetComponent<CharacterBody>();
                    if (body != owner)
                    {
                        EatDaFruit(body);
                        UnityEngine.Object.Destroy(this.baseObject);
                    }
                    else if (ownerCanPickup && body.inventory.GetEquipmentIndex() == RoR2Content.Equipment.Fruit.equipmentIndex)
                    {
                        //bool equipmentChargesLessThanMax = body.inventory.GetEquipmentRestockableChargeCount(body.inventory.activeEquipmentSlot) < body.inventory.GetActiveEquipmentMaxCharges();

                        body.inventory.RestockEquipmentCharges(body.inventory.activeEquipmentSlot, 1);
                        UnityEngine.Object.Destroy(this.baseObject);
                    }
                }
            }

            //FireFruit but independent...
            private bool EatDaFruit(CharacterBody characterbody)
            {
                if (characterbody.healthComponent)
                {
                    EffectData effectData = new EffectData();
                    effectData.origin = base.transform.position;
                    effectData.SetNetworkedObjectReference(base.gameObject);
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/FruitHealEffect"), effectData, true);
                    characterbody.healthComponent.HealFraction(0.5f, default(ProcChainMask));
                }
                return true;
            }

            // Token: 0x04000F18 RID: 3864
            [Tooltip("The base object to destroy when this pickup is consumed.")]
            public GameObject baseObject;

            // Token: 0x04000F19 RID: 3865
            [Tooltip("The team filter object which determines who can pick up this pack.")]
            public TeamFilter teamFilter;

            // Token: 0x04000F1A RID: 3866
            public GameObject pickupEffect;

            // Token: 0x04000F1D RID: 3869
            private bool alive = true;

            public CharacterBody owner;

            public GameObject modelObject;

            public float localTime = 0;

            public float spinSpeed = 55f;

            private float durationBeforeOwnerPickup = 3f;

            private bool ownerCanPickup = false;
        }
    }
}