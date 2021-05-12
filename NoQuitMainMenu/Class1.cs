using BepInEx;
using UnityEngine;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace NoQuitMainMenu
{
    [BepInPlugin("com.DestroyedClone.NoQuitMainMenu", "No Quit To Desktop On Main Menu", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            var obj = GameObject.Find("MainMenu/MENU: Title/TitleMenu/SafeZone/GenericMenuButtonPanel/JuicePanel/GenericMenuButton (Quit)");
            obj.SetActive(false);
        }
    }
}
