using BepInEx;
using RoR2;
using RoR2.Skills;
using System;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API;
using UnityEngine;
using R2API.Utils;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace WorkingVoidOutro
{
    [BepInPlugin("com.DestroyedClone.Neat", "Name", "1.0")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public static Action<GameObject[]> SceneAssetAPI_OutroAction;
        public static Action<GameObject[]> SceneAssetAPI_VoidOutroAction;

        public static GameObject SkipVoteController;
        public static GameObject VoidOutroHolder = null;

        public static void VoidOutro_Steal(GameObject[] gameObjects)
        {
            var tempGameObject = new GameObject();

            var banned = new string[] { "Music & Sound", "SkipVoteController" };
            foreach (var obj in gameObjects)
            {
                if (banned.Contains(obj.name))
                    continue;
                obj.transform.parent = tempGameObject.transform;
            }

            VoidOutroHolder = PrefabAPI.InstantiateClone(tempGameObject, "Void Outro Holder");
        }

        private void SceneAssetTest()
        {

        }

        public static void Outro_StealObjects(GameObject[] gameObjects)
        {
            bool voteController = false;


            foreach (var gameObject in gameObjects)
            {
                if (!voteController && gameObject.name == "SkipVoteController")
                {
                    SkipVoteController = gameObject.InstantiateClone("OutroVoteController");
                    continue;
                }
            }
        }

        public void Awake()
        {
            SceneAssetAPI_VoidOutroAction += VoidOutro_Steal;
            //SceneAssetAPI_OutroAction += Outro_StealObjects;
            //R2API.SceneAssetAPI.AddAssetRequest("outro", SceneAssetAPI_OutroAction);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            if (scene.name == "voidoutro" && VoidOutroHolder == null)
            {
                var tempGameObject = new GameObject();

                var banned = new string[] { "Music & Sound", "SkipVoteController" };
                foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (banned.Contains(obj.name))
                        continue;
                    obj.transform.parent = tempGameObject.transform;
                }

                VoidOutroHolder = PrefabAPI.InstantiateClone(tempGameObject, "Void Outro Holder");
            }


            if (scene.name == "outro")
            {
                Instantiate(VoidOutroHolder);
            }
        }
    }
}
