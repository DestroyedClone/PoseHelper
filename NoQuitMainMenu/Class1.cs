using BepInEx;
using UnityEngine;

namespace NoQuitMainMenu
{
    [BepInPlugin("com.DestroyedClone.NoQuitMainMenu", "No Quit To Desktop On Main Menu", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.UI.MainMenu.MainMenuController.Start += (orig, self) => 
            {
                orig(self);
                gameObject.AddComponent<HideOnUnfocus>().quitButton = GameObject.Find("MainMenu/MENU: Title/TitleMenu/SafeZone/GenericMenuButtonPanel/JuicePanel/GenericMenuButton (Quit)");
            };
        }
    }
    public class HideOnUnfocus : MonoBehaviour
    {
        public GameObject quitButton;
        bool showButton = true;

        public void Awake()
        {
            if (quitButton)
                quitButton.SetActive(false);
        }

        void OnGUI()
        {
            if (quitButton)
                quitButton.SetActive(showButton);
        }
        void OnApplicationFocus(bool hasFocus){ showButton = hasFocus; }
    }
}