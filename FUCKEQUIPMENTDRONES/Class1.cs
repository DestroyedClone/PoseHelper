using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using R2API;
using System.Linq;
//using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace FUCKEQUIPMENTDRONES
{
    [BepInPlugin("com.DestroyedClone.FUCKEQUIPMENTDRONES", "FUCKEQUIPMENTDRONES", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class FUCKINGMAIN : BaseUnityPlugin
    {
        public static ConfigEntry<string> BannedIDS { get; set; }
        //public EquipmentIndex[] bannedEquipmentIndices = new EquipmentIndex[] { };
        public List<EquipmentIndex> bannedEquipmentIndices = new List<EquipmentIndex>();

        public void Awake()
        {
            On.RoR2.PurchaseInteraction.GetInteractability += PreventEquipmentFromAllowance;

            BannedIDS = Config.Bind("Default", "Banned Equipment IDS", "AffixRed,AffixBlue,AffixYellow,AffixGold,AffixWhite,AffixPoison,Jetpack,GoldGat,Gateway,QuestVolatileBattery,Recycle,DeathProjectile", "Enter the IDs of the equipment you want to ban from equipment drones." +
                "\nSeparated by commas (ex: AffixRed,Meteor,Fruit)");

            On.RoR2.EquipmentCatalog.Init += CacheBannedIDS;
        }

        private void CacheBannedIDS(On.RoR2.EquipmentCatalog.orig_Init orig)
        {
            orig();
            var testStringArray = BannedIDS.Value.Split(',');
            if (testStringArray.Length > 0)
            {
                foreach (string stringToTest in testStringArray)
                {
                    if (EquipmentCatalog.FindEquipmentIndex(stringToTest) == EquipmentIndex.None) { continue; }
                    bannedEquipmentIndices.Add(EquipmentCatalog.FindEquipmentIndex(stringToTest));
                }
            }
        }

        private Interactability PreventEquipmentFromAllowance(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            SummonMasterBehavior summonMasterBehavior = self.gameObject.GetComponent<SummonMasterBehavior>();
            if (summonMasterBehavior && summonMasterBehavior.callOnEquipmentSpentOnPurchase)
            {
                CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                if (characterBody && characterBody.inventory)
                {
                    if (bannedEquipmentIndices.Contains(characterBody.inventory.currentEquipmentIndex))
                    {
                        return Interactability.ConditionsNotMet;
                    }
                }
            }
            return orig(self, activator);
        }
    }
}
