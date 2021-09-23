using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using EntityStates.Commando;

namespace ROR1AltSkills.Commando
{
    public class TacticalDive : EntityStates.Commando.DodgeState
    {


        public override void OnEnter()
        {
            initialSpeedCoefficient = 0;
            finalSpeedCoefficient = 10f;

            base.OnEnter();
            if (base.outer.commonComponents.characterBody)
                base.outer.commonComponents.characterBody.AddTimedBuff(RoR2Content.Buffs.Immune, duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
