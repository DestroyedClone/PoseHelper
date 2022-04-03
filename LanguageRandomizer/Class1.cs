using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace LanguageRandomizer
{
    [BepInPlugin("com.DestroyedClone.LanguageRandomizer", "Language Randomizer", "1.0.1")]
    public class RandomizerPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> cfgEnablePersistance;
        public static ConfigEntry<string> cfgAllowedLanguages;

        public static Language[] languages = null;
        public static Dictionary<string, string> alreadyRandomizedTokenDict = new Dictionary<string, string>();

        public void Awake()
        {
            cfgEnablePersistance = Config.Bind("", "Persistant Tokens", true, "If true, then the tokens used per language are stored upon generation.");
            cfgAllowedLanguages = Config.Bind("", "Allowed Languages", "en,es-419,de,FR,IT,ja,ko,pt-BR,RU,tr,zh-CN", "Enter the names of languages that you want to include in the randomization, separated by comma, and case-sensitive. Defaults to all if empty." +
                "\nen - English" +
                "\nes-419 - Spanish-Latin America (Español-España)" +
                "\nde - German (Deutsch)" +
                "\nFR - French (Français)" +
                "\nIT - Italian (Italiano)" +
                "\nja - Japanese (日本語)" +
                "\nko - Korean (한국어)" +
                "\npt-BR - Portuguese-Brazil (Português-Brasil)" +
                "\nRU - Russian (Русский)" +
                "\ntr - Turkish (Türkçe)" +
                "\nzh-CN - Chinese Simplified (简体中文)");

            if (cfgAllowedLanguages.Value.IsNullOrWhiteSpace())
            {
                cfgAllowedLanguages.Value = (string)cfgAllowedLanguages.DefaultValue;
            }

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            On.RoR2.Language.Init += Language_Init;
        }

        private string[] GetDelimitedAllowedLanguages()
        {
            var testArray = cfgAllowedLanguages.Value.Split(',');
            testArray = System.Array.ConvertAll(testArray, d => d.ToLower());
            return testArray;
        }

        private void Language_Init(On.RoR2.Language.orig_Init orig)
        {
            orig();

            List<Language> vs = new List<Language>();
            var delimitedLanguages = GetDelimitedAllowedLanguages().ToList();
            foreach (var a in delimitedLanguages)
            {
                Logger.LogMessage("Allowed Language: " + a);
            }
            foreach (var entry in Language.GetAllLanguages())
            {
                Logger.LogMessage($"Check: {entry.name.ToLowerInvariant()}");
                if (delimitedLanguages.Contains(entry.name.ToLowerInvariant()))
                {
                    Logger.LogMessage("Added.");
                    entry.LoadStrings();
                    vs.Add(entry);
                }
            }
            languages = vs.ToArray();

            if (cfgEnablePersistance.Value)
                On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
            else
                On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByTokenNotPersistent;
        }

        private string Language_GetLocalizedStringByTokenNotPersistent(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            return orig(GetRandomLanguage(), token);
        }

        private string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            //Logger.LogMessage(token);
            //Logger.LogMessage(alreadyRandomizedTokenDict.ContainsKey(token));
            if (alreadyRandomizedTokenDict.TryGetValue(token, out string localizedString))
                return localizedString;
            else
            {
                //Logger.LogMessage(alreadyRandomizedTokenDict.ContainsKey(token));
                var language = GetRandomLanguage(); //for logging

                localizedString = orig(language, token);
                if (!alreadyRandomizedTokenDict.ContainsKey(token))
                    alreadyRandomizedTokenDict.Add(token, localizedString);

                //if (language != RoR2.Language.currentLanguage) //for logging too
                //Logger.LogMessage($"Adding [({language}) {token} - {localizedString}");

                return localizedString;
            }
        }

        private Language GetRandomLanguage()
        {
            return languages[UnityEngine.Random.Range(0, languages.Length)];
            if (languages != null)
            {
                var lang = languages[UnityEngine.Random.Range(0, languages.Length - 1)];
                return lang;
            }
            else if (Language.GetAllLanguages() != null)
            {
                //Logger.LogWarning("cum");
                return null;
            }
            return null;
        }

        [ConCommand(commandName = "randlanguage_clear", flags = ConVarFlags.None, helpText = "randlanguage_clear - Clears the dictionary of generated tokens so they may be regenerated.")]
        public static void CCClearDictionary(ConCommandArgs args)
        {
            alreadyRandomizedTokenDict.Clear();
        }

        /*
        [ConCommand(commandName = "randlanguage_fix", flags = ConVarFlags.ExecuteOnServer, helpText = "randlanguage_fix - Removes entries from the dictionary of generated tokens that are not tokens.")]
        public static void CCRemoveLocalizaedStrings(ConCommandArgs args)
        {
            var tempDict = new Dictionary<string, string>(alreadyRandomizedTokenDict);
            int amt = 0;
            foreach (var entry in alreadyRandomizedTokenDict)
            {
                if (entry.Key.Contains(' '))
                {
                    tempDict.Remove(entry.Key);
                    amt++;
                }
            }
            alreadyRandomizedTokenDict = tempDict;
            UnityEngine.Debug.Log($"Removed {amt} non-token keys.");
        }*/
    }
}