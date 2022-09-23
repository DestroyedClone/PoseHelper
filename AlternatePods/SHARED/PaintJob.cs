using R2API;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using System;

namespace AlternatePods
{
    public class PaintJob : PodModPodBase
    {
        public override string podName => "PaintJob";
        public override string podToken => "PODMOD_SHARED_DEFAULT";
        public override Texture2D icon => null;

        public override GameObject CreatePodPrefab()
        {
            var podPrefabInstance = PrefabAPI.InstantiateClone(Assets.genericPodPrefab, podName, true);
            //var childLoc = pod.transform.Find("Base/mdlEscapePod").GetComponent<ChildLocator>();

            var podObj = podPrefabInstance.transform.Find("Base/mdlEscapePod/EscapePodArmature/Base/EscapePodMesh");
            var meshFilter = podObj.GetComponent<MeshFilter>();
            var meshRenderer = podObj.GetComponent<MeshRenderer>();
            var module = podObj.gameObject.AddComponent<PodMod_PaintJobModule>();
            module.meshFilter = meshFilter;
            module.meshRenderer = meshRenderer;
            module.vehicleSeat = podPrefabInstance.GetComponent<VehicleSeat>();
            podPrefab = podPrefabInstance;
            return podPrefabInstance;
        }

        public class PodMod_PaintJobModule : MonoBehaviour
        {
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;
            public VehicleSeat vehicleSeat;
            public Material materialOverride = null;

            public void Start()
            {
                Material materialToSet = null;
                if (materialOverride)
                {
                    materialToSet = materialOverride;
                }
                else {
                    if (vehicleSeat && vehicleSeat.hasPassenger)
                    {
                        var body = vehicleSeat.currentPassengerBody;
                        
                        var bodySkin = SkinCatalog.GetSkinDef((SkinIndex)body.skinIndex);
                        var bodyMat = bodySkin.rendererInfos[0].defaultMaterial;
                    }
                }
                meshRenderer.SetMaterialArray(new Material[]{materialToSet});
            }
        }

    }
}