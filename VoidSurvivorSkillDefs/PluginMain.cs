using System;
using BepInEx;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace MyNameSpace
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(
        "com.MyName.IHerebyGrantPermissionToDeprecateMyModFromThunderstoreBecauseIHaveNotChangedTheName",
        "IHerebyGrantPermissionToDeprecateMyModFromThunderstoreBecauseIHaveNotChangedTheName",
        "1.0.0")]
    [R2APISubmoduleDependency(nameof(ContentAddition), nameof(LanguageAPI))]
    public class ExamplePlugin : BaseUnityPlugin
    {
        public static GameObject myCharacter;

        public void Start()
        {
            // myCharacter should either be
            // LegacyResourcesAPI.Load<GameObject>("RoR2/Base/Commando/CommandoBody");
            // or BodyCatalog.FindBodyIndex("CommandoBody");

            // If you are using Addressables (the preferred way)
            // You will need to have: using UnityEngine.AddressableAssets;
            // Addressables.LoadAssetAsync("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
            // 
            // For the last one, see https://xiaoxiao921.github.io/GithubActionCacheTest/assetPathsDump.html for paths
            myCharacter = LegacyResourcesAPI.Load<GameObject>("RoR2/Base/Commando/CommandoBody");

            // We add our skills in a separate method for cleaner code practice.
            AddSkills();
        }

        public void AddSkills()
        {
            LanguageAPI.Add("COMMANDO_UTILITY_POWEREXCHANGE_NAME", "Power Exchange");
            LanguageAPI.Add("COMMANDO_UTILITY_POWEREXCHANGE_DESCRIPTION", "<style=cIsUtility>Focus</style>, increasing <style=cIsDamage>damage</style>, <style=cIsDamage>critical hit chance</style>, but lose <style=cIsHealth>movement speed</style>.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            // Check step 2 for the code of the MyNameSpace.MyEntityStates.ExampleState class
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ExampleState));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = true;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("NotAnActualPath");
            mySkillDef.skillDescriptionToken = "CHARACTERNAME_SKILLSLOT_SKILLNAME_DESCRIPTION";
            mySkillDef.skillName = "CHARACTERNAME_SKILLSLOT_SKILLNAME_NAME";
            mySkillDef.skillNameToken = "CHARACTERNAME_SKILLSLOT_SKILLNAME_NAME";

            ContentAddition.AddSkillDef(mySkillDef);
            //This adds our skilldef. If you don't do this, the skill will not work.

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            //Note; if your character does not originally have a skill family for this, use the following:
            //skillLocator.special = gameObject.AddComponent<GenericSkill>();
            //var newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            //LoadoutAPI.AddSkillFamily(newFamily);
            //skillLocator.special.SetFieldValue("_skillFamily", newFamily);
            //var specialSkillFamily = skillLocator.special.skillFamily;


            //Note; you can change component.primary to component.secondary , component.utility and component.special
            var skillFamily = skillLocator.primary.skillFamily;

            //If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the exisiting Skill Family.
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)

            };

            //Note; if your character does not originally have a skill family for this, use the following:
            //skillFamily.variants = new SkillFamily.Variant[1]; // substitute 1 for the number of skill variants you are implementing

            //If this is the default/first skill, copy this code and remove the //,
            //skillFamily.variants[0] = new SkillFamily.Variant
            //{
            //    skillDef = mySkillDef,
            //    unlockableName = "",
            //    viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            //};
        }
    }
}