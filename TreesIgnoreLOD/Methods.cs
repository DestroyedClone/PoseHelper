using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CollisionLODOverride.Main;

namespace CollisionLODOverride
{
    public static class Methods
    {
        public static string[] GetPathSet(string sceneName = null)
        {
            var attempt = PathSets.sceneName_to_pathSets.ContainsKey(sceneName);
            return attempt ? PathSets.sceneName_to_pathSets[sceneName] : null;
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

        public static bool PatchScene(string[] chosenPathSet, int lodOverrideValue)
        {
            if (chosenPathSet == null)
            {
                //_logger.LogWarning($"Could not find chosen pathSet for current scene ({UnityEngine.SceneManagement.SceneManager.GetActiveScene().name})!");
                return false;
            }
            _logger.LogMessage("Attempting to patch scene");
            foreach (string path in chosenPathSet)
            {
                var gameObj = GameObject.Find(path);
                if (gameObj)
                {
                    gameObj.GetComponent<LODGroup>().ForceLOD(lodOverrideValue);
                } else
                {
                    _logger.LogWarning($"Could not find GameObject with path {path}");
                }
            }
            _logger.LogMessage($"Overriding collideable LODGroups in scene using path set \"{nameof(chosenPathSet)}\" and value {lodOverrideValue}");
            return true;
        }

        public static void SetConfigSetting(int value)
        {
            var clamped = Mathf.Clamp(value, -1, 3);
            cfgLODOverride.Value = clamped;
        }
    }
}