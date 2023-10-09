using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HealthbarImmune
{
    [BepInPlugin("com.DestroyedClone.HealthbarImmune", "Healthbar Immune", "1.0.2")]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class HealthbarImmunePlugin : BaseUnityPlugin
    {
        public static ConfigEntry<string> cfgCharacterBlacklist;

        public static Color ImmuneColor = Color.yellow;
        public static string token = "IMMUNE_TO_DAMAGE_HITMARKER";
        public static string currentLanguageToken = "NOHIT";
        internal static BepInEx.Logging.ManualLogSource _logger;

        public static List<BodyIndex> bannedBodies = new List<BodyIndex>();

        public void Awake()
        {
            _logger = Logger;
            cfgCharacterBlacklist = Config.Bind("", "Character Blacklist", "", $"Blacklisted characters if needed. Use body name, and seperate by commas." +
                $"\n CommandoBody,HuntressBody");

            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;

            Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
                ModSupport_RiskOfOptions.Initialize();

            On.RoR2.BodyCatalog.Init += BodyCatalog_Init;
        }

        private void BodyCatalog_Init(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();
            SetupDictionary();
        }

        public static void SetupDictionary()
        {
            //Delimiter credit: https://github.com/KomradeSpectre/AetheriumMod/blob/c6fe6e8a30c3faf5087802ad7e5d88020748a766/Aetherium/Items/AccursedPotion.cs#L349
            _logger.LogMessage($"Setting up banned bodies.");
            bannedBodies.Clear();
            var valueArray = cfgCharacterBlacklist.Value.Split(',');
            var workingBodies = new List<string>();
            var failedBodies = new List<string>();
            if (valueArray.Length > 0)
            {
                foreach (string valueToTest in valueArray)
                {
                    if (valueToTest.IsNullOrWhiteSpace()) continue;
                    var bodyIndex = BodyCatalog.FindBodyIndex(valueToTest);
                    if (bodyIndex == BodyIndex.None)
                    {
                        failedBodies.Add(valueToTest);
                        continue;
                    }
                    bannedBodies.Add(bodyIndex);
                    workingBodies.Add(valueToTest);
                }
                if (workingBodies.Count > 0)
                {
                    var finalString = $"Successfully blacklisted: ";
                    foreach (var text in workingBodies)
                    {
                        finalString += $"{text}, ";
                    }
                    _logger.LogMessage(finalString);
                }
                if (failedBodies.Count > 0)
                {
                    var finalString = $"Failed to blacklist: ";
                    foreach (var text in failedBodies)
                    {
                        finalString += $"{text}, ";
                    }
                    _logger.LogMessage(finalString);
                }
            }
        }

        private void Language_onCurrentLanguageChanged()
        {
            if (Language.currentLanguage != null)
                currentLanguageToken = Language.GetString(token);
        }

        //https://github.com/Nebby1999/VarianceAPI/blob/59744f91ea25f061562e961371b02d1a9dd5bf19/VarianceAPI/Assets/VarianceAPI/Modules/Pickups/Items/PurpleHealthbar.cs
        private static void HealthBar_UpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            if (!self.source) return;
            if (!self.source.body) return;
            if (bannedBodies.Contains(self.source.body.bodyIndex)) return;

            var slash = self.transform.Find("Slash");
            if (!slash) return;
            var component = slash.GetComponent<RoR2.UI.HGTextMeshProUGUI>();
            if (!component) return;
            // the self.source check can be skipped because the original method returns if source is missing. probably
            if (component.text == currentLanguageToken)
            {
                if ((bool)self.source.godMode
                        || (bool)self.source.body.HasBuff(RoR2Content.Buffs.HiddenInvincibility)
                        || (bool)self.source.body.HasBuff(RoR2Content.Buffs.Immune))
                {
                    if (self.currentHealthText) self.currentHealthText.text = "";
                    if (self.fullHealthText) self.fullHealthText.text = "";
                    self.barInfoCollection.trailingOverHealthbarInfo.color = ImmuneColor;
                    return;
                }
                else //If you're no longer godmode and you still have immune text
                {
                    component.text = "/";
                    if (self.currentHealthText)
                    {
                        float num2 = Mathf.Ceil(self.source.combinedHealth);
                        self.displayStringCurrentHealth = num2;
                        self.currentHealthText.text = num2.ToString();
                    }
                    if (self.fullHealthText)
                    {
                        float num3 = Mathf.Ceil(self.source.fullHealth);
                        self.displayStringFullHealth = num3;
                        self.fullHealthText.text = num3.ToString();
                    }
                    return;
                }
            }

            bool changeColor = false;
            var healthComponent = self._source;
            if (healthComponent)
            {
                var characterBody = healthComponent.body;
                if (characterBody)
                {
                    changeColor = healthComponent.godMode
                        || characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility)
                        || characterBody.HasBuff(RoR2Content.Buffs.Immune);
                }
            }
            if (changeColor)
            {
                self.barInfoCollection.trailingOverHealthbarInfo.color = ImmuneColor;
                self.transform.Find("Slash").GetComponent<RoR2.UI.HGTextMeshProUGUI>().text = currentLanguageToken;
                if (self.currentHealthText) self.currentHealthText.text = "";
                if (self.fullHealthText) self.fullHealthText.text = "";

                if (self.scaleHealthbarWidth)
                {
                    float x = Util.Remap(
                        self.maxHealthbarWidth,
                        self.minHealthbarHealth,
                        self.maxHealthbarHealth,
                        self.minHealthbarWidth,
                        self.maxHealthbarWidth);
                    self.rectTransform.sizeDelta = new Vector2(x, self.rectTransform.sizeDelta.y);
                }
            }
        }
    }
}