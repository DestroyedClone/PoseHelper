using EntityStates;
using RoR2;
using UnityEngine;

namespace VanillaDamageTyped.Modules
{
    public class BeetleQueen : CharacterMain
    {
        public static GameObject myCharacter;

        public override void GetBody()
        {
            myCharacter = Load<GameObject>("RoR2/Base/Beetle/BeetleQueen2Body.prefab");
        }

        public override void SetupBody()
        {
            var weakComp = myCharacter.GetComponent<SetStateOnWeakened>();
            if (!weakComp)
            {
                weakComp = myCharacter.AddComponent<SetStateOnWeakened>();
            }
            if (weakComp)
            {
                weakComp.characterBody = myCharacter.GetComponent<CharacterBody>();
                //weakComp.selfDamagePercentage = 0.25f;
                weakComp.hurtState = new SerializableEntityStateType(typeof(EntityStates.BeetleQueenMonster.WeakState));
            }
        }
    }
}