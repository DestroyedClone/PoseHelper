using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlternatePods
{
    public abstract class PodCharBase
    {
        public virtual GameObject BodyPrefab { get; set; }
        public abstract string BodyName { get; }

        public virtual string ConfigCategory
        {
            get
            {
                return "Pod: " + BodyName;
            }
        }

        public virtual GenericSkill PassiveSlot { get; set; }
        public virtual SkillFamily SkillFamily { get; set; }
        public virtual List<PodBase> PodBases { get; set; } = new List<PodBase>();

        public virtual void Init(ConfigFile config)
        {
            SetupConfig(config);
            CreatePassiveSlot();
            AssignPodBases(); //todo
            AddPods();
        }

        public virtual void AssignPodBases()
        {
        }

        public virtual void AddPods()
        {
            if (PodBases == null)
            {
                AlternatePodsPlugin._logger.LogError($"{BodyName}'s PodCharBase's PodBases is null! How?");
                return;
            }
            foreach (var pod in PodBases)
            {
                AlternatePodsPlugin.podName_to_podPrefab.Add(
                    pod.TokenPrefix,
                    pod.CreatePod()
                );
                AddSkillToSkillFamily(pod.Skill, SkillFamily);
                AlternatePodsPlugin._logger.LogMessage("Added pod prefab");
            }
        }

        private void AddSkillToSkillFamily(SkillDef skillDef, SkillFamily skillFamily)
        {
            //HG.ArrayUtils.ArrayAppend(ref MainPlugin.ContentPack.entityStateTypes, skillDef.activationState);
            ContentAddition.AddSkillDef(skillDef);

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        public virtual void SetupConfig(ConfigFile config)
        {
        }

        public virtual void CreatePassiveSlot()
        {
            if (!BodyPrefab)
                BodyPrefab = LegacyResourcesAPI.Load<GameObject>($"prefabs/CharacterBodies/{BodyName}Body");
            if (BodyPrefab)
            {
                PassiveSlot = BodyPrefab.AddComponent<GenericSkill>();

                SkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
                SkillFamily.variants = new RoR2.Skills.SkillFamily.Variant[0];
                BodyPrefab.AddComponent<AlternatePodsPlugin.PodModGenericSkillPointer>().podmodGenericSkill = PassiveSlot;
            }
        }
    }
}