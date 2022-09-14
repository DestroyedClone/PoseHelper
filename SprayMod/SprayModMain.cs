using BepInEx;
using R2API;
using RoR2;
using RoR2.VoidRaidCrab;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
using R2API;
using ThreeEyedGames;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace SprayMod
{
    [BepInPlugin("com.DestroyedClone.SprayMod", "SprayMod", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2API.Utils.R2APISubmoduleDependency(nameof(PrefabAPI), nameof(R2API.Utils.CommandHelper))]
    public class SprayModMain : BaseUnityPlugin
    {
        public static Material sprayMaterial;
        public static Texture2D loadedTexture;
        public static GameObject sprayObjectPrefab;

        public static SprayModMain instance;

        public void Start()
        {



            /*instance = this;
            loadedTexture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Achievements/texAttackSpeedIcon.png").WaitForCompletion();

            StartCoroutine(DownloadImage("https://gcdn.thunderstore.io/live/repository/icons/ThinkInvis-TinkersSatchel-2.2.0.png.128x128_q95.jpg"));

            CreateMaterial();
            CreatePrefab();

            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();*/
        }

        [ConCommand(commandName = "loadTex", flags = ConVarFlags.None, helpText = "loadTex [URL]")]
        public static void CCSpawnEncounter(ConCommandArgs args)
        {
            instance.DownloadImage(args.GetArgString(0));
            sprayMaterial.mainTexture = loadedTexture;
            sprayObjectPrefab.transform.Find("FX/Decal").GetComponent<Decal>().Material = sprayMaterial;
            Debug.Log("Updated spray material");
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.isPlayerControlled)
            {
                obj.gameObject.AddComponent<SprayComponent>().charBody = obj;
            }
        }

        public static void CreateMaterial()
        {
            var spitMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Beetle/matBeetleSpitLarge.mat").WaitForCompletion();
            sprayMaterial = new Material(spitMaterial)
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
            sprayObjectPrefab = PrefabAPI.InstantiateClone(acidPrefab, "SprayObjectPrefab");
            var fx = sprayObjectPrefab.transform.Find("FX");
            var animShaderAlpha = fx.GetComponent<AnimateShaderAlpha>();
            UnityEngine.Object.Destroy(fx.Find("Sphere").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Hitbox").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Spittle").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Gas").gameObject);
            UnityEngine.Object.Destroy(fx.Find("Point Light").gameObject);
            var decalObj = fx.Find("Decal");
            UnityEngine.Object.Destroy(sprayObjectPrefab.GetComponent<RoR2.Projectile.ProjectileController>());
            UnityEngine.Object.Destroy(sprayObjectPrefab.GetComponent<RoR2.Projectile.ProjectileDamage>());
            UnityEngine.Object.Destroy(sprayObjectPrefab.GetComponent<RoR2.TeamFilter>());
            UnityEngine.Object.Destroy(sprayObjectPrefab.GetComponent<RoR2.HitBoxGroup>());
            UnityEngine.Object.Destroy(sprayObjectPrefab.GetComponent<RoR2.Projectile.ProjectileDotZone>());
            UnityEngine.Object.Destroy(sprayObjectPrefab.GetComponent<RoR2.VFXAttributes>());
            UnityEngine.Object.Destroy(sprayObjectPrefab.GetComponent<RoR2.DetachParticleOnDestroyAndEndEmission>());
            var decal = decalObj.GetComponent<Decal>();
            decal.Material = sprayMaterial;
        }

        public class SprayComponent : MonoBehaviour
        {
            public GameObject sprayObjInstance;
            public CharacterBody charBody;

            public void OnDestroy()
            {
                if (sprayObjInstance)
                {
                    Destroy(sprayObjInstance);
                }
            }

            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (!sprayObjInstance)
                    {
                        sprayObjInstance = Object.Instantiate(sprayObjectPrefab);
                    }

                    if (charBody.inputBank.GetAimRaycast(1000f, out RaycastHit hitInfo))
                    {
                        sprayObjInstance.transform.SetPositionAndRotation(hitInfo.point,
                            Quaternion.Euler(hitInfo.normal));
                    }
                }
            }
        }

        IEnumerator<UnityWebRequestAsyncOperation> DownloadImage(string MediaUrl)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);
            else
                loadedTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}