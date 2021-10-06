using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2.Skills;
using RoR2;

namespace ROR1AltSkills.Loader
{
    public class ActivateShield : BaseSkillState
    {
        public BuffDef speedBuff = RoR2Content.Buffs.CloakSpeed;
        public BuffDef armorBuff = RoR2Content.Buffs.Immune;

        public float duration = 1f;
        public float armorBuffDuration = 3f;
        public float speedBuffDuration = 1f;

        public override void OnEnter()
        {
            base.OnEnter();
            outer.commonComponents.characterBody.AddTimedBuff(armorBuff, armorBuffDuration);
            outer.commonComponents.characterBody.AddTimedBuff(speedBuff, speedBuffDuration);
            ApplyBuff(outer.commonComponents.characterBody);
            if (characterBody.master && LoaderMain.DebrisShieldAffectsDrones.Value)
            {
                foreach (var deployable in characterBody.master.deployablesList)
                {
                    if (deployable.deployable && deployable.deployable.GetComponent<CharacterBody>())
                    {
                        var characterBody = deployable.deployable.GetComponent<CharacterBody>();
                        if ((characterBody.bodyFlags &= CharacterBody.BodyFlags.Mechanical) == CharacterBody.BodyFlags.Mechanical)
                        {
                            ApplyBuff(characterBody);
                        }
                    }
                }
            }
        }

        public void ApplyBuff(CharacterBody characterBody)
        {
            if (characterBody)
            {
                characterBody.AddTimedBuff(armorBuff, armorBuffDuration);
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
