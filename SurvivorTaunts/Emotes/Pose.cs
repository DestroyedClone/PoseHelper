using System;
using System.Collections.Generic;
using System.Text;

namespace SurvivorTaunts.Emotes
{
    public class Pose : BaseEmote
    {
        public override void OnEnter()
        {
            this.animString = "Pose";
            this.animDuration = 0.75f;
            this.soundString = "HenryDance";
            base.OnEnter();
        }
    }
}
