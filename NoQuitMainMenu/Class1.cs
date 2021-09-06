using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace NoQuitMainMenu
{
    [BepInPlugin("com.DestroyedClone.NoQuitMainMenu", "No Quit To Desktop On Main Menu", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public ConfigEntry<bool> HideWholeMenu;

        public void Awake()
        {
            HideWholeMenu = Config.Bind("", "Hide whole menu", false, "If false, then only hides the quit button.");
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            var element = HideWholeMenu.Value ? GameObject.Find("MainMenu/MENU: Title") : GameObject.Find("MainMenu/MENU: Title/TitleMenu/SafeZone/GenericMenuButtonPanel/JuicePanel/GenericMenuButton (Quit)");
            gameObject.AddComponent<HideOnUnfocus>().elementToHide = element;
            element.SetActive(false);
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }
    }
    public class HideOnUnfocus : MonoBehaviour
    {
        public GameObject elementToHide;
        bool showButton = false;

        void OnGUI()
        {
            if (elementToHide && showButton)
            {
                elementToHide.SetActive(true);
                enabled = false;
            }
        }
        void OnApplicationFocus(bool hasFocus){ showButton = hasFocus; }
    }
}