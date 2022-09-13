using BepInEx;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using System.Runtime.CompilerServices;

namespace HUDPassiveSkillSlot
{
    [BepInPlugin("com.DestroyedClone.PassiveSkillIcon", "Passive Skill Icon", "1.0.0")]
    [BepInDependency("com.RiskyLives.RiskyMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class Class1 : BaseUnityPlugin
    {
        private static BodyIndex bandit2BodyIndex;

        public void Start()
        {
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskyLives.RiskyMod"))
            {
                RiskyModModCompat();
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void RiskyModModCompat()
        {
            RoR2Application.onLoad += () =>
            {
                var banditBodyPrefab = RoR2Content.Survivors.Bandit2.bodyPrefab;
                bandit2BodyIndex = RoR2Content.Survivors.Bandit2.bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                var genericSkills = banditBodyPrefab.GetComponents<GenericSkill>();
                var pointerComp = banditBodyPrefab.AddComponent<HUDPSS_GenericSkillPointer>();
                pointerComp.genericSkill = genericSkills[genericSkills.Length - 1];
                pointerComp.skillKeyText = "RiskyMod";
            };
        }

        public class HUDPSS_GenericSkillPointer : MonoBehaviour
        {
            public GenericSkill genericSkill = null;
            public string skillKeyText = "Misc";
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            if (self.skillIcons.Length > 1 && self.skillIcons[0] && self.skillIcons[1])
            {
                var passiveSkillIcon = UnityEngine.Object.Instantiate(self.skillIcons[0].gameObject, self.skillIcons[0].transform.parent);
                passiveSkillIcon.name = "PassiveSkillRoot";
                var oldSkillIcon = passiveSkillIcon.GetComponent<SkillIcon>();

                //This whole section is just to copy an existing UI element, delete whatever's
                //unnecessary, and then adjust what we want.
                //var iconDistance = Vector3.Distance(self.skillIcons[0].transform.position, self.skillIcons[1].transform.position);
                var iconDistance = 64.08698f;
                passiveSkillIcon.transform.position += new Vector3(+564, +151, +49);
                var comp = passiveSkillIcon.AddComponent<PassiveSkillIcon>();
                comp.hud = self;
                comp.iconImage = oldSkillIcon.iconImage;
                comp.tooltipProvider = oldSkillIcon.tooltipProvider;
                Destroy(oldSkillIcon.cooldownText.gameObject);
                Destroy(oldSkillIcon.cooldownRemapPanel.gameObject);
                Destroy(oldSkillIcon.stockText.transform.parent.gameObject);
                var skillKeyText = oldSkillIcon.transform.Find("SkillBackgroundPanel/SkillKeyText");
                Destroy(skillKeyText.GetComponent<InputBindingDisplayController>());
                skillKeyText.GetComponent<HGTextMeshProUGUI>().text = "Passive";
                Destroy(oldSkillIcon);
                //HG.ArrayUtils.ArrayAppend(ref self.skillIcons, in passiveSkillIcon);
                passiveSkillIcon.transform.localScale *= 0.9f;

                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskyLives.RiskyMod"))
                {
                    //see https://github.com/Moffein/RiskyMod/blob/6e010c28f1485ab6f15eedcc9934534aba4c4e1a/RiskyMod/Survivors/Bandit2/Components/SpecialDamageController.cs#L11
                    var riskySkillIcon = UnityEngine.Object.Instantiate(passiveSkillIcon, passiveSkillIcon.transform.parent);
                    riskySkillIcon.name = "RiskyModSkillRoot";
                    var riskyPassiveSkillIcon = riskySkillIcon.GetComponent<PassiveSkillIcon>();
                    var riskyComp = riskySkillIcon.AddComponent<TargetedSkillIcon>();
                    riskyComp.hud = self;
                    riskyComp.iconImage = riskyPassiveSkillIcon.iconImage;
                    riskyComp.tooltipProvider = riskyPassiveSkillIcon.tooltipProvider;
                    riskyComp.requiredBodyIndex = bandit2BodyIndex;
                    Destroy(riskyPassiveSkillIcon);
                    riskySkillIcon.transform.position = passiveSkillIcon.transform.position + Vector3.up * 130;
                    var riskySkillKeyText = riskySkillIcon.transform.Find("SkillBackgroundPanel/SkillKeyText");
                    riskyComp.skillKeyTextTMP = riskySkillKeyText.GetComponent<HGTextMeshProUGUI>();
                }
            }
        }

        public class TargetedSkillIcon : MonoBehaviour
        {
            public HUD hud;
            public GenericSkill targetSkill;
            public Image iconImage;
            public TooltipProvider tooltipProvider;
            public HGTextMeshProUGUI skillKeyTextTMP;
            public BodyIndex requiredBodyIndex = BodyIndex.None;

            private string CreateDesc(string descToken, string[] keywords)
            {
                if (keywords == null)
                {
                    return descToken;
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(Language.GetString(descToken));
                foreach (var kw in keywords)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(Language.GetString(kw));
                }
                return stringBuilder.ToString();
            }

            private void Update()
            {
                if (hud.targetBodyObject)
                {
                    SkillLocator skillLocator = hud.targetBodyObject.GetComponent<SkillLocator>();
                    if (skillLocator && skillLocator.primary)
                    {
                        if (requiredBodyIndex != BodyIndex.None && skillLocator.primary.characterBody.bodyIndex != requiredBodyIndex)
                        {
                            if (tooltipProvider)
                            {
                                tooltipProvider.bodyColor = Color.gray;
                                tooltipProvider.titleToken = "";
                                tooltipProvider.bodyToken = "";
                                tooltipProvider.enabled = false;
                            }
                            if (iconImage)
                            {
                                iconImage.enabled = false;
                                iconImage.sprite = null;
                            }
                            if (skillKeyTextTMP)
                            {
                                skillKeyTextTMP.text = "";
                            }
                            return;
                        }
                        string[] keywords = null;
                        var characterBody = skillLocator.primary.characterBody;

                        if (!targetSkill)
                        {
                            var comp = skillLocator.GetComponent<HUDPSS_GenericSkillPointer>();
                            targetSkill = comp.genericSkill;
                            skillKeyTextTMP.text = comp.skillKeyText;

                            if (targetSkill.skillDef.keywordTokens.Length > 0)
                            {
                                keywords = targetSkill.skillDef.keywordTokens;
                            }
                        }

                        //targetSkill = skillLocator.passiveSkill;
                        if (tooltipProvider)
                        {
                            Color color = characterBody.bodyColor;
                            SurvivorCatalog.GetSurvivorIndexFromBodyIndex(characterBody.bodyIndex);
                            Color.RGBToHSV(color, out float h, out float s, out float num);
                            num = ((num > 0.7f) ? 0.7f : num);
                            color = Color.HSVToRGB(h, s, num);
                            tooltipProvider.titleColor = color;
                            tooltipProvider.titleToken = targetSkill.skillNameToken;
                            tooltipProvider.bodyToken = CreateDesc(targetSkill.skillDescriptionToken, keywords);
                            tooltipProvider.enabled = true;
                        }
                        if (iconImage)
                        {
                            iconImage.enabled = true;
                            iconImage.color = Color.white;
                            iconImage.sprite = targetSkill.icon;
                        }
                        return;
                    }
                }
            }
        }

        public class PassiveSkillIcon : MonoBehaviour
        {
            public HUD hud;
            public SkillLocator.PassiveSkill targetSkill;
            public Image iconImage;
            public TooltipProvider tooltipProvider;

            private string CreateDesc(string descToken, string[] keywords)
            {
                if (keywords == null)
                {
                    return descToken;
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(Language.GetString(descToken));
                foreach (var kw in keywords)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(Language.GetString(kw));
                }
                return stringBuilder.ToString();
            }

            private void Update()
            {
                if (hud.targetBodyObject)
                {
                    SkillLocator skillLocator = hud.targetBodyObject.GetComponent<SkillLocator>();
                    if (skillLocator && skillLocator.primary)
                    {
                        var titleToken = "";
                        var descToken = "";
                        Sprite iconSprite = null;
                        string[] keywords = null;
                        bool passiveExists = false;
                        if (skillLocator.passiveSkill.enabled)
                        {
                            titleToken = skillLocator.passiveSkill.skillNameToken;
                            descToken = skillLocator.passiveSkill.skillDescriptionToken;
                            iconSprite = skillLocator.passiveSkill.icon;
                            keywords = new string[] { skillLocator.passiveSkill.keywordToken };
                            passiveExists = true;
                        } else
                        {
                            var firstSkill = hud.targetBodyObject.GetComponent<GenericSkill>();
                            if (firstSkill != skillLocator.primary)
                            {
                                titleToken = firstSkill.skillNameToken;
                                descToken = firstSkill.skillDescriptionToken;
                                iconSprite = firstSkill.icon;
                                keywords = firstSkill.skillDef.keywordTokens;
                                passiveExists = true;
                            }
                        }
                        if (!passiveExists)
                        {
                            if (tooltipProvider)
                            {
                                tooltipProvider.bodyColor = Color.gray;
                                tooltipProvider.titleToken = "";
                                tooltipProvider.bodyToken = "";
                                tooltipProvider.enabled = false;
                            }
                            if (iconImage)
                            {
                                iconImage.enabled = false;
                                iconImage.sprite = null;
                            }
                            return;
                        }
                        var characterBody = skillLocator.primary.characterBody;
                        //targetSkill = skillLocator.passiveSkill;
                        if (tooltipProvider)
                        {
                            Color color = characterBody.bodyColor;
                            SurvivorCatalog.GetSurvivorIndexFromBodyIndex(characterBody.bodyIndex);
                            Color.RGBToHSV(color, out float h, out float s, out float num);
                            num = ((num > 0.7f) ? 0.7f : num);
                            color = Color.HSVToRGB(h, s, num);
                            tooltipProvider.titleColor = color;
                            tooltipProvider.titleToken = titleToken;
                            tooltipProvider.bodyToken = CreateDesc(descToken, keywords);
                            tooltipProvider.enabled = true;
                        }
                        if (iconImage)
                        {
                            iconImage.enabled = true;
                            iconImage.color = Color.white;
                            iconImage.sprite = iconSprite;
                        }
                        return;
                    }
                }
            }
        }
    }
}