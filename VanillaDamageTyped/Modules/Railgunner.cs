using RoR2;
using UnityEngine;

namespace VanillaDamageTyped.Modules
{
    public class Railgunner : CharacterMain
    {
        public static GameObject blindingMinePrefab;

        public override void GetBody()
        {
            blindingMinePrefab = Load<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAlt.prefab");
        }

        public override void SetupSkills()
        {
            base.SetupSkills();
            if (blindingMinePrefab)
            {
                if (blindingMinePrefab.GetComponent<BuffWard>())
                {
                    blindingMinePrefab.GetComponent<BuffWard>().buffDef = DLC1Content.Buffs.Blinded;
                }
            }
        }
    }
}