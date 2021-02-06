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
        public static ConfigEntry<bool> ShrineHideIncomplete { get; set; }
        public static ConfigEntry<bool> TrishopHide { get; set; }
        //public static ConfigEntry<bool> HideScrapper { get; set; } todo
        public void Awake()
        {
            ShrineHideIncomplete = Config.Bind("Default", "Enable to remove indicator on shrines even with charges left", true, "Toggles the bright light that fades out on scan.");
            TrishopHide = Config.Bind("Default", "Hide all items of a tri-shop terminal", true, "Toggles the small light that's emitted pretty much right where you are.");

            GlobalEventManager.OnInteractionsGlobal += HideScannerIndicator;
        }

        private void HideScannerIndicator(Interactor interactor, IInteractable interactable, GameObject gameObject)
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
