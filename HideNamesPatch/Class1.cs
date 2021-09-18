using System;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HideNamesPatch
{
    [BepInPlugin("com.DestroyedClone.NameOverrides", "Name Overrides", "1.0.2")]
    public class HideNames : BaseUnityPlugin
    {
		public ConfigEntry<string> NameOverride;
		public ConfigEntry<string> FallbackName;

		public static Dictionary<CSteamID, string> SteamID_to_DisplayName = new Dictionary<CSteamID, string>();

		public void Awake()
		{
			SetupConfig();

			On.RoR2.NetworkUser.GetNetworkPlayerName += NetworkUser_GetNetworkPlayerName;
            //On.RoR2.NetworkUser.UpdateUserName += NetworkUser_UpdateUserName;
            On.RoR2.NetworkUser.Start += NetworkUser_Start;
		}

        private void NetworkUser_Start(On.RoR2.NetworkUser.orig_Start orig, NetworkUser self)
        {
			orig(self);
			self.gameObject.AddComponent<CockSucker>().networkUser = self;
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
					} else
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

		public class CockSucker : MonoBehaviour
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
