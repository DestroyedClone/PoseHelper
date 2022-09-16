using System;
using RoR2;
using UnityEngine;
using EntityStates.Toolbot;
using EntityStates;
using EntityStates.Merc;

namespace AlternateSkills.Mercenary
{
    public class LaserSaw : BaseSkillState
    {
        private OverlapAttack overlapAttack;


        public void OnEnter()
        {
            this.overlapAttack = new OverlapAttack();
            this.overlapAttack.attacker = base.gameObject;
            this.overlapAttack.inflictor = base.gameObject;
            this.overlapAttack.teamIndex = TeamComponent.GetObjectTeam(this.overlapAttack.attacker);
            this.overlapAttack.damage = FireBuzzsaw.damageCoefficientPerSecond * this.damageStat / FireBuzzsaw.baseFireFrequency;
            this.overlapAttack.procCoefficient = FireBuzzsaw.procCoefficientPerSecond / FireBuzzsaw.baseFireFrequency;

            if (base.characterDirection && base.inputBank)
            {
                base.characterDirection.forward = base.inputBank.aimDirection;
            }
            if (FireBuzzsaw.impactEffectPrefab)
            {
                this.overlapAttack.hitEffectPrefab = FireBuzzsaw.impactEffectPrefab;
            }
            Util.PlaySound(Uppercut.enterSoundString, base.gameObject);
            this.PlayAnim();
            this.overlapAttack.hitBoxGroup = HitBoxGroup.FindByGroupName(modelTransform.gameObject, groupName);
        }

        protected virtual void PlayAnim()
        {
            base.PlayCrossfade("FullBody, Override", "Uppercut", "Uppercut.playbackRate", this.duration, 0.1f);
        }

        public override void OnExit()
        {
            base.OnExit();
            base.PlayAnimation("FullBody, Override", "UppercutExit");
        }

    }
}
