using BepInEx;
using UnityEngine;
using RoR2;
using BepInEx.Configuration;

namespace HideScannerIndicatorOnUse
{
    [BepInPlugin("com.DestroyedClone.HideScannerIndicatorOnUse", "Hide Scanner Indicator On Use", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class HSCIOUPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> ChanceShrineHideComplete { get; set; }
        public static ConfigEntry<bool> HealShrineHideComplete { get; set; }
        public static ConfigEntry<bool> BloodShrineHideComplete { get; set; }
        //public static ConfigEntry<bool> HideScrapper { get; set; } todo
        public void Awake()
        {
            var shrineCategory = "Shrines w/ Charges";
            var shrineDesc = "Enable to only hide the indicator only if the shrine is out of charges.";
            ChanceShrineHideComplete = Config.Bind(shrineCategory, "Chance Shrine", true, shrineDesc);
            HealShrineHideComplete = Config.Bind(shrineCategory, "Shrine of the Woods", true, shrineDesc);
            BloodShrineHideComplete = Config.Bind(shrineCategory, "Blood Shrine", true, shrineDesc);
            var HideAll = !ChanceShrineHideComplete.Value && !HealShrineHideComplete.Value && !BloodShrineHideComplete.Value;

            On.RoR2.MultiShopController.DisableAllTerminals += MultiShopController_DisableAllTerminals;
            if (HideAll)
            {
                GlobalEventManager.OnInteractionsGlobal += HideScannerIndicatorAny;
            }
            else
            {
                GlobalEventManager.OnInteractionsGlobal += HideScannerIndicatorOnlyComplete;
                if (ChanceShrineHideComplete.Value)
                    On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
                if (HealShrineHideComplete.Value)
                    On.RoR2.ShrineHealingBehavior.AddShrineStack += ShrineHealingBehavior_AddShrineStack;
                if (BloodShrineHideComplete.Value)
                    On.RoR2.ShrineBloodBehavior.AddShrineStack += ShrineBloodBehavior_AddShrineStack;
            }
        }

        private void HideScannerIndicatorOnlyComplete(Interactor interactor, IInteractable interactable, GameObject gameObject)
        {
            MultiShopController multiShopController = gameObject.GetComponent<MultiShopController>();
            ShrineChanceBehavior shrineChanceBehavior = gameObject.GetComponent<ShrineChanceBehavior>();
            ShrineHealingBehavior shrineHealingBehavior = gameObject.GetComponent<ShrineHealingBehavior>();
            ShrineBloodBehavior shrineBloodBehavior = gameObject.GetComponent<ShrineBloodBehavior>();
            if (!multiShopController || !shrineChanceBehavior || !shrineHealingBehavior || !shrineBloodBehavior)
                RemoveIndicator(gameObject);
        }

        private void ShrineBloodBehavior_AddShrineStack(On.RoR2.ShrineBloodBehavior.orig_AddShrineStack orig, ShrineBloodBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            if (self.purchaseCount >= self.maxPurchaseCount)
            {
                RemoveIndicator(self.symbolTransform.gameObject);
            }
        }

        private void ShrineHealingBehavior_AddShrineStack(On.RoR2.ShrineHealingBehavior.orig_AddShrineStack orig, ShrineHealingBehavior self, Interactor activator)
        {
            orig(self, activator);
            if (self.purchaseCount >= self.maxPurchaseCount)
            {
                RemoveIndicator(self.symbolTransform.gameObject);
            }
        }

        private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            orig(self, activator);
            if (self.successfulPurchaseCount >= self.maxPurchaseCount)
            {
                RemoveIndicator(self.symbolTransform.gameObject);
            }
        }

        private void MultiShopController_DisableAllTerminals(On.RoR2.MultiShopController.orig_DisableAllTerminals orig, MultiShopController self, Interactor interactor)
        {
            orig(self, interactor);
            foreach (GameObject gameObject in self.terminalGameObjects)
            {
                RemoveIndicator(gameObject);
            }
        }

        private void HideScannerIndicatorAny(Interactor interactor, IInteractable interactable, GameObject gameObject)
        {
            RemoveIndicator(gameObject);
        }
        private void RemoveIndicator(GameObject gameObject)
        {
            if (gameObject)
            {
                var indicator = gameObject.GetComponent<ChestRevealer.RevealedObject>();
                if (indicator)
                    indicator.enabled = false;
            }
        }
    }
}
