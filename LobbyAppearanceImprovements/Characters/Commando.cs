
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using RoR2;
using R2API;

namespace LobbyAppearanceImprovements.Characters
{
    public class Commando : CharacterTemplate
    {
        public override SurvivorIndex SurvivorIndex => SurvivorIndex.Commando;
        public override string SurvivorName => "CommandoBody";
        public override Vector3 Position => new Vector3(2.65f, 0.01f, 6.00f);
        public override Vector3 Rotation => new Vector3(0f, 240f, 0f);
        public override CameraSetting CameraSetting => new CameraSetting()
        {
            fov = 20f,
            pitch = 2f,
            yaw = 24f
        };
    }
}
