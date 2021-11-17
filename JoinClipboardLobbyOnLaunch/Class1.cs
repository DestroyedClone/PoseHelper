using BepInEx;
using System.Security.Permissions;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace JoinClipboardLobbyOnLaunch
{
    [BepInPlugin("com.DestroyedClone.JoinClipboardLobbyOnLaunch", "Join Clipboard Lobby On Launch", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);

            self.multiplayerMenuScreen.transform.gameObject.SetActive(true);
            var component = RoR2.UI.SteamJoinClipboardLobby.instance;
            if (component)
            {
                component.clipboardLobbyID = RoR2.UI.SteamJoinClipboardLobby.FetchClipboardLobbyId();
                component.validClipboardLobbyID = (component.clipboardLobbyID != CSteamID.nil);
                component.TryToJoinClipboardLobby();
            }

            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }
    }
}