using System;
using System.Collections.Generic;
using System.Text;

namespace SurvivorTaunts.Emotes
{
    public class Display : BaseEmote
    {
        public override void OnEnter()
        {
            this.animString = "Dance";
            this.animDuration = 0.75f;
            this.soundString = "HenryDance";
            base.OnEnter();
        }
    }
}
