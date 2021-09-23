using UnityEngine;
using RoR2;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using System.Security;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;
using BepInEx;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ROR1AltSkills
{
    [BepInPlugin("com.DestroyedClone.OriginalSkills", "Original Skills", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI), nameof(ProjectileAPI), nameof(DamageAPI), nameof(BuffAPI), nameof(DotAPI))]
    public class OriginalSkillsPlugin : BaseUnityPlugin
    {
        internal static string modkeyword = "DC_ORIGSKILLS_KEYWORD_IDENTIFIER";
        public static Color ImmuneColor = Color.yellow;

        public void Awake()
        {
            SetupLanguage();

            Acrid.AcridMain.Init();
            Huntress.HuntressMain.Init();
            //Loader.LoaderMain.Init();


            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;
        }

        public void SetupLanguage()
        {
            LanguageAPI.Add(modkeyword, $"Original Skills Mod");
        }


        //https://github.com/Nebby1999/VarianceAPI/blob/59744f91ea25f061562e961371b02d1a9dd5bf19/VarianceAPI/Assets/VarianceAPI/Modules/Pickups/Items/PurpleHealthbar.cs
        private static void HealthBar_UpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            orig(self);
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
                self.barInfoCollection.healthBarInfo.color = ImmuneColor;
                self.currentHealthText.text = "IMMUNE";
                self.fullHealthText.text = "";

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
