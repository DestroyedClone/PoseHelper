using BepInEx;
using UnityEngine;

namespace NoQuitMainMenu
{
    [BepInPlugin("com.DestroyedClone.NoQuitMainMenu", "No Quit To Desktop On Main Menu", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.UI.MainMenu.MainMenuController.Start += (orig, self) => 
            {
                orig(self);
                GameObject.Find("MainMenu/MENU: Title/TitleMenu/SafeZone/GenericMenuButtonPanel/JuicePanel/GenericMenuButton (Quit)").SetActive(false);
            };
        }
    }
}