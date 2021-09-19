using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace HideNamesPatch
{
    [BepInPlugin("com.DestroyedClone.HideNamesPatch", "HideNamesPatch", "1.0.0")]
    public class HideNames : BaseUnityPlugin
    {
        public ConfigEntry<string> NameOverride;
        public ConfigEntry<string> FallbackName;

        public static Dictionary<CSteamID, string> SteamID_to_DisplayName = new Dictionary<CSteamID, string>();

        public void Awake()
        {
            SetupConfig();

            On.RoR2.NetworkUser.GetNetworkPlayerName += NetworkUser_GetNetworkPlayerName;
            On.RoR2.NetworkUser.Start += NetworkUser_Start;
            On.RoR2.NetworkUser.OnDestroy += NetworkUser_OnDestroy;
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
            if (nameOverride == "")
            {
                if (self.GetCurrentBody())
                {
                    nameOverride = self.GetCurrentBody().GetDisplayName();
                }
                else
                {
                    if (BodyCatalog.GetBodyPrefab(self.bodyIndexPreference))
                    {
                        nameOverride = BodyCatalog.GetBodyPrefabBodyComponent(self.bodyIndexPreference).GetDisplayName();
                    }
                    else
                    {
                        nameOverride = FallbackName.Value;
                    }
                }
            }
            if (self && self.id.steamId != null)
            {
                SteamID_to_DisplayName[self.id.steamId] = nameOverride;
            }

            return new RoR2.NetworkPlayerName
            {
                nameOverride = nameOverride,
                steamId = self.id.steamId
            };
        }

        public void SetupConfig()
        {
            NameOverride = Config.Bind("General Settings", "Default Name", "", $"The name all players will use. Leave empty to default to the survivor name.");
            FallbackName = Config.Bind("General Settings", "Fallback Name", "Player", $"If it fails to default to the survivor name, then it will fallback to this name.");
        }

        public class UserRenamer : MonoBehaviour
        {
            public NetworkUser networkUser;

            public CSteamID steamID;

            public void Start()
            {
                if (networkUser)
                    steamID = networkUser.id.steamId;
            }

            public void FixedUpdate()
            {
                if (networkUser)
                {
                    if (SteamID_to_DisplayName.ContainsKey(steamID))
                    {
                        networkUser.UpdateUserName();
                        return;
                    }
                }
            }
        }
    }
}