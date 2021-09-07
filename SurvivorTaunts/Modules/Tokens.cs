using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using R2API;

namespace SurvivorTaunts.Modules
{
    public static class Tokens
    {
        public static void AddTokens()
        {
            LanguageAPI.AddOverlay("DESTROYEDCLONE_TAUNTPLUGIN_CHATMESSAGE", "Chat ");
        }
    }
}
