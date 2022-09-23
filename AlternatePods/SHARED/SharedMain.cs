using R2API;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;

namespace AlternatePods
{
    public class SharedMain : PodModCharBase
    {
        public override GameObject bodyPrefab => null;
        public override void AddPodsToPodChar()
        {
        }

        public override void Init()
        {
            //empty to prevent running
        }
    }
}