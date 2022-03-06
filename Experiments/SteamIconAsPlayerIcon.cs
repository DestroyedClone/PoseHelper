using System;
using BepInEx;
using RoR2;
using UnityEngine;


namespace SteamIconAsPlayerIcon
{
    [BepInPlugin("com.DestroyedClone.SteamIconAsPlayerIcon", "Steam Icon As Player Icon", "1.0.0")]
    public class SteamIconAsPlayerIcon : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.UI.ScoreboardStrip.FindMasterPortrait += ScoreboardStrip_FindMasterPortrait;
        }

        private UnityEngine.Texture ScoreboardStrip_FindMasterPortrait(On.RoR2.UI.ScoreboardStrip.orig_FindMasterPortrait orig, RoR2.UI.ScoreboardStrip self)
        {
			if (self.userBody)
			{
				return self.userBody.portraitIcon;
			}
			if (self.master)
			{
				GameObject bodyPrefab = self.master.bodyPrefab;
				if (bodyPrefab)
				{
					CharacterBody component = bodyPrefab.GetComponent<CharacterBody>();
					if (component)
					{
						return component.portraitIcon;
					}
				}
			}
			return null;
		}
    }
}
