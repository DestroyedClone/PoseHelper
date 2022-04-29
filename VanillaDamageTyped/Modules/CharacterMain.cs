
using RoR2;
using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates;
using R2API;
using RoR2.Skills;
using RoR2;
using System;

namespace VanillaDamageTyped.Modules
{
    public class CharacterMain
    {

        public static T Load<T>(string assetPath)
        {
            var loadedAsset = Addressables.LoadAssetAsync<T>(assetPath).WaitForCompletion();
            return loadedAsset;
        }

        public virtual void GetBody()
        {

        }

        public virtual void Init()
        {
            GetBody();
            SetupSkills();
            SetupBody();
        }

        public virtual void SetupSkills()
        {

        }

        public virtual void SetupBody()
        {

        }

        public static void AddSkillToFamily(ref SkillFamily skillFamily, SkillDef skillDef)
        {
            if (!skillDef)
            {
                Debug.LogError($"Attempted to add null skilldef.");
                return;
            }

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }
    }
}
