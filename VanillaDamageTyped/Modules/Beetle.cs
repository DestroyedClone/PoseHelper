using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Skills;

namespace VanillaDamageTyped.Modules
{
    public class Beetle : CharacterMain
    {
        public static GameObject myCharacter;

        public override void GetBody()
        {
            myCharacter = Load<GameObject>("RoR2/Base/Beetle/BeetleBody.prefab");
        }

        public override void SetupSkills()
        {
            SkillDef headbuttSD = Load<SkillDef>("RoR2/Base/Beetle/BeetleBodyHeadbutt.asset");
            headbuttSD.activationState = new SerializableEntityStateType(typeof(SleepState));
            headbuttSD.activationState = new SerializableEntityStateType(typeof(EntityStates.BeetleMonster.MeleeState));
        }

        public override void SetupBody()
        {
            var esMachines = myCharacter.GetComponents<EntityStateMachine>();
            foreach (var esm in esMachines)
            {
                if (esm.customName == "Body")
                {
                    esm.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BeetleMonster.MainState));
                    break;
                }
            }
        }
    }
}