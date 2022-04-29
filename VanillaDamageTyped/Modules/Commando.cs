using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace VanillaDamageTyped.Modules
{
    public class Commando : CharacterMain
    {
        public static GameObject myCharacter;

        public static SkillDef sweepBarrageSD;
        public static SkillDef throwStickyGrenadeSD;
        public static SkillDef fireShrapnelSD;

        public override void GetBody()
        {
            myCharacter = Load<GameObject>("RoR2/Base/Commando/CommandoBody.prefab");
        }

        public void CreateSkillDefs()
        {
            sweepBarrageSD = Load<SkillDef>("RoR2/Junk/Commando/CommandoBodySweepBarrage.asset");
            throwStickyGrenadeSD = Load<SkillDef>("RoR2/Junk/Commando/ThrowStickyGrenade.asset");

            LanguageAPI.Add("COMMANDO_SECONDARY_FIRESHRAPNEL_NAME", "Fire Shrapnel");
            LanguageAPI.Add("COMMANDO_SECONDARY_FIRESHRAPNEL_DESCRIPTION", "Fire shrapnel");

            fireShrapnelSD = ScriptableObject.CreateInstance<SkillDef>();
            fireShrapnelSD.activationState = new SerializableEntityStateType(typeof(EntityStates.Commando.CommandoWeapon.FireShrapnel));
            fireShrapnelSD.activationStateMachineName = "Weapon";
            fireShrapnelSD.baseMaxStock = 1;
            fireShrapnelSD.baseRechargeInterval = 7f;
            fireShrapnelSD.beginSkillCooldownOnSkillEnd = true;
            fireShrapnelSD.canceledFromSprinting = false;
            fireShrapnelSD.fullRestockOnAssign = true;
            fireShrapnelSD.interruptPriority = InterruptPriority.Any;
            fireShrapnelSD.isCombatSkill = true;
            fireShrapnelSD.mustKeyPress = true;
            fireShrapnelSD.rechargeStock = 1;
            fireShrapnelSD.requiredStock = 1;
            fireShrapnelSD.stockToConsume = 1;
            fireShrapnelSD.icon = Resources.Load<Sprite>("textures/achievementicons/texAttackSpeedIcon");
            fireShrapnelSD.skillDescriptionToken = "COMMANDO_SECONDARY_FIRESHRAPNEL_DESCRIPTION";
            fireShrapnelSD.skillName = "COMMANDO_SECONDARY_FIRESHRAPNEL_NAME";
            fireShrapnelSD.skillNameToken = "COMMANDO_SECONDARY_FIRESHRAPNEL_NAME";
            (fireShrapnelSD as ScriptableObject).name = "CommandoBodyFireShrapnel";

            ContentAddition.AddSkillDef(fireShrapnelSD);
        }

        public override void SetupSkills()
        {
            CreateSkillDefs();

            #region existing

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var secondarySkillFamily = skillLocator.secondary.skillFamily;
            Array.Resize(ref secondarySkillFamily.variants, secondarySkillFamily.variants.Length + 1);
            secondarySkillFamily.variants[secondarySkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = fireShrapnelSD,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(fireShrapnelSD.skillNameToken, false, null)
            };

            var specialSkillFamily = skillLocator.special.skillFamily;
            specialSkillFamily.defaultSkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Commando.CommandoWeapon.PrepBarrage));

            Array.Resize(ref specialSkillFamily.variants, specialSkillFamily.variants.Length + 1);
            specialSkillFamily.variants[specialSkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sweepBarrageSD,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(sweepBarrageSD.skillNameToken, false, null)
            };

            Array.Resize(ref specialSkillFamily.variants, specialSkillFamily.variants.Length + 1);
            specialSkillFamily.variants[specialSkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = throwStickyGrenadeSD,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(throwStickyGrenadeSD.skillNameToken, false, null)
            };

            #endregion existing
        }
    }
}