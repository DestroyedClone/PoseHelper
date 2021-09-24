using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace ROR1AltSkills.Loader
{
    public class SwingComboFistAlt : ModifiedBasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public SwingComboFistAlt()
        {
        }
        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            this.gauntlet = i;
            Chat.AddMessage($"{gauntlet} v {i}");
        }

        public float comboFinisherDamageCoefficient = 2.4f;
        public Vector3 force = Vector3.up * 2f;
        public int gauntlet = 0;

        private bool IsComboFinisher
        {
            get
            {
                return gauntlet == 2;
            }
        }

        public override void OnEnter()
        {
            damageCoefficient = 1.2f;
            hitBoxGroupName = "Punch";
            hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");
            procCoefficient = 1;
            pushAwayForce = 1000;
            forceVector = new Vector3(1200.0f, 0.0f, 0.0f);
            hitPauseDuration = 0.1f;
            //swingEffectPrefab = "LoaderSwingBasic (UnityEngine.GameObject)"; //Resources.Load
            swingEffectMuzzleString = "";
            mecanimHitboxActiveParameter = "SwingFist.hitBoxActive";
            shorthopVelocityFromHit = 6;
            beginStateSoundString = "Play_loader_m1_swing";
            beginSwingSoundString = "";
            forceForwardVelocity = true;
            AnimationCurve animationCurve = new AnimationCurve(
                new Keyframe[]
                {
                    new Keyframe()
                    {
                        value = 0,
                        time = 0,
                        //tangentMode = 0, //obsolete
                        inTangent = 0.2531297f,
                        outTangent = Mathf.Infinity,
                        weightedMode = WeightedMode.None,
                        inWeight = 0,
                        outWeight = 0.3333333f,
                    },
                    new Keyframe()
                    {
                        value = 0.2f,
                        time = 0.2492953f,
                        //tangentMode = 0, //obsolete
                        inTangent = -1.34474f,
                        outTangent = -1.34474f,
                        weightedMode = WeightedMode.None,
                        inWeight = 0.3333333f,
                        outWeight = 0.09076658f,
                    },
                    new Keyframe()
                    {
                        value = 0,
                        time = 0.6705322f,
                        //tangentMode = 0, //obsolete
                        inTangent = -0.1023506f,
                        outTangent = -0.1023506f,
                        weightedMode =  WeightedMode.None,
                        inWeight = 0.7332441f,
                        outWeight = 0,
                    }

                })
            {
                preWrapMode = WrapMode.ClampForever,
                postWrapMode = WrapMode.ClampForever
            };
            forwardVelocityCurve = animationCurve;
            scaleHitPauseDurationAndVelocityWithAttackSpeed = true;
            ignoreAttackSpeed = false;
            base.OnEnter();
            if (IsComboFinisher)
            {
                damageCoefficient = comboFinisherDamageCoefficient;
                overlapAttack.pushAwayForce = 15f;
                overlapAttack.forceVector = Vector3.up;
            }
        }
        protected override void PlayAnimation()
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