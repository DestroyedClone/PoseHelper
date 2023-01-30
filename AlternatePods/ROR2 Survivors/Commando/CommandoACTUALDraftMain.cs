using R2API;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2.Skills;
using System;
using UnityEngine.AddressableAssets;
using System.Linq;

namespace AlternatePods
{
    public class CommandoACTUALDRAFTMain
    {
        public static GameObject bodyPrefab;
        public static SkillDef defaultSkillDef;
        public static SkillDef paintjobSkillDef;
        public static GameObject paintjobPrefab;

        public static void Init()
        {
            bodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();

            defaultSkillDef = CreateSkillDef("PODMOD_SHARED_NOPOD");
            paintjobSkillDef = CreateSkillDef("PODMOD_SHARED_PAINTJOB");

            var passiveSlot = bodyPrefab.AddComponent<GenericSkill>();

            passiveSlot._skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (passiveSlot.skillFamily as ScriptableObject).name = "PodModSkillFamily";
            //ContentAddition.AddSkillFamily(passiveSlot.skillFamily);
            LoadoutAPI.AddSkillFamily(passiveSlot.skillFamily);
            passiveSlot.skillFamily.variants = new SkillFamily.Variant[1];
            passiveSlot.skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = defaultSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(defaultSkillDef.skillName, false, null)
            };
            bodyPrefab.AddComponent<AlternatePodsPlugin.PodModGenericSkillPointer>().podmodGenericSkill = passiveSlot;

            //passiveSlot.defaultSkillDef = defaultSkillDef;

            paintjobPrefab = CreatePodRecolor("PODMOD_SHARED_PAINTJOB", Addressables.LoadAssetAsync<Material>("RoR2/Base/Commando/matCommandoDualies.mat").WaitForCompletion());
            AddSkillDef(passiveSlot.skillFamily, paintjobSkillDef, paintjobPrefab);
        }

        public static GameObject CreatePodRecolor(string tokenPrefix, Material material)
        {
            var podPrefabInstance = PrefabAPI.InstantiateClone(Assets.genericPodPrefab, tokenPrefix, true);
            //var childLoc = pod.transform.Find("Base/mdlEscapePod").GetComponent<ChildLocator>();

            var podObj = podPrefabInstance.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/EscapePodMesh");
            var meshFilter = podObj.GetComponent<MeshFilter>();
            var meshRenderer = podObj.GetComponent<MeshRenderer>();

            //var commandoMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Commando/matCommandoDualies.mat").WaitForCompletion();
            //meshRenderer.SetMaterial(material);
            meshRenderer.SetMaterialArray(new Material[]{material});
            return podPrefabInstance;
        }

        public static void AddSkillDef(SkillFamily skillFamily, SkillDef skillDef, GameObject podPrefab)
        {
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillName, false, null)
            };
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