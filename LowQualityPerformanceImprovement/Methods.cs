using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Text;
using BepInEx;
using BepInEx.Logging;

namespace LowQualityPerformanceImprovement
{
    public static class Methods
    {

        // http://answers.unity.com/answers/8502/view.html
        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        public struct TransformInfo
        {
            public Transform parentTransform;
            public int childCount;
            public List<Transform> discoveredChildren;
            public bool allChildrenAreValid;
        }

        public static void PrintSceneCollisi2ons()
        {
            var transforms = UnityEngine.Object.FindObjectsOfType<Transform>();
            var total = 0;

            Dictionary<Transform, TransformInfo> keyValuePairs = new Dictionary<Transform, TransformInfo>();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"public static readonly string[] paths{RoR2.SceneCatalog.GetSceneDefForCurrentScene().cachedName} = new string[]");
            stringBuilder.AppendLine($"{{");

            foreach (var item in transforms.ToList())
            {
                var capsuleCollider = item.GetComponent<CapsuleCollider>();
                var rigidBody = item.GetComponent<Rigidbody>();
                var meshCollider = item.GetComponent<MeshCollider>();
                var sphereCollider = item.GetComponent<SphereCollider>();

                if (capsuleCollider || rigidBody || meshCollider || sphereCollider || item.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    continue;
                }
                if (item.GetComponent<MeshRenderer>() || item.GetComponent<MeshFilter>() || item.GetComponent<SkinnedMeshRenderer>())
                {
                    keyValuePairs.Add(item, new TransformInfo()
                    {
                        parentTransform = item.parent,
                        childCount = item.childCount,
                        discoveredChildren = new List<Transform>(),
                        allChildrenAreValid = false
                    }) ;
                    if (item.parent && keyValuePairs.ContainsKey(item.parent))
                    {
                        TransformInfo parentInfo = keyValuePairs[item.parent];
                        parentInfo.discoveredChildren.Add(item);
                        if (parentInfo.childCount == parentInfo.discoveredChildren.Count)
                        {
                            Debug.Log($"Parent {item.parent} has achieved validation for children. Removing.");
                            parentInfo.allChildrenAreValid = true;
                            foreach (var child in parentInfo.discoveredChildren)
                            {
                                keyValuePairs.Remove(child);
                            }
                        }
                    }
                }
            }

            foreach (var kvp in keyValuePairs)
            {
                total++;
                stringBuilder.AppendLine($"\"{GetGameObjectPath(kvp.Key.gameObject)}\",");
                kvp.Key.gameObject.SetActive(false);
            }

            stringBuilder.AppendLine($"}}");
            stringBuilder.AppendLine($"Total: {total}");
            Debug.Log(stringBuilder);
        }

        public static void PrintSceneCollisions(bool printPath = false)
        {
            var lodGroups = UnityEngine.Object.FindObjectsOfType<LODGroup>();
            var total = 0;
            var weak = 0;
            var solid = 0;

            var cachedPathList = new List<string>();
            foreach (var item in lodGroups)
            {//item.fadeMode == LODFadeMode.SpeedTree &&
                total++;
                var rigidBody = item.GetComponent<Rigidbody>();
                var rigidBodies = item.GetComponentInChildren<Rigidbody>();
                var meshCollider = item.GetComponent<MeshCollider>();
                var meshColliders = item.GetComponentInChildren<MeshCollider>();

                if (rigidBody || rigidBodies || meshCollider || meshColliders)
                {
                    solid++;
                    if (printPath)
                    {
                        var path = GetGameObjectPath(item.gameObject);
                        cachedPathList.Add($"\"{path}\"");
                    }
                    Debug.Log($"{GetGameObjectPath(item.gameObject)}");
                    //item.ForceLOD(cfgLODOverride.Value);
                }
                else
                {
                    weak++;
                }
            }
            Debug.Log($"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}: Collideable ({solid}), Not ({weak}), Total: {total}");
            if (printPath)
            {
                //https://stackoverflow.com/a/29575110
                string nameOfString = (string.Join(",\n", cachedPathList.Select(x => x.ToString()).ToArray()));
                nameOfString = "Paths To LODGroups:\n" + nameOfString;
                if (nameOfString.Length < 16384)
                {
                    Debug.Log(nameOfString);
                }
                else
                {
                    foreach (var value in cachedPathList)
                    {
                        Debug.Log(value);
                    }
                }
            }
        }
    }
}