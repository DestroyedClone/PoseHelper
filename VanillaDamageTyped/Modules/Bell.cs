using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace VanillaDamageTyped.Modules
{
    public class Bell : CharacterMain
    {
        public static GameObject myCharacter;

        public override void GetBody()
        {
            myCharacter = Load<GameObject>("RoR2/Base/Bell/BellBody.prefab");
        }

        public override void SetupSkills()
        {
            LanguageAPI.Add("BELLBODY_SPECIAL_HEALBEAM_NAME", "Heal Beam");
            LanguageAPI.Add("BELLBODY_SPECIAL_HEALBEAM_DESCRIPTION", "<style=cIsDamage>Protect</style> an ally, granting them <style=cIsUtility>invulnerability</style>. <style=cIsHealth>Removed on death</style>.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Bell.BellWeapon.BuffBeam));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 7f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            mySkillDef.skillDescriptionToken = "BELLBODY_SPECIAL_HEALBEAM_DESCRIPTION";
            mySkillDef.skillName = "BELLBODY_SPECIAL_HEALBEAM_NAME";
            mySkillDef.skillNameToken = "BELLBODY_SPECIAL_HEALBEAM_NAME";
            (mySkillDef as ScriptableObject).name = "BellBodyHealBeam";

            ContentAddition.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();
            var skillFamily = skillLocator.primary.skillFamily;

            AddSkillToFamily(ref skillFamily, mySkillDef);

            //Note; if your character does not originally have a skill family for this, use the following:
            /*skillLocator.special = myCharacter.AddComponent<GenericSkill>();
            var newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            ContentAddition.AddSkillFamily(newFamily);
            skillLocator.special.SetFieldValue("_skillFamily", newFamily);
            var specialSkillFamily = skillLocator.special.skillFamily;

            ContentAddition.AddSkillFamily(newFamily);

            Array.Resize(ref specialSkillFamily.variants, specialSkillFamily.variants.Length + 1);
            specialSkillFamily.variants[specialSkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };*/
        }
    }
}