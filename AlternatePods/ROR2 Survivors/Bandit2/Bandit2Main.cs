using R2API;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;

namespace AlternatePods
{
    public class Bandit2Main : PodModCharBase
    {
        public override GameObject bodyPrefab => 
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion();
        public override void AddPodsToPodChar()
        {
            podBases.Add(new PaintJob());
        }

        public override void Init()
        {
            base.Init();
        }
    }
}