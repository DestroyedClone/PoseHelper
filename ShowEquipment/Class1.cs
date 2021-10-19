using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace ShowEquipment
{
    [BepInPlugin("com.DestroyedClone.ShowEquipment", "Show Equipment", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ShowEquipmentPlugin : BaseUnityPlugin
    {
        public static BodyIndex EquipmentDroneBodyIndex;
        public static GameObject AllyCard = Resources.Load<GameObject>("prefabs/ui/AllyCard");

        public void Start()
        {
            On.RoR2.Util.GetBestBodyName += Util_GetBestBodyName;
            On.RoR2.UI.AllyCardController.Awake += AllyCardController_Awake;
            //SetupShit();
        }

        private void AllyCardController_Awake(On.RoR2.UI.AllyCardController.orig_Awake orig, AllyCardController self)
        {
            orig(self);
            var portrait = self.portraitIconImage.gameObject;
            var display = Instantiate(portrait.transform.Find("CriticallyHurt"), portrait.transform);
            display.name = "EquipmentDisplay";
            display.transform.localPosition = new Vector3(-25, 0, 0);
            var component = display.gameObject.AddComponent<AllyCardEquipment>();
            component.image = display.GetComponent<Image>();
            component.allyCardController = self;
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.BodyCatalog))]
        public static void GetBodyIndex()
        {
            EquipmentDroneBodyIndex = BodyCatalog.FindBodyIndex("EQUIPMENTDRONE");
            Debug.Log(EquipmentDroneBodyIndex);
        }

        public static void SetupShit()
        {
            var display = Instantiate(AllyCard.transform.Find("Portrait/CriticallyHurt"), AllyCard.transform.Find("Portrait"));
            display.transform.parent = AllyCard.transform.Find("Portrait");
            display.name = "EquipmentDisplay";
            var component = display.gameObject.AddComponent<AllyCardEquipment>();
            component.image = display.GetComponent<Image>();
            component.allyCardController = AllyCard.GetComponent<AllyCardController>();
        }

        private string Util_GetBestBodyName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var origName = orig(bodyObject);
            CharacterBody characterBody = null;
            if (bodyObject)
            {
                characterBody = bodyObject.GetComponent<CharacterBody>();
            }

            if (characterBody && characterBody.inventory && characterBody.inventory.currentEquipmentIndex != EquipmentIndex.None)
            {
                if (characterBody.teamComponent.teamIndex == LocalUserManager.GetFirstLocalUser().cachedMaster.teamIndex)
                {
                    origName += $" ({Language.GetString(EquipmentCatalog.GetEquipmentDef(characterBody.inventory.currentEquipmentIndex).nameToken)})";
                    //35 char limit ish before truncation
                }
            }
            return origName;
        }

        public class AllyCardEquipment : MonoBehaviour
        {
            public Image image;
            public AllyCardController allyCardController;
            public Inventory inventory;

            public void Start()
            {
                inventory = allyCardController?.sourceMaster?.inventory;
                if (inventory)
                {
                    inventory.onInventoryChanged += Inventory_onInventoryChanged;
                    UpdateSprite();
                    image.enabled = true;
                }
            }

            public void OnDestroy()
            {
                inventory.onInventoryChanged -= Inventory_onInventoryChanged;
            }

            private void Inventory_onInventoryChanged()
            {
                UpdateSprite();
            }

            public void UpdateSprite()
            {
                if (inventory.currentEquipmentIndex != EquipmentIndex.None)
                {
                    image.enabled = false;
                    var equipmentDef = EquipmentCatalog.GetEquipmentDef(allyCardController.sourceMaster.inventory.currentEquipmentIndex);
                    image.sprite = equipmentDef.pickupIconSprite;
                    image.enabled = true;
                } else
                {
                    image.sprite = null;
                }
            }
        }
    }
}