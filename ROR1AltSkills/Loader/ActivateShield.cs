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
