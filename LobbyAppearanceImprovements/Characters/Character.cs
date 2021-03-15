
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using RoR2;
using R2API;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;

namespace LobbyAppearanceImprovements.Characters
{
    public abstract class CharacterTemplate : MonoBehaviour
    {
        public abstract SurvivorIndex SurvivorIndex { get; }
        public abstract string SurvivorName { get; }

        //Position of the Character Display
        public abstract Vector3 Position { get; }
        //Rotation of the Character display
        public abstract Vector3 Rotation { get; }
        //Camera Setting for the Zoom option
        public virtual CameraSetting CameraSetting { get; } = new CameraSetting()
        {
            fov = 60f,
            pitch = 0f,
            yaw = 0f
        };

        protected CameraSetting GetCameraSetting()
        {
            return CameraSetting;
        }
    }

    //Camera Settings for the Zoom option
    public struct CameraSetting
    {
        public float fov;
        public float pitch;
        public float yaw;
    }
}
