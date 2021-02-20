using BepInEx;
using R2API.Utils;
using R2API;
using UnityEngine.Networking;
using RoR2;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using Path = System.IO.Path;
using R2API.Networking;
using UnityEngine.Playables;
using System;
using static UnityEngine.ScriptableObject;

namespace LeBuilder.Pasts
{
    public abstract class PastDef
    {
        public abstract string StageName { get; }
        public abstract string StageDesc { get; }
        public abstract string Soundtrack { get; }
        public abstract DirectorAPI.Stage Stage { get; }

        //public abstract DirectorAPI.DirectorCardHolder[] AllowedSpawns { get; }
        //public abstract DirectorAPI.DirectorCardHolder[] BannedSpawns { get; }
        protected abstract DirectorAPI.DirectorCardHolder[] AllowedSpawns();

        public abstract void BuildScene();

        public void ConfigureScene()
        {

        }

        public enum FilterTypes
        {
            None,
            OnlyAllowed,
            OnlyBanned
        }

    }
}
