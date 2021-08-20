using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace LockedJartificer
{
    [BepInPlugin("com.DestroyedClone.LockedJartificer", "Locked Jartificer", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public static GameObject displayPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/LockedMage");
        public static GameObject wispJarDisplay = Resources.Load<GameObject>("prefabs/pickupmodels/PickupWilloWisp");
        public static GameObject glassArtifact = Resources.Load<GameObject>("prefabs/pickupmodels/artifacts/PickupGlass");
        public static ConfigEntry<bool> DisableUnlockableFilter { get; set; }

        public void Awake()
        {
            DisableUnlockableFilter = Config.Bind("Default", "Disable Unlockable Filter", true, "Disables the unlockable filter of the lockedmage so that she shows up even if you have her unlocked.");
            ModifyPrefab();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (arg0.name == "bazaar")
            {
                var oldMage = GameObject.Find("HOLDER: Store/HOLDER: Store Platforms/LockedMage");
                var oldPosition = oldMage.transform.position;
                var oldRotation = oldMage.transform.rotation;
                UnityEngine.Object.Destroy(oldMage);
                var newMage = UnityEngine.Object.Instantiate<GameObject>(displayPrefab);
                newMage.transform.position = oldPosition;
                newMage.transform.rotation = oldRotation;
                newMage.transform.parent = GameObject.Find("HOLDER: Store/HOLDER: Store Platforms").transform;
            }
        }

        public static void ModifyPrefab()
        {
            displayPrefab.GetComponent<GameObjectUnlockableFilter>().enabled = !DisableUnlockableFilter.Value;
            var iceMesh1 = displayPrefab.transform.Find("ModelBase/IceMesh");
            iceMesh1.name = "Jar";
            iceMesh1.transform.localPosition = new Vector3(0, -0.2f, 0);
            iceMesh1.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
            iceMesh1.transform.localScale = new Vector3(2, 2, 2);
            iceMesh1.GetComponent<MeshFilter>().sharedMesh = wispJarDisplay.transform.Find("mdlGlassJar/GlassJar").GetComponent<MeshFilter>().sharedMesh;
            iceMesh1.GetComponent<MeshRenderer>().sharedMaterial = glassArtifact.transform.Find("mdlArtifactSimpleCube").GetComponent<MeshRenderer>().sharedMaterial;
            var glassJarLid = wispJarDisplay.transform.Find("mdlGlassJar/GlassJarLid");
            var iceMesh2 = displayPrefab.transform.Find("ModelBase/IceMesh");
            iceMesh2.name = "JarLid";
            iceMesh2.transform.localPosition = new Vector3(0, -0.5f, 0);
            iceMesh2.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
            iceMesh2.transform.localScale = new Vector3(2, 2, 2);
            iceMesh2.GetComponent<MeshFilter>().sharedMesh = glassJarLid.GetComponent<MeshFilter>().sharedMesh;
            iceMesh2.GetComponent<MeshRenderer>().sharedMaterial = glassJarLid.GetComponent<MeshRenderer>().sharedMaterial;
            Debug.Log("6");
            displayPrefab.transform.Find("ModelBase/IceMesh (1)").gameObject.SetActive(false);
        }
    }
}