using BepInEx.Configuration;
using RoR2.Skills;
using UnityEngine;

namespace AlternatePods
{
    public abstract class PodBase
    {
        public abstract string BodyName { get; }
        public abstract string PodName { get; }

        public virtual string ConfigCategory
        {
            get
            {
                return "Pod: " + BodyName;
            }
        }

        public virtual string PodPrefabName
        {
            get
            {
                return BodyName + PodName + "PodPrefab";
            }
        }

        public virtual string TokenPrefix { get; set; }
        public virtual SkillDef Skill { get; set; }

        public virtual void CreateTokenPrefix()
        {
            TokenPrefix = $"PODMOD_{BodyName.ToUpper()}_{PodName.ToUpper()}";
        }

        public virtual void Init(ConfigFile config)
        {
            SetupConfig(config);
            CreateTokenPrefix();
            Skill = CreateSkillDef();
        }

        public virtual void SetupConfig(ConfigFile config)
        {
        }

        public virtual SkillDef CreateSkillDef()
        {
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            //mySkillDef.activationState = null;
            //mySkillDef.icon = SurvivorSkillLocator.special.skillDef.icon;
            mySkillDef.skillName = TokenPrefix;
            mySkillDef.skillNameToken = $"{TokenPrefix}_NAME";
            mySkillDef.skillDescriptionToken = $"{TokenPrefix}_DESC";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;
            mySkillDef.keywordTokens = new string[] { };
            return mySkillDef;
        }

        public virtual GameObject CreatePod()
        {
            AlternatePodsPlugin._logger.LogWarning($"PodBase found for {BodyName} but no modifications were done to it! Assigning Robocrate.");
            return null;
        }

        public void GetMeshComponents(GameObject podPrefabInstance, out MeshFilter meshFilter, out MeshRenderer meshRenderer)
        {
            if (!podPrefabInstance)
            {
                AlternatePodsPlugin._logger.LogError("A pod was requested to be created, but its prefab is null!");
                meshFilter = null;
                meshRenderer = null;
                return;
            }
            var podObj = podPrefabInstance.transform.Find("Base/mdlEscapePod/EscapePodArmature/EscapePodMesh");
            meshFilter = podObj.GetComponent<MeshFilter>();
            meshRenderer = podObj.GetComponent<MeshRenderer>();
        }

        public void CreatePaintJob(Material material)
        {
        }
    }
}