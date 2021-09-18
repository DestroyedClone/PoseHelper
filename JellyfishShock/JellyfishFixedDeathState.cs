using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using EntityStates.JellyfishMonster;

namespace JellyfishShock
{
    public class JellyfishFixedDeathState : DeathState
    {
        public override void PlayDeathAnimation(float crossfadeDuration = 0.1F)
        {
            return;
        }
    }
}
