using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AlternatePods
{
    public class Assets
    {
        //Prefabs
        public static GameObject genericPodPrefab;

        public static GameObject roboCratePodPrefab;
        public static GameObject batteryQuestPrefab;
        public static SkillDef defaultSkillDef;

        public static void SetupAssets()
        {
            roboCratePodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion();
            genericPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();

            defaultSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            defaultSkillDef.skillName = "PODMODDEFAULT";
            defaultSkillDef.skillNameToken = "PODMOD_SHARED_DEFAULT_NAME";
            defaultSkillDef.skillDescriptionToken = "PODMOD_SHARED_DEFAULT_DESC";
            (defaultSkillDef as ScriptableObject).name = defaultSkillDef.skillName;
            R2API.ContentAddition.AddSkillDef(defaultSkillDef);
        }
    }
}