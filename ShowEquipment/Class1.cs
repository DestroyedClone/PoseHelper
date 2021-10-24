using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using BepInEx.Configuration;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ShowEquipment
{
    [BepInPlugin("com.DestroyedClone.ShowEquipment", "Show Equipment", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ShowEquipmentPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<NameEquipmentMode> AllyCard_ShowEquipment;

        public static ConfigEntry<NameEquipmentCategory> cfgNameEquipmentCategory;
        public static ConfigEntry<NameEquipmentMode> cfgNameEquipmentMode;

        public static MasterCatalog.MasterIndex EquipmentDroneMasterIndex;
        public static GameObject AllyCard = Resources.Load<GameObject>("prefabs/ui/AllyCard");


        public enum NameEquipmentMode
        {
            Disabled,
            EquipmentDrones,
            Any
        }

        public enum NameEquipmentCategory
        {
            AllyCard,
            Any
        }

        public void Start()
        {
            AllyCard_ShowEquipment = Config.Bind("Ally Cards", "Show Equipment Icon", NameEquipmentMode.Any);
            cfgNameEquipmentCategory = Config.Bind("", "Name Category", NameEquipmentCategory.AllyCard, 
                "AllyCard = Shows up only on the ally cards" +
                "\nAny = Shows up on most, if not all, occurences.");
            cfgNameEquipmentMode = Config.Bind("General", "Name Mode", NameEquipmentMode.EquipmentDrones,
                "Disabled - Self Explanatory." +
                "\nEquipmentDrones = Only equipment drones will have their name changed." +
                "\nAny = Most bodies will have their name changed.");
            On.RoR2.MasterCatalog.Init += MasterCatalog_Init;

            if (AllyCard_ShowEquipment.Value > NameEquipmentMode.Disabled)
            {
                On.RoR2.UI.AllyCardController.Awake += AllyCardController_Awake;
            }

            if (cfgNameEquipmentMode.Value > NameEquipmentMode.Disabled)
            {
                switch (cfgNameEquipmentCategory.Value)
                {
                    case NameEquipmentCategory.AllyCard:
                        On.RoR2.UI.AllyCardController.UpdateInfo += AllyCardController_UpdateInfo;
                        break;
                    default:
                        On.RoR2.Util.GetBestBodyName += Util_GetBestBodyName;
                        On.RoR2.Util.GetBestMasterName += Util_GetBestMasterName;
                        break;
                }
            }
        }

        private string Util_GetBestMasterName(On.RoR2.Util.orig_GetBestMasterName orig, CharacterMaster characterMaster)
        {
            var origName = orig(characterMaster);

            if (characterMaster && characterMaster.inventory && characterMaster.inventory.currentEquipmentIndex != EquipmentIndex.None)
            {
                if (characterMaster.teamIndex == LocalUserManager.GetFirstLocalUser().cachedMaster.teamIndex)
                {
                    if ((characterMaster.masterIndex == EquipmentDroneMasterIndex && cfgNameEquipmentMode.Value == NameEquipmentMode.EquipmentDrones)
                        || cfgNameEquipmentMode.Value == NameEquipmentMode.Any)
                    {
                        origName += $" ({Language.GetString(EquipmentCatalog.GetEquipmentDef(characterMaster.inventory.currentEquipmentIndex).nameToken)})";
                    }
                    //35 char limit ish before truncation
                }
            }
            return origName;

        }

        private void MasterCatalog_Init(On.RoR2.MasterCatalog.orig_Init orig)
        {
            orig();
            EquipmentDroneMasterIndex = MasterCatalog.FindMasterIndex("EquipmentDroneMaster");
            //Debug.Log($"Equipment Drone MAster Index: {EquipmentDroneMasterIndex.i}");
            On.RoR2.MasterCatalog.Init -= MasterCatalog_Init;
        }

        private void AllyCardController_UpdateInfo(On.RoR2.UI.AllyCardController.orig_UpdateInfo orig, AllyCardController self)
        {
            orig(self);
            CharacterMaster master = self.sourceMaster;
            if (master
                && (( master.masterIndex == EquipmentDroneMasterIndex && cfgNameEquipmentMode.Value == NameEquipmentMode.EquipmentDrones) || cfgNameEquipmentMode.Value == NameEquipmentMode.Any)
                && master.inventory && master.inventory.currentEquipmentIndex != EquipmentIndex.None)
            {
                if (master.teamIndex == LocalUserManager.GetFirstLocalUser()?.cachedMaster?.teamIndex)
                {
                    self.nameLabel.text += $" ({Language.GetString(EquipmentCatalog.GetEquipmentDef(master.inventory.currentEquipmentIndex).nameToken)})";
                }
            }
        }


        private void AllyCardController_Awake(On.RoR2.UI.AllyCardController.orig_Awake orig, AllyCardController self)
        {
            orig(self);
            if ((AllyCard_ShowEquipment.Value == NameEquipmentMode.EquipmentDrones && self.sourceMaster && self.sourceMaster.masterIndex == EquipmentDroneMasterIndex)
                || (AllyCard_ShowEquipment.Value == NameEquipmentMode.Any))
            {
                var portrait = self.portraitIconImage.gameObject;
                var display = Instantiate(portrait.transform.Find("CriticallyHurt"), portrait.transform);
                display.name = "EquipmentDisplay";
                display.transform.localPosition = new Vector3(-25, 0, 0);
                var component = display.gameObject.AddComponent<AllyCardEquipment>();
                component.image = display.GetComponent<Image>();
                component.allyCardController = self;
            }
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
                var firstLocalUser = LocalUserManager.GetFirstLocalUser();
                if (characterBody.teamComponent 
                    && firstLocalUser != null 
                    && firstLocalUser.cachedMaster
                    && characterBody.teamComponent.teamIndex == LocalUserManager.GetFirstLocalUser().cachedMaster.teamIndex)
                {
                    if ( (characterBody.master && characterBody.master.masterIndex == EquipmentDroneMasterIndex && cfgNameEquipmentMode.Value == NameEquipmentMode.EquipmentDrones)
                        || cfgNameEquipmentMode.Value == NameEquipmentMode.Any)
                    {
                        origName += $" ({Language.GetString(EquipmentCatalog.GetEquipmentDef(characterBody.inventory.currentEquipmentIndex).nameToken)})";
                    }
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
                    image.enabled = false;
                }
            }
        }
    }
}