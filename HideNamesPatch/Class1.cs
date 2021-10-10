using BepInEx;
using BepInEx.Configuration;
using Facepunch.Steamworks;
using RoR2;
using RoR2.Networking;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HideNamesPatch
{
    [BepInPlugin("com.DestroyedClone.HideNamesPatch", "HideNamesPatch", "1.1.0")]
    public class HideNames : BaseUnityPlugin
    {
        public ConfigEntry<string> NameOverride;
        public ConfigEntry<string> BodyFallbackName;

        public ConfigEntry<string> SkinFallbackName;
        public ConfigEntry<string> SkinNameFormatting;
        public ConfigEntry<string> DefaultSkinNameOverride;
        public static ConfigEntry<float> NameUpdateFrequency;
        public ConfigEntry<string> ShowHost;
        public static string StringForCharacterName = "";
        public static string StringForSkinName = "Skin";

        public static bool GetBodyName = false;
        public static bool GetSkinName = false;
        public static bool ReplaceDefaultName = false;
        public static bool GetHostID = false;
        public static bool UseDefaultHostFormatting = false;
        private static string hostFormattingString = "{0} ";
        public static CSteamID HostSteamID;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static Dictionary<CSteamID, string> SteamID_to_DisplayName = new Dictionary<CSteamID, string>();

        public void Awake()
        {
            _logger = Logger;
            SetupConfig();
            ReadConfig();

            On.RoR2.NetworkUser.GetNetworkPlayerName += NetworkUser_GetNetworkPlayerName;
            On.RoR2.NetworkUser.Start += NetworkUser_Start;
            On.RoR2.NetworkUser.OnDestroy += NetworkUser_OnDestroy;
            On.RoR2.UI.SocialUsernameLabel.RefreshForSteam += SocialUsernameLabel_RefreshForSteam;
            //On.RoR2.SocialUserIcon.RefreshForSteam += SocialUserIcon_RefreshForSteam;
            On.RoR2.UI.HostGamePanelController.SetDefaultHostNameIfEmpty += HostGamePanelController_SetDefaultHostNameIfEmpty;
        }

        private void HostGamePanelController_SetDefaultHostNameIfEmpty(On.RoR2.UI.HostGamePanelController.orig_SetDefaultHostNameIfEmpty orig, RoR2.UI.HostGamePanelController self)
        {
            GameNetworkManager.SvHostNameConVar instance = GameNetworkManager.SvHostNameConVar.instance;
            if (string.IsNullOrEmpty(instance.GetString()))
            {
                instance.SetString(Language.GetStringFormatted("HOSTGAMEPANEL_DEFAULT_SERVER_NAME_FORMAT", new object[]
                {
                    GetDefaultNameOutsideLobby()
                }));
            }
            orig(self);
        }

        public string GetDefaultNameOutsideLobby()
        {
            return GetBodyName ? BodyFallbackName.Value : NameOverride.Value;
        }

        private void SocialUsernameLabel_RefreshForSteam(On.RoR2.UI.SocialUsernameLabel.orig_RefreshForSteam orig, RoR2.UI.SocialUsernameLabel self)
        {
            orig(self);
            Client instance = Client.Instance;
            if (instance != null && self.textMeshComponent != null)
            {
                var replace = GetDefaultNameOutsideLobby();

                self.textMeshComponent.text = replace;
                if (self.subPlayerIndex != 0)
                {
                    TextMeshProUGUI textMeshProUGUI = self.textMeshComponent;
                    textMeshProUGUI.text = string.Concat(new object[]
                    {
                        textMeshProUGUI.text,
                        "(",
                        self.subPlayerIndex + 1,
                        ")"
                    });
                }
            }
        }

        public void ReadConfig()
        {
            if (NameOverride.Value == StringForCharacterName)
            {
                GetBodyName = true;
            }
            if (NameOverride.Value == StringForSkinName)
            {
                if (SkinNameFormatting.Value.Contains("{0}"))
                {
                    GetBodyName = true;
                }
                if (SkinNameFormatting.Value.Contains("{1}"))
                {
                    GetSkinName = true;
                }
            }
            if (DefaultSkinNameOverride.Value != "Keep")
            {
                ReplaceDefaultName = true;
            }
            if (!ShowHost.Value.IsNullOrWhiteSpace())
            {
                _logger.LogMessage("ShowHost Works");
                GetHostID = true;
                if (!ShowHost.Value.Contains("{0}"))
                {
                    _logger.LogMessage("ShowHost Defaulted");
                    hostFormattingString = $"{{0}} {ShowHost.Value}";
                }
                else
                {
                    _logger.LogMessage("ShowHost Using value");
                    hostFormattingString = ShowHost.Value;
                }
            }
        }

        public void SetupConfig()
        {
            NameOverride = Config.Bind("General Settings", "Default Name", "", $"The name all players will use. Leave empty to default to the survivor name, or to \"Skin\" to show their skin name.");
            BodyFallbackName = Config.Bind("General Settings", "Fallback Body Name", "Player", $"If it fails to default to the survivor name, then it will fallback to this name.");

            SkinFallbackName = Config.Bind("Skin Settings", "Fallback Skin Name", "Default", $"If it fails to get the current skin name, then it will fallback to this name.");
            SkinNameFormatting = Config.Bind("Skin Settings", "Skin Name Formatting", "{1} {0}", $"If \"Default Name\" is set to \"Skin\", then it will format their name as such:" +
                $"\n\"{{0}} = Survivor Name; {{1}} = SkinName\"" +
                $"\n\"{{1}} {{0}}\" = \"Admiral Captain\", \"Arctic Huntress\", \"Default Commando\", etc");
            DefaultSkinNameOverride = Config.Bind("Skin Settings", "Default Skin Name Override", "Keep", $"If the skin is the default skin, then it will be replaced with this name." +
                $"\nSet to \"Keep\" to not replace it.");
            NameUpdateFrequency = Config.Bind("Performance", "Name Update Frequency", 3f, "In seconds, of how often to update the name.");
            ShowHost = Config.Bind("General Settings", "Show Host", "{0} (Host)", "Appends the name after the name of the host, defaults to after the name if the {0} is missing. Leave empty to disable." +
                "\n {0} = Original Name Override" +
                "\nExample: \"{0} (Hoster)\" = \"Player (Hoster)\" | \"(Hoster)\" = \"Player (Hoster)\"" +
                "\n\"(Host) {0}\" = \"(Host) Player\"");
        }

        private void NetworkUser_OnDestroy(On.RoR2.NetworkUser.orig_OnDestroy orig, NetworkUser self)
        {
            orig(self);
            if (self && self.id.steamId != null && SteamID_to_DisplayName.ContainsKey(self.id.steamId))
            {
                SteamID_to_DisplayName.Remove(self.id.steamId);
            }
        }

        private void NetworkUser_Start(On.RoR2.NetworkUser.orig_Start orig, NetworkUser self)
        {
            orig(self);
            self.gameObject.AddComponent<UserRenamer>().networkUser = self;
        }

        private void NetworkUser_UpdateUserName(On.RoR2.NetworkUser.orig_UpdateUserName orig, NetworkUser self)
        {
            if (SteamID_to_DisplayName.TryGetValue(self.id.steamId, out string value))
            {
                self.userName = value;
                return;
            }
            orig(self);
        }

        private RoR2.NetworkPlayerName NetworkUser_GetNetworkPlayerName(On.RoR2.NetworkUser.orig_GetNetworkPlayerName orig, RoR2.NetworkUser self)
        {
            var nameOverride = NameOverride.Value;
            var bodyName = BodyFallbackName.Value;
            if (GetBodyName)
            {
                if (self.GetCurrentBody())
                {
                    bodyName = self.GetCurrentBody().GetDisplayName();
                }
                else
                {
                    if (BodyCatalog.GetBodyPrefab(self.bodyIndexPreference))
                    {
                        bodyName = BodyCatalog.GetBodyPrefabBodyComponent(self.bodyIndexPreference).GetDisplayName();
                    }
                }
            }
            /*if (false == true && !SteamworksLobbyManager.isInLobby)
            {
                if (GetBodyName || (!GetBodyName && GetSkinName))
                {
                    nameOverride = bodyName;
                }
                return new RoR2.NetworkPlayerName
                {
                    nameOverride = nameOverride,
                    steamId = self.id.steamId
                };
            }*/
            var skinName = SkinFallbackName.Value;
            SkinDef skinDef;
            bool isDefaultSkin = false;
            if (GetSkinName)
            {
                var body = self.GetCurrentBody();
                if (body)
                {
                    skinDef = SkinCatalog.FindCurrentSkinDefForBodyInstance(body.gameObject);
                    if (skinDef)
                    {
                        var skinDefs = SkinCatalog.FindSkinsForBody(body.bodyIndex);
                        skinName = Language.GetString(skinDef.nameToken);
                        isDefaultSkin = skinDefs[0] == skinDef;
                    }
                }
                else
                {
                    if (self.bodyIndexPreference >= 0)
                    {
                        var skinDefs = SkinCatalog.FindSkinsForBody(self.bodyIndexPreference);
                        if (skinDefs != null)
                        {
                            if (self.localUser?.userProfile?.loadout?.bodyLoadoutManager != null)
                            {
                                int skinIndex = (int)self.localUser.userProfile.loadout.bodyLoadoutManager.GetSkinIndex(self.bodyIndexPreference);

                                var userSkinDef = skinDefs[skinIndex];
                                //var sksadinIndex = SkinCatalog.FindLocalSkinIndexForBody(self.bodyIndexPreference, userSkinDef);
                                skinName = Language.GetString(userSkinDef.nameToken);
                                isDefaultSkin = skinDefs[0] == userSkinDef;
                            }
                        }
                    }
                }
                if (isDefaultSkin && ReplaceDefaultName)
                {
                    skinName = DefaultSkinNameOverride.Value;
                }
            }
            if (GetSkinName || GetBodyName)
            {
                nameOverride = "";
                if (GetSkinName)
                {
                    nameOverride = string.Format(SkinNameFormatting.Value, bodyName, skinName);
                }
                else if (GetBodyName)
                {
                    nameOverride = bodyName;
                }
            }

            if (self && self.id.steamId != null)
            {
                GetHost();
                if (GetHostID && self.id.steamId == HostSteamID)
                {
                    nameOverride = string.Format(hostFormattingString, nameOverride);
                }

                SteamID_to_DisplayName[self.id.steamId] = nameOverride;
            }

            return new RoR2.NetworkPlayerName
            {
                nameOverride = nameOverride,
                steamId = self.id.steamId
            };
        }

        private void GetHost()
        {
            if (Client.Instance != null && Client.Instance.Lobby != null && Client.Instance.Lobby.Owner != 0UL)
            {
                HostSteamID = new CSteamID(Client.Instance.Lobby.Owner);
                //_logger.LogMessage($"Host's Steam ID: {HostSteamID}");
                return;
            }
            HostSteamID = new CSteamID(0UL);
        }

        public class UserRenamer : MonoBehaviour
        {
            public NetworkUser networkUser;
            private float age;

            public CSteamID steamID;

            public void Start()
            {
                if (networkUser)
                {
                    steamID = networkUser.id.steamId;
                    networkUser.UpdateUserName();
                }
                age = NameUpdateFrequency.Value;
            }

            public void FixedUpdate()
            {
                age += Time.fixedDeltaTime;
                if (age >= NameUpdateFrequency.Value)
                {
                    if (networkUser)
                    {
                        if (SteamID_to_DisplayName.ContainsKey(steamID))
                        {
                            networkUser.UpdateUserName();
                            return;
                        }
                    }
                    age = 0;
                }
            }

            public void ManualUpdate()
            {
            }
        }
    }
}