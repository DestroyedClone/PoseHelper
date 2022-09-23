using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Skills;

namespace AlternatePods
{
    public abstract class PodModCharBase
    {
        public abstract GameObject bodyPrefab { get; }
        public virtual GenericSkill podSlot { get; private set;}
        public virtual List<PodModPodBase> podBases { get; set; } = new List<PodModPodBase>();

        public virtual void Init()
        {
            SetupPodSlot();
            AddPodsToPodChar();
            RegisterPods();
        }

        public virtual void AddPodsToPodChar()
        {
            if (podBases.Count == 0)
                AlternatePodsPlugin._logger.LogWarning($"AddPodsToPodChar: {bodyPrefab.name} has a PodModChar, but no pods are added?");
            return;
        }

        public void RegisterPods()
        {
            foreach (var pod in podBases)
            {
                string skillDefName = $"PodMod_{bodyPrefab.name}+{pod.podName}";
                var skillDef = CreateSkillDef(skillDefName, pod.podToken+"_NAME", pod.podToken+"_DESC");
                AddSkillDef(podSlot.skillFamily, skillDef, pod.GetPodPrefab());
            }
        }

        public virtual void SetupPodSlot()
        {
            podSlot = bodyPrefab.AddComponent<GenericSkill>();

            podSlot._skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            //podSlot.skillName
            (podSlot.skillFamily as ScriptableObject).name = "PodModSkillFamily";
            //ContentAddition.AddSkillFamily(passiveSlot.skillFamily);
            LoadoutAPI.AddSkillFamily(podSlot.skillFamily);
            podSlot.skillFamily.variants = new SkillFamily.Variant[1];
            podSlot.skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = Assets.defaultSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(Assets.defaultSkillDef.skillName, false, null)
            };
            bodyPrefab.AddComponent<AlternatePodsPlugin.PodModGenericSkillPointer>().podmodGenericSkill = podSlot;
        }
        
        public void AddSkillDef(SkillFamily skillFamily, SkillDef skillDef, GameObject podPrefab)
        {
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillName, false, null)
            };
            if (AlternatePodsPlugin.podName_to_podPrefab.ContainsKey(skillDef.skillName))
            {
                AlternatePodsPlugin._logger.LogError($"Duplicate skillName '{skillDef.skillName}' in '{bodyPrefab.name}'");
                UnityEngine.Object.Destroy(skillDef);
                return;
            }
            AlternatePodsPlugin.podName_to_podPrefab.Add(
                skillDef.skillName,
                podPrefab
            );
        }
        public static SkillDef CreateSkillDef(string skillName, string skillNameToken = null, string skillDescriptionToken = null)
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            //mySkillDef.activationState = null;
            //mySkillDef.icon = SurvivorSkillLocator.special.skillDef.icon;
            mySkillDef.skillName = skillName;
            mySkillDef.skillNameToken = skillNameToken == null ? skillName+"_NAME" : skillNameToken;
            mySkillDef.skillDescriptionToken = skillDescriptionToken == null ? skillName+"_DESC" : skillDescriptionToken;
            (mySkillDef as ScriptableObject).name = skillName;
            mySkillDef.keywordTokens = new string[]{};
            //ContentAddition.AddSkillDef(mySkillDef);
            LoadoutAPI.AddSkillDef(mySkillDef);
            return mySkillDef;
        }
    }
}