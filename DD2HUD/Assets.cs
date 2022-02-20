using System;
using System.Collections.Generic;
using System.Text;

namespace DD2HUD
{
    internal static class Assets
    {        //{ new string[]{ "", "", "", "" }, "" },
        internal static Dictionary<string[], string> characterNames_to_teamName = new Dictionary<string[], string>()
        {
            // Todo: Organize
            // Category: Rank 4
            // Rank 4, RAnk 3, Rank 2, RAnk 1 : TeamName
            #region CaptainBody
            { new string[]{ "CaptainBody", "CaptainBody", "CaptainBody", "CaptainBody" }, "The Committee" },
            { new string[]{ "CaptainBody", "CommandoBody", "CommandoBody", "LoaderBody" }, "UES Authorized" },
            { new string[]{ "CaptainBody", "EngineerBody", "PaladinBody", "EnforcerBody" }, "Protectors" },
            { new string[]{ "CaptainBody", "MercBody", "CommandoBody", "MercBody" }, "Boy's Club" },
            // They all have a protection skill: Heal Beacon, Shield, Heal Utility, Shield+Bash
            #endregion

            #region CommandoBody
            { new string[]{ "CommandoBody", "CommandoBody", "CommandoBody", "CommandoBody" }, "Immeasurable Newcomers" },
            //Commando is the first character unlocked
            #endregion

            #region CrocoBody
            { new string[]{ "CrocoBody", "Bandit2Body", "LoaderBody", "MercBody" }, "Sliced Club" },
            { new string[]{ "CrocoBody", "CrocoBody", "CrocoBody", "CrocoBody" }, "The Kennel" },
            //Fight Club, each has a method of dealing melee damage.
            #endregion

            #region Bandit2Body
            { new string[]{ "Bandit2Body", "CommandoBody", "EnforcerBody", "LoaderBody" }, "The Unusual Suspects" },
            { new string[]{ "Bandit2Body", "Bandit2Body", "Bandit2Body", "Bandit2Body" }, "Yee Haw" },
            // The starting team for Risk of Rain 1
            { new string[]{ "Bandit2Body", "HuntressBody", "LoaderBody", "MercBody" }, "Reformed Crew" },
            // All survivors who have had a significant change to their kit (Overhaul, Orbs vs arrows, Mobility, Expose)
            #endregion

            #region CrocoBody
            #endregion

            //unsorted
            // All survivors authorized to enter the rescue mission

            //MageBody
            { new string[]{ "MageBody", "MageBody", "MageBody", "MageBody" }, "Hefty Hefty" },
            // ARTIFICER IS ___
            { new string[]{ "MageBody", "HuntressBody", "LoaderBody", "RailgunnerBody" }, "Sisters of Battle" },
            // All girls, name from DD2

            // Modded
            { new string[]{ "RailgunnerBody", "SniperClassicBody", "HuntressBody", "ToolbotBody" }, "Camping Cohorts" },
            { new string[]{ "SniperClassicBody", "MinerModBody", "CHEFBody", "EnforcerBody" }, "Abandoned Adventurers" },
        };
    }
}
