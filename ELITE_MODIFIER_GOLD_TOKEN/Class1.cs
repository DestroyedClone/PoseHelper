using BepInEx;
using R2API;
using R2API.Utils;
using System.Collections.Generic;

namespace ELITE_MODIFIER_GOLD_TOKEN
{
    [BepInPlugin("com.DestroyedClone.AccuracyTest", "Accuracy Test", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]
    public class GoldTokenMain : BaseUnityPlugin
    {
        public void Start()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
            {
                {"ar" ,"{ذهبي {0" },
                {"bg" ,"Златен {0}" },
                {"zh-CN" ,"金的 {0}" },
                {"zh-TW" ,"金的 {0}" },
                {"cs" ,"Zlatý {0}" },
                {"da" ,"Gylden {0}" },
                {"nl" ,"gouden {0}" },
                {"en" ,"Golden {0}" },
                {"fi" ,"Kultainen {0}" },
                {"fr" ,"Doré {0}" }, //masculine
                {"de" ,"Golden {0}" },
                {"el" ,"Χρυσαφένιος {0}" },
                {"hu" ,"Aranysárga {0}" },
                {"it" ,"d'oro {0}" },
                {"ja" ,"ゴールデン {0}" },
                {"ko" ,"골든 {0}" },
                {"no" ,"gylden {0}" },
                {"pl" ,"Złoty {0}" },
                {"pt" ,"Dourado {0}" }, //masc
                {"pt-BR" ,"Dourado {0}" }, //brazilian
                {"ro" ,"De aur {0}" },
                {"ru" ,"Золотой {0}" },
                {"es" ,"Dorado {0}" },
                {"es-419" ,"Dorado {0}" },
                {"sv" ,"gyllene {0}" },
                {"th" ,"โกลเด้น {0}" },
                {"tr" ,"Altın {0}" },
                {"uk" ,"Золотий {0}" },
                {"vn" ,"Vàng {0}" },
            };

            var goldToken = "ELITE_MODIFIER_GOLD";
            foreach (var kvp in keyValuePairs)
            {
                LanguageAPI.Add(goldToken, kvp.Key, kvp.Value);
            }
        }
    }
}