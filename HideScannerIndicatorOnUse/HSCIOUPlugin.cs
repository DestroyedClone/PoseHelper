using RoR2;
using UnityEngine;
using BepInEx;
using System.Security;
using System.Security.Permissions;
using R2API.Utils;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HideScannerIndicatorOnUse
{
    [BepInPlugin("com.DestroyedClone.HideScannerIndicatorOnUse", "Hide Scanner Indicator On Use", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class HSCIOUPlugin : BaseUnityPlugin
    {
        public void Awake()
        {

            On.RoR2.MultiShopController.DisableAllTerminals += MultiShopController_DisableAllTerminals;
            GlobalEventManager.OnInteractionsGlobal += HideScannerIndicatorAny;
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
