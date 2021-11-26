using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using EntityStates;
using System;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace SaveModdedProfileOnClose
{
    [BepInPlugin("com.DestroyedClone.Profile", "Profile", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    //[R2APISubmoduleDependency(nameof(PrefabAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource _logger;

        public static List<string> modifiedBodyNames = new List<string>();

        public void Start()
        {
            _logger = Logger;

            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            On.RoR2.ContentManagement.ContentPack.Copy += ContentPack_Copy;
        }

        private void ContentPack_Copy(On.RoR2.ContentManagement.ContentPack.orig_Copy orig, RoR2.ContentManagement.ContentPack src, RoR2.ContentManagement.ContentPack dest)
        {
            orig(src, dest);
            foreach (var survivorDef in src.survivorDefs)
            {
                if (survivorDef.bodyPrefab && survivorDef.bodyPrefab.GetComponent<SkillLocator>())
                {
                    var skillLoc = survivorDef.bodyPrefab.GetComponent<SkillLocator>();

                    var survivorName = (survivorDef.cachedName.IsNullOrWhiteSpace() ? survivorDef.bodyPrefab.name : survivorDef.cachedName);
                    survivorName = survivorName.Replace(" ", "_");

                    if (modifiedBodyNames.Contains(survivorName))
                        continue;

                    _logger.LogMessage($"Checking {survivorName}");


                    int index = 0;

                    void ApplyChange(GenericSkill genericSkill)
                    {
                        if (genericSkill && genericSkill.skillFamily)
                        {
                            var skillFamily = genericSkill.skillFamily as ScriptableObject;
                            if (skillFamily.name.IsNullOrWhiteSpace())
                            {
                                _logger.LogMessage($"SkillFamily {genericSkill.name} has been updated to:");
                                string newFamilyName = $"{survivorName}{index}Family";
                                skillFamily.name = newFamilyName;
                                _logger.LogMessage(skillFamily.name);
                                index++;
                            }
                        }

                        foreach (var variant in genericSkill.skillFamily.variants)
                        {
                            var skillDef = variant.skillDef;
                            if ((skillDef as ScriptableObject).name.IsNullOrWhiteSpace())
                            {
                                _logger.LogMessage($"SkillDef {skillDef.skillName} INDEX:{skillDef.skillIndex} has been updated to:");
                                (skillDef as ScriptableObject).name = skillDef.skillName;
                                _logger.LogMessage((skillDef as ScriptableObject).name);
                            }
                        }
                    }

                    ApplyChange(skillLoc.primary);
                    ApplyChange(skillLoc.secondary);
                    ApplyChange(skillLoc.utility);
                    ApplyChange(skillLoc.special);

                    modifiedBodyNames.Add(survivorName); //reduces log bloat
                }
            }
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            //Fuck();
        }

        public static void Fuck()
        {
            foreach (var survivorDef in SurvivorCatalog.allSurvivorDefs)
            {
                if (survivorDef.bodyPrefab && survivorDef.bodyPrefab.GetComponent<SkillLocator>())
                {
                    var skillLoc = survivorDef.bodyPrefab.GetComponent<SkillLocator>();

                    _logger.LogMessage($"Checking {survivorDef.cachedName}");

                    void ApplyChange(GenericSkill genericSkill)
                    {
                        if (genericSkill && genericSkill.skillFamily)
                        {
                            var skillFamily = genericSkill.skillFamily as ScriptableObject;
                            if (skillFamily.name.IsNullOrWhiteSpace())
                            {
                                _logger.LogMessage($"Updating {nameof(genericSkill)}\'s name.");
                                skillFamily.name = $"{survivorDef.cachedName ?? survivorDef.bodyPrefab.name}Family";
                                _logger.LogMessage(skillFamily.name);
                            }
                        }

                        foreach (var variant in genericSkill.skillFamily.variants)
                        {
                            var skillDef = variant.skillDef;
                            var skillDefScriptable = skillDef as ScriptableObject;
                            if (skillDefScriptable.name.IsNullOrWhiteSpace())
                            {
                                _logger.LogMessage($"Updated skill: {skillDef.skillName} w/ index {skillDef.skillIndex}");
                                (skillDefScriptable).name = skillDef.skillName;
                                _logger.LogMessage(skillDefScriptable.name);
                            }
                        }
                    }

                    ApplyChange(skillLoc.primary);
                    ApplyChange(skillLoc.secondary);
                    ApplyChange(skillLoc.utility);
                    ApplyChange(skillLoc.special);
                }
            }
        }

        public static void FuckOld()
        {
            _logger.LogMessage($"Evaluating survivorDefs: {SurvivorCatalog.allSurvivorDefs.Count()}");

            foreach (var survivorDef in SurvivorCatalog.allSurvivorDefs)
            {
                if (survivorDef.bodyPrefab && survivorDef.bodyPrefab.GetComponent<SkillLocator>())
                {
                    var skillLoc = survivorDef.bodyPrefab.GetComponent<SkillLocator>();
                    _logger.LogMessage($"{survivorDef.cachedName}:");

                    if (skillLoc.primary && skillLoc.primary.skillFamily)
                    {
                        _logger.LogMessage($"Primary Skill: {skillLoc.primary.skillName} + {((ScriptableObject)skillLoc.primary.skillFamily).name}");
                        foreach(var variant in skillLoc.primary.skillFamily.variants)
                        {
                            _logger.LogMessage($"{variant.skillDef.skillNameToken}");
                        }
                    }
                }
            }
        }
    }
}
