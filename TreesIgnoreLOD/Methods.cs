using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreesIgnoreLOD
{
    public static class Methods
    {
        public static string[] GetPathSet(string sceneName = null)
        {
            var attempt = PathSets.sceneName_to_pathSets.TryGetValue(sceneName, out string[] value);
            return attempt ? value : null;
        }

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

        public static void PatchScene(string[] chosenPathSet, int lodOverrideValue)
        {
            foreach (string path in chosenPathSet)
            {
                GameObject.Find(path).GetComponent<LODGroup>().ForceLOD(lodOverrideValue);
            }
        }
    }
}