using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Skills;
using EntityStates;
using UnityEngine.AddressableAssets;

namespace AlternatePods
{
    public class CommandoPaintJob : PodBase
    {
        public override string BodyName => "Commando";
        public override string PodName => "PaintJob";
        //override tokenprefix? for generality?
        public override GameObject CreatePod()
        {
            var pod = PrefabAPI.InstantiateClone(Assets.genericPodPrefab, TokenPrefix);
            //var childLoc = pod.transform.Find("Base/mdlEscapePod").GetComponent<ChildLocator>();
            GetMeshComponents(pod,
             out MeshFilter meshFilter,
             out MeshRenderer meshRenderer);
            //var commandoBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
            //var commandoSkins = SkinCatalog.FindSkinsForBody(BodyCatalog.FindBodyIndex("CommandoBody"));
            //var commandoMainSkin = commandoSkins[0];
            //var commandoMat = commandoMainSkin.rendererInfos[0].defaultMaterial;

            var commandoMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Commando/matCommandoDualies.mat").WaitForCompletion();

            meshRenderer.SetMaterial(commandoMat);
            return base.CreatePod();
        }

        public override void CreateTokenPrefix()
        {
            TokenPrefix = $"PODMOD_SHARED_PAINTJOB";
        }

    }
}
