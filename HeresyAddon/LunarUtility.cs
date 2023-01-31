using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.Bandit2.Weapon;
using EntityStates.GlobalSkills.LunarNeedle;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

namespace HeresyAddon
{
    public static class LunarUtility
    {
        public static void GhostUtilitySkillState_OnEnter(On.EntityStates.GhostUtilitySkillState.orig_OnEnter orig, GhostUtilitySkillState self)
        {
            orig(self);

            switch (self.characterBody.baseNameToken)
            {
                case "BANDIT2_BODY_NAME":
                    self.PlayAnimation("Gesture, Additive", "ThrowSmokebomb", "ThrowSmokebomb.playbackRate", EntityStates.Bandit2.ThrowSmokebomb.duration);
                    break;
                /*case "VOIDSURVIVOR_BODY_NAME":
                    self.PlayAnimation();
                    break;*/
                default:
                    break;
            }
        }
    }
}
