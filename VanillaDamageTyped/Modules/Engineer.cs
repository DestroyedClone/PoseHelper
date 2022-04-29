using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace VanillaDamageTyped.Modules
{
    public class Engineer : CharacterMain
    {
        public static GameObject myCharacter;

        public static EngiMineDeployerSkill engiMineDeployerSkill;

        //public static SkillDef shieldSD;
        public static SkillDef fireWallShieldSD;

        public static SkillDef selfShieldSD;
        public static SkillDef giveShieldSD;

        public static GenericSkill giveShieldGS;
        public static GenericSkill retractShieldGS;

        public override void GetBody()
        {
            myCharacter = Load<GameObject>("RoR2/Base/Engi/EngiBody.prefab");
        }

        public void CreateSkillDefs()
        {
            engiMineDeployerSkill = Load<EngiMineDeployerSkill>("RoR2/Junk/Engi/EntityStates.Engi.EngiWeapon.ThrowMineDeployer.asset");

            LanguageAPI.Add("ENGI_UTILITY_WALLSHIELD_NAME", "Wall Shield");
            LanguageAPI.Add("ENGI_UTILITY_WALLSHIELD_DESCRIPTION", "");

            fireWallShieldSD = ScriptableObject.CreateInstance<SkillDef>();
            fireWallShieldSD.activationState = new SerializableEntityStateType(typeof(EntityStates.Engi.EngiWeapon.FireWallShield));
            fireWallShieldSD.activationStateMachineName = "Weapon";
            fireWallShieldSD.baseMaxStock = 1;
            fireWallShieldSD.baseRechargeInterval = 7f;
            fireWallShieldSD.beginSkillCooldownOnSkillEnd = true;
            fireWallShieldSD.canceledFromSprinting = false;
            fireWallShieldSD.fullRestockOnAssign = true;
            fireWallShieldSD.interruptPriority = InterruptPriority.Any;
            fireWallShieldSD.isCombatSkill = true;
            fireWallShieldSD.mustKeyPress = true;
            fireWallShieldSD.rechargeStock = 1;
            fireWallShieldSD.requiredStock = 1;
            fireWallShieldSD.stockToConsume = 1;
            fireWallShieldSD.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            fireWallShieldSD.skillDescriptionToken = "ENGI_UTILITY_WALLSHIELD_DESCRIPTION";
            fireWallShieldSD.skillName = "ENGI_UTILITY_WALLSHIELD_NAME";
            fireWallShieldSD.skillNameToken = "ENGI_UTILITY_WALLSHIELD_NAME";

            (fireWallShieldSD as ScriptableObject).name = "EngiBodyFireWallShield";

            ContentAddition.AddSkillDef(fireWallShieldSD);

            LanguageAPI.Add("ENGI_UTILITY_SELFSHIELD_NAME", "Shield");
            LanguageAPI.Add("ENGI_UTILITY_SELFSHIELD_DESCRIPTION", "Gain <style=cIsUtility>+100% of your health as shield and immunity to knockback</style>. After casting," +
                "Target an ally within <style=cIsUtility>80m</style> to <style=cIsUtility>the shield to them</style>, granting them <style=cIsUtility>+50% health as shield</style>.");

            selfShieldSD = ScriptableObject.CreateInstance<SkillDef>();
            selfShieldSD.activationState = new SerializableEntityStateType(typeof(EntityStates.Engi.EngiWeapon.EngiSelfShield));
            selfShieldSD.activationStateMachineName = "Weapon";
            selfShieldSD.baseMaxStock = 1;
            selfShieldSD.baseRechargeInterval = 7f;
            selfShieldSD.beginSkillCooldownOnSkillEnd = true;
            selfShieldSD.canceledFromSprinting = false;
            selfShieldSD.fullRestockOnAssign = true;
            selfShieldSD.interruptPriority = InterruptPriority.Any;
            selfShieldSD.isCombatSkill = true;
            selfShieldSD.mustKeyPress = true;
            selfShieldSD.rechargeStock = 1;
            selfShieldSD.requiredStock = 1;
            selfShieldSD.stockToConsume = 1;
            selfShieldSD.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            selfShieldSD.skillDescriptionToken = "ENGI_SELF_SELFSHIELD_DESCRIPTION";
            selfShieldSD.skillName = "GiveShield";
            selfShieldSD.skillNameToken = "ENGI_SELF_SELFSHIELD_NAME";
            (selfShieldSD as ScriptableObject).name = "EngiBodyShieldSelf";

            ContentAddition.AddSkillDef(selfShieldSD);

            LanguageAPI.Add("ENGI_UTILITY_SELFSHIELD_NAME", "Retract Shield");
            LanguageAPI.Add("ENGI_UTILITY_SELFSHIELD_DESCRIPTION", "");

            giveShieldSD = ScriptableObject.CreateInstance<SkillDef>();
            giveShieldSD.activationState = new SerializableEntityStateType(typeof(EntityStates.Engi.EngiWeapon.EngiSelfShield));
            giveShieldSD.activationStateMachineName = "Weapon";
            giveShieldSD.baseMaxStock = 1;
            giveShieldSD.baseRechargeInterval = 7f;
            giveShieldSD.beginSkillCooldownOnSkillEnd = true;
            giveShieldSD.canceledFromSprinting = false;
            giveShieldSD.fullRestockOnAssign = true;
            giveShieldSD.interruptPriority = InterruptPriority.Any;
            giveShieldSD.isCombatSkill = true;
            giveShieldSD.mustKeyPress = true;
            giveShieldSD.rechargeStock = 1;
            giveShieldSD.requiredStock = 1;
            giveShieldSD.stockToConsume = 1;
            giveShieldSD.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            giveShieldSD.skillDescriptionToken = "ENGI_SELF_SELFSHIELD_DESCRIPTION";
            giveShieldSD.skillName = "RetractShield";
            giveShieldSD.skillNameToken = "ENGI_SELF_SELFSHIELD_NAME";
            (giveShieldSD as ScriptableObject).name = "EngiBodyShieldAlly";

            ContentAddition.AddSkillDef(giveShieldSD);
        }

        public override void SetupSkills()
        {
            CreateSkillDefs();

            #region existing

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var secondarySkillFamily = skillLocator.secondary.skillFamily;
            //AddSkillToFamily(ref secondarySkillFamily, engiMineDeployerSkill);

            var utilitySkillFamily = skillLocator.utility.skillFamily;

            AddSkillToFamily(ref utilitySkillFamily, fireWallShieldSD);
            AddSkillToFamily(ref utilitySkillFamily, selfShieldSD);
            /*
            Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
            utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = fireWallShieldSD,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(fireWallShieldSD.skillNameToken, false, null)
            };

            Array.Resize(ref utilitySkillFamily.variants, utilitySkillFamily.variants.Length + 1);
            utilitySkillFamily.variants[utilitySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = selfShieldSD,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(selfShieldSD.skillNameToken, false, null)
            };*/

            #endregion existing
        }

        public override void SetupBody()
        {
            base.SetupBody();
            giveShieldGS = myCharacter.AddComponent<GenericSkill>();
            giveShieldGS.skillName = "GiveShield";
            giveShieldGS.skillDef = selfShieldSD;

            retractShieldGS = myCharacter.AddComponent<GenericSkill>();
            retractShieldGS.skillName = "RetractShield";
            retractShieldGS.skillDef = giveShieldSD;
        }
    }
}