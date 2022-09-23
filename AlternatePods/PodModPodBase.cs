using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Skills;
using UnityEngine.AddressableAssets;

namespace AlternatePods
{
    public abstract class PodModPodBase
    {
        public abstract string podName { get; }
        public abstract string podToken { get; }
        public abstract Texture2D icon { get; }
        public static GameObject podPrefab { get; set; }
        
        public virtual GameObject CreatePodPrefab()
        {
            return Assets.genericPodPrefab;
        }

        public virtual GameObject GetPodPrefab()
        {
            if (!podPrefab)
                podPrefab = CreatePodPrefab();
            return podPrefab;
        }

        public GameObject CreatePodRecolor(string podName, Material material)
        {
            var podPrefabInstance = PrefabAPI.InstantiateClone(Assets.genericPodPrefab, podName, true);
            //var childLoc = pod.transform.Find("Base/mdlEscapePod").GetComponent<ChildLocator>();

            var podObj = podPrefabInstance.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/EscapePodMesh");
            var meshFilter = podObj.GetComponent<MeshFilter>();
            var meshRenderer = podObj.GetComponent<MeshRenderer>();

            //var commandoMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Commando/matCommandoDualies.mat").WaitForCompletion();
            //meshRenderer.SetMaterial(material);
            meshRenderer.SetMaterialArray(new Material[]{material});
            return podPrefabInstance;
        }

        public GameObject CreatePodRecolor (string podName, string addressableKey)
        {
            return CreatePodRecolor(podName, Addressables.LoadAssetAsync<Material>(addressableKey).WaitForCompletion());
        }
    }
}