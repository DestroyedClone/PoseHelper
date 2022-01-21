using EntityStates.GlobalSkills.LunarDetonator;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace HeresyAddon
{
    public class LunarSpecial
    {
        public struct PlayAnimParams
        {

            public PlayAnimParams(string AnimationLayerName, string AnimationStateName, string PlaybackRateParam, float Duration) : this()
            {
                animationLayerName = AnimationLayerName;
                animationStateName = AnimationStateName;
                playbackRateParam = PlaybackRateParam;
                duration = Duration;
            }

            public string animationLayerName;
            public string animationStateName;
            public string playbackRateParam;
            public float duration;
        }

        public static Dictionary<string, PlayAnimParams> animOverrideParams = new Dictionary<string, PlayAnimParams>()
        {
            // negative = multiply this number by the duration
            {"COMMANDO_BODY_NAME" , new PlayAnimParams("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", -2f)},
            {"CROCO_BODY_NAME" , new PlayAnimParams("Gesture, Additive", "Slash1", "Slash.playbackRate", -1)},
            {"MAGE_BODY_NAME", new PlayAnimParams("Gesture", "Additive", "FireWall", -1f) },
            //{"BANDIT2_BODY_NAME" , new PlayAnimParams()},
            {"CAPTAIN_BODY_NAME" , new PlayAnimParams("Gesture, Override", "CallAirstrike1", "Slash.playbackRate", -1f)},
            {"ENGI_BODY_NAME" , new PlayAnimParams("Gesture", "PlaceTurret", "Slash.playbackRate", -1f)},
            //{"HUNTRESS_BODY_NAME" , new PlayAnimParams()},
            //{"LOADER_BODY_NAME" , new PlayAnimParams()},
            {"MERC_BODY_NAME" , new PlayAnimParams("FullBody, Override", "Uppercut", "Uppercut.playbackRate", -1f)},
            {"TOOLBOT_BODY_NAME" , new PlayAnimParams()},
            {"TREEBOT_BODY_NAME" , new PlayAnimParams()},
            //{"HERETIC_BODY_NAME" , new PlayAnimParams()},
        };

        public static void Detonate_OnEnter(On.EntityStates.GlobalSkills.LunarDetonator.Detonate.orig_OnEnter orig, EntityStates.GlobalSkills.LunarDetonator.Detonate self)
        {
            //cache
            PlayAnimParams oldParams = new PlayAnimParams(self.animationLayerName, self.animationStateName, self.playbackRateParam, self.duration);
            //var oldAnimName = self.animationLayerName;
            //var oldStateName = self.animationStateName;
            //var oldPlaybackRate = self.playbackRateParam;
            //var oldDuration = self.duration;

            void modify(PlayAnimParams playAnimParams)
            {
                self.animationLayerName = playAnimParams.animationLayerName;
                self.animationStateName = playAnimParams.animationStateName;
                self.playbackRateParam = playAnimParams.playbackRateParam;
                self.duration = playAnimParams.duration > 0 ? playAnimParams.duration : oldParams.duration * -playAnimParams.duration;
            }

            if (animOverrideParams.TryGetValue(self.characterBody.baseNameToken, out PlayAnimParams value))
            {
                modify(value);
            }

            /*void modify(string animationLayerName, string animationStateName, string playbackRateParam, float duration)
            {
                self.animationLayerName = animationLayerName;
                self.animationStateName = animationStateName;
                self.playbackRateParam = playbackRateParam;
                self.duration = duration;
            }*/


            orig(self);

            //restore
            modify(oldParams);
        }

    }
}
