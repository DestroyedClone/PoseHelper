using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Runtime.CompilerServices;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace BanditItemlet
{
    [BepInPlugin("com.DestroyedClone.Banditlet", "Banditlet", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class Main : BaseUnityPlugin
    {
        Dictionary<string, int> language_framerate = new Dictionary<string, int>()
        {
            {"es-419", 5 }
        };

        public void Start()
        {
            //Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
        }

        private void Language_onCurrentLanguageChanged()
        {
            if (Language.currentLanguage.name == "es-419")
            {
                Debug.Log("Mexico Simulator enabled");
                Application.targetFrameRate = 25;
            } else if (Language.currentLanguage.name == "pt-BR")
            {
                Debug.Log("Brazil Simulator enabled");
                Application.targetFrameRate = 5;
            } else
            {
                Application.targetFrameRate = -1; //uncapped
            }
        }
    }
}
