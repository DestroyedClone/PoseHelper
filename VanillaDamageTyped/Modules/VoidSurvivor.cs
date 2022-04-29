using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace VanillaDamageTyped.Modules
{
    public class VoidSurvivor : CharacterMain
    {
        public static GameObject myCharacter;

        public static SteppedSkillDef swingSkillDef;

        public override void GetBody()
        {
            myCharacter = Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab");
        }

        public override void SetupSkills()
        {
            swingSkillDef = Load<SteppedSkillDef>("RoR2/DLC1/VoidSurvivor/SwingMelee.asset");

            //LanguageAPI.Add(swingSkillDef.skillName, "Backup Shiv");
            //LanguageAPI.Add("VOIDSURVIVOR_PRIMARY_MELEE_DESCRIPTION", "Slice for <style=cIsDamage>220% damage</style>. <style=cIsHealing>Recover 7% health</style> on kill.");

            //ContentAddition.AddSkillDef(swingSkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = swingSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(swingSkillDef.skillNameToken, false, null)
            };
        }
    }
}