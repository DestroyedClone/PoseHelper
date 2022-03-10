using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HealthbarImmune
{
    [BepInPlugin("com.DestroyedClone.HealthbarImmune", "Healthbar Immune", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]
    public class HealthbarImmunePlugin : BaseUnityPlugin
    {
        public static Color ImmuneColor = Color.yellow;
        public static string token = "IMMUNE_TO_DAMAGE_HITMARKER";
        public static string currentLanguageToken = "NOHIT";

        public void Awake()
        {
            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;

            Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
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

            var slash = self.transform.Find("Slash");
            if (!slash) return;
            var component = slash.GetComponent<RoR2.UI.HGTextMeshProUGUI>();
            if (!component) return;
            // the self.source check can be skipped because the original method returns if source is missing. probably
            if (component.text == currentLanguageToken)
            {
                if ((bool)self.source?.godMode
                        || (bool)self.source?.body?.HasBuff(RoR2Content.Buffs.HiddenInvincibility)
                        || (bool)self.source?.body?.HasBuff(RoR2Content.Buffs.Immune))
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