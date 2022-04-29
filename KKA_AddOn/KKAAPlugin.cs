using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections.Generic;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace KKA_AddOn
{
    [BepInPlugin("com.DestroyedClone.KKA_AddOn", "KingKombatArena AddOn", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class KKAAPlugin : BaseUnityPlugin
    {
        /* 1. Custom scenes like LobbyAppearanceImprovements
         * 2. Reduce size of billboarded particle effects due to blocking the screen
         * 3. 
         * 
         * 
         */

        public static ConfigEntry<float> particleSizeReduction;

        public static Dictionary<string, Diorama> dioramas = new Dictionary<string, Diorama>()
        {
            {"Arena", new Diorama(){
                dioramaObject = Resources.Load<GameObject>("prefabs/stagedisplay/ArenaDioramaDisplay"),
            }}
        };

        public static Language[] languages = null;

        public struct Diorama
        {
            public GameObject dioramaObject;
            public Vector3 positionOffset;
            public Vector3 rotation;
            public Vector3 scale;

            public void A()
            {

            }
        }

        public void Awake()
        {
            particleSizeReduction = Config.Bind("Visuals", "Particle Effect Size Multiplier", 0.5f, "Certain particle effects will get reduced in size." +
                "\nIncluding: ");
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            //CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.Language.Init += Language_Init;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            Run.onRunStartGlobal -= Run_onRunStartGlobal;
        }

        private string Language_GetString_string_string(On.RoR2.Language.orig_GetString_string_string orig, string token, string language)
        {
            Language currentLanguage = GetRandomLanguage();
            return (currentLanguage?.GetLocalizedStringByToken(token)) ?? token;
        }

        private string Language_GetString_string(On.RoR2.Language.orig_GetString_string orig, string token)
        {
            Language currentLanguage = GetRandomLanguage();
            return (currentLanguage?.GetLocalizedStringByToken(token)) ?? token;
        }

        private void Language_Init(On.RoR2.Language.orig_Init orig)
        {
            orig();

            List<Language> vs = new List<Language>();
            foreach (var entry in Language.GetAllLanguages())
            {
                entry.LoadStrings();
                vs.Add(entry);
            }
            languages = vs.ToArray();

            Logger.LogMessage("Language Count: "+languages.Length);

            //On.RoR2.Language.GetStringFormatted += Language_GetStringFormatted;
            //On.RoR2.Language.GetString_string += Language_GetString_string;
            //On.RoR2.Language.GetString_string_string += Language_GetString_string_string;
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            self = GetRandomLanguage();
            return orig(self, token);
        }

        private Language GetRandomLanguage()
        {
            if (languages != null)
            {
                var lang = languages[Random.Range(0, languages.Length - 1)];
                Logger.LogMessage($"requested language: {lang.selfName}");
                return lang;
            }
            else if (Language.GetAllLanguages() != null)
            {
                Logger.LogWarning("cum");
                return null;
            }
            return null;
        }

        private string Language_GetStringFormatted(On.RoR2.Language.orig_GetStringFormatted orig, string token, object[] args)
        {
            Language currentLanguage = GetRandomLanguage();
            return (currentLanguage?.GetLocalizedFormattedStringByToken(token, args)) ?? string.Format(token, args);
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (!obj.isPlayerControlled && obj.teamComponent.teamIndex == TeamIndex.Player)
            {
                var a = obj.gameObject.AddComponent<WeatherParticles>();
                a.lockPosition = true;
            }
        }


        [ConCommand(commandName = "spawnprefab", flags = ConVarFlags.ExecuteOnServer, helpText = "spawnprefab at your location {x} {y} {z}")]
        public static void ChangeLight(ConCommandArgs args)
        {
            var a = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(args.GetArgString(0)));
            a.transform.position = args.senderBody.corePosition;
        }
    }
}
