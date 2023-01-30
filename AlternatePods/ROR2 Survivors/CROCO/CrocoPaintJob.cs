using R2API;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using System;

namespace AlternatePods
{
    public class CrocoPaintJob : PodModPodBase
    {
        public override string podName => "PaintJobCroco";
        public override string podToken => "PODMOD_SHARED_DEFAULT";
        public override Texture2D icon => Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Croco/CrocoBody.png").WaitForCompletion();

        public override GameObject CreatePodPrefab()
        {
            return CreatePodRecolor(podName, "RoR2/Base/Croco/matCroco.mat");
        }
    }
}