using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace QuickerVotes
{
    [BepInPlugin("com.DestroyedClone.QuickerVotes", "Quicker Votes", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class QuickerVotesMain : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.VoteController.Awake += VoteController_Awake;
        }

        private void VoteController_Awake(On.RoR2.VoteController.orig_Awake orig, VoteController self)
        {
            orig(self);
            if (self.customName == "ReturnToCharacterSelect") self.Networktimer = 30;
        }
    }
}
