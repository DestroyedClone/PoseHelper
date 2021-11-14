using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace ROR1AltSkills.Loader
{
    public class ActivateShield : BaseSkillState
    {
        public BuffDef speedBuff = RoR2Content.Buffs.CloakSpeed;
        public BuffDef armorBuff = RoR2Content.Buffs.Immune;
        public BuffDef pylonBuff => LoaderMain.PylonPoweredBuff.BuffDef;

        public float duration = 1f;
        public float speedBuffDuration = 1f;

        private List<CharacterBody> characterBodies;

        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active)
            {
                GetCorrectBuff();
                characterBodies = new List<CharacterBody>
                {
                    characterBody
                };

                if (LoaderMain.DebrisShieldAffectsDrones.Value && characterBody && characterBody.master && characterBody.master.deployablesList != null)
                {
                    foreach (var characterMaster in CharacterMaster.readOnlyInstancesList)
                    {
                        if (characterMaster.minionOwnership && characterMaster.minionOwnership.ownerMaster == characterBody.master)
                        {
                            var minionBody = characterMaster.GetBody();
                            if (minionBody && (minionBody.bodyFlags &= CharacterBody.BodyFlags.Mechanical) == CharacterBody.BodyFlags.Mechanical)
                                characterBodies.Add(characterMaster.GetBody());
                        }
                    }
                }

                characterBody.AddTimedBuff(pylonBuff, LoaderMain.DebrisShieldDuration.Value);
                foreach (var characterBody in characterBodies)
                {
                    ApplyImmuneBuff(characterBody);
                }
            }
        }

        public void GetCorrectBuff()
        {
            switch (LoaderMain.DebrisShieldSelectedMode.Value)
            {
                case LoaderMain.DebrisShieldMode.Immunity:
                    armorBuff = RoR2Content.Buffs.Immune;
                    break;
                case LoaderMain.DebrisShieldMode.Shield:
                    armorBuff = RoR2Content.Buffs.EngiShield;
                    break;
                case LoaderMain.DebrisShieldMode.Barrier:
                    armorBuff = LoaderMain.DebrisShieldBarrierBuff.BuffDef;
                    break;
            }
        }

        public virtual void ApplyImmuneBuff(CharacterBody characterBody)
        {
            if (characterBody)
            {
                characterBody.AddTimedBuff(armorBuff, LoaderMain.DebrisShieldDuration.Value);
                characterBody.AddTimedBuff(speedBuff, speedBuffDuration);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}