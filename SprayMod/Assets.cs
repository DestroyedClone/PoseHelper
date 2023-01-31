using BepInEx;
using R2API;
using R2API.Networking;
using RoR2;
using RoR2.VoidRaidCrab;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
//using ThreeEyedGames;
using ThreeEyedGames;

namespace SprayMod
{
    public class Assets
    {
        public static Material defaultSprayMaterial;
        public static Texture2D loadedTexture;
        public static GameObject defaultSprayObjectPrefab;

        public static void Setup()
        {
            Assets.loadedTexture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Achievements/texAttackSpeedIcon.png").WaitForCompletion();
            CreateMaterial();
            CreatePrefab();
        }

        public static void CreateMaterial()
        {
            var spitMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Beetle/matBeetleSpitLarge.mat").WaitForCompletion();
            defaultSprayMaterial = new Material(spitMaterial)
            {
                name = "texSprayMaterial",
                mainTexture = loadedTexture,
                shader = spitMaterial.shader,
                color = spitMaterial.color,
                doubleSidedGI = spitMaterial.doubleSidedGI,
                enableInstancing = spitMaterial.enableInstancing,
                globalIlluminationFlags = spitMaterial.globalIlluminationFlags,
                hideFlags = spitMaterial.hideFlags,
                mainTextureOffset = spitMaterial.mainTextureOffset,
                mainTextureScale = spitMaterial.mainTextureScale,
                renderQueue = spitMaterial.renderQueue,
                shaderKeywords = spitMaterial.shaderKeywords
            };
        }

        public static void CreatePrefab()
        {
            var acidPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenAcid.prefab").WaitForCompletion();
            defaultSprayObjectPrefab = PrefabAPI.InstantiateClone(acidPrefab, "SprayObjectPrefab");
            var fx = defaultSprayObjectPrefab.transform.Find("FX");
            var animShaderAlpha = fx.GetComponent<AnimateShaderAlpha>();
            UnityEngine.Object.Destroy(fx.Find("Sphere").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Hitbox").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Spittle").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Gas").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Point Light").gameObject);
            var decalObj = fx.Find("Decal");
            UnityEngine.Object.Destroy(defaultSprayObjectPrefab.GetComponent<RoR2.Projectile.ProjectileController>());
            UnityEngine.Object.Destroy(defaultSprayObjectPrefab.GetComponent<RoR2.Projectile.ProjectileDamage>());
            UnityEngine.Object.Destroy(defaultSprayObjectPrefab.GetComponent<RoR2.TeamFilter>());
            UnityEngine.Object.Destroy(defaultSprayObjectPrefab.GetComponent<RoR2.HitBoxGroup>());
            UnityEngine.Object.Destroy(defaultSprayObjectPrefab.GetComponent<RoR2.Projectile.ProjectileDotZone>());
            UnityEngine.Object.Destroy(defaultSprayObjectPrefab.GetComponent<RoR2.VFXAttributes>());
            UnityEngine.Object.Destroy(defaultSprayObjectPrefab.GetComponent<RoR2.DetachParticleOnDestroyAndEndEmission>());
            var decal = decalObj.GetComponent<Decal>();
            decal.Material = defaultSprayMaterial;
        }

    }
}
