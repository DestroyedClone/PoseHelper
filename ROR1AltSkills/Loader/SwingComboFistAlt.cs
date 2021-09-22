using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace ROR1AltSkills.Loader
{
    public class SwingComboFistAlt : EntityStates.Loader.SwingComboFist//EntityStates.Loader.LoaderMeleeAttack//, SteppedSkillDef.IStepSetter
    {
        public SwingComboFistAlt()
        {
            damageCoefficient = 1.2f;
            barrierPercentagePerHit = 0f;
        }
        public void SetStep(int i)
        {
            gauntlet = i;
        }

        public float comboFinisherDamageCoefficient = 2.4f;
        public Vector3 force = Vector3.up * 2f;

        private bool IsComboFinisher
        {
            get
            {
                return gauntlet == 2;
            }
        }

        public override void OnEnter()
        {
            if (IsComboFinisher)
            {
                damageCoefficient = comboFinisherDamageCoefficient;
                overlapAttack.pushAwayForce = 15f;
                overlapAttack.forceVector = Vector3.up;
            }
            base.OnEnter();
        }
        public override void PlayAnimation()
        {
            string animationStateName = "";
            float duration = Mathf.Max(this.duration, 0.2f);
            switch (gauntlet)
            {
                case 0:
                    animationStateName = "SwingFistRight";
                    break;

                case 1:
                    animationStateName = "SwingFistLeft";
                    break;

                case 2:
                    animationStateName = "BigPunch";
                    //base.PlayAnimation("FullBody, Override", "BigPunch", "BigPunch.playbackRate", duration); //BigPunch
                    break;
            }

            base.PlayCrossfade("Gesture, Additive", animationStateName, "SwingFist.playbackRate", duration, 0.1f);
            base.PlayCrossfade("Gesture, Override", animationStateName, "SwingFist.playbackRate", duration, 0.1f);
        }

        public override void BeginMeleeAttackEffect()
        {
            switch (gauntlet)
            {
                case 0:
                    swingEffectMuzzleString = "SwingRight";
                    break;

                case 1:
                    swingEffectMuzzleString = "SwingLeft";
                    break;
                case 2:
                    swingEffectMuzzleString = "SwingLeft";
                    break;
            }
            base.BeginMeleeAttackEffect();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)gauntlet);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.gauntlet = (int)reader.ReadByte();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}