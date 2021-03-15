using BepInEx;
using BepInEx.Configuration;
using LeTai.Asset.TranslucentImage;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using static UnityEngine.ColorUtility;
using static LobbyAppearanceImprovements.StaticValues;
using static LobbyAppearanceImprovements.LAIPlugin;

namespace LobbyAppearanceImprovements
{
    public static class Methods
    {
        public static void HideBackgroundCharacters(RoR2.UI.CharacterSelectController self)
        {
            var component = HasBackgroundComponent(self);
            if (component)
            {
                // Re-enable everything
                if (component.survivorDisplays.Count > 0)
                {
                    foreach (var backgroundCharacters in component.survivorDisplays)
                    {
                        backgroundCharacters.Value.SetActive(true);
                    }
                    // Now we can disable
                    foreach (var currentDisplays in self.characterDisplayPads)
                    {
                        var index = currentDisplays.displaySurvivorIndex;
                        component.survivorDisplays.TryGetValue(index, out GameObject objectToToggle);
                        if (objectToToggle)
                            objectToToggle.SetActive(false);
                        //selectedCharacters.Add(currentDisplays.displaySurvivorIndex);
                    }
                }
            }
        }

        public static void RefreshBackgroundCharacter(RoR2.UI.CharacterSelectController self)
        {
            var component = HasBackgroundComponent(self);
            if (component)
            {
                if (component.survivorDisplays.Count > 0)
                {
                    //List<SurvivorIndex> difference = new List<SurvivorIndex>();


                    foreach (var backgroundCharacters in component.survivorDisplays)
                    {
                        backgroundCharacters.Value.SetActive(true);
                    }
                    foreach (var currentDisplays in self.characterDisplayPads)
                    {
                        var index = currentDisplays.displaySurvivorIndex;
                        component.survivorDisplays.TryGetValue(index, out GameObject objectToToggle);
                        if (objectToToggle)
                        {
                            var delay = self.gameObject.AddComponent<LAI_Delayer>();
                            delay.characterDisplay = objectToToggle;
                        }
                        //selectedCharacters.Add(currentDisplays.displaySurvivorIndex);
                    }
                }
            }
        }


        public static GameObject CreateDisplay(string bodyPrefabName, Vector3 position, Vector3 rotation, Transform parent = null)
        {
            var bodyPrefab = GetBodyPrefab(bodyPrefabName);

            SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
            GameObject displayPrefab = survivorDef.displayPrefab;
            var gameObject = UnityEngine.Object.Instantiate<GameObject>(displayPrefab, position, Quaternion.Euler(rotation), parent);
            switch (bodyPrefabName)
            {
                case "Croco":
                    gameObject.transform.Find("mdlCroco")?.transform.Find("Spawn")?.transform.Find("FloorMesh")?.gameObject.SetActive(false);
                    break;
                case "RobEnforcer":
                    break;
                case "HANDOverclocked":
                    GameObject.Find("HANDTeaser").SetActive(false);
                    break;
            }
            return gameObject;
        }


        public static void UpdateSurvivorCache(RoR2.UI.CharacterSelectController self)
        {
            var component = HasBackgroundComponent(self);
            if (component)
            {
                if (component.survivorDisplays.Count > 0)
                {
                    component.currentlySelectedSurvivors.Clear();
                    foreach (var currentDisplays in self.characterDisplayPads)
                    {
                        var index = currentDisplays.displaySurvivorIndex;
                        component.currentlySelectedSurvivors.Add(index);
                    }
                }
            }
        }


        // Simple Methods
        public static void SetCamera(CameraRigController cameraRig, Characters.CameraSetting cameraSetting)
        {
            SetCamera(cameraRig, cameraSetting.fov, cameraSetting.pitch, cameraSetting.yaw);
        }
        public static void SetCamera(CameraRigController cameraRig, float fov = 60f, float pitch = 0f, float yaw = 0f)
        {
            cameraRig.baseFov = fov;
            cameraRig.currentFov += 30f;
            cameraRig.pitch = pitch;
            cameraRig.yaw = yaw;
        }
        public static void ChangeLobbyLightColor(Color32 color)
        {
            GameObject.Find("Directional Light").gameObject.GetComponent<Light>().color = color;
        }
        public static GameObject GetBodyPrefab(string bodyPrefabName)
        {
            switch (bodyPrefabName)
            {
                case "CHEF":
                    break;
                default:
                    bodyPrefabName += "Body";
                    break;
            }
            var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyPrefabName);
            if (!bodyPrefab) return null;
            return bodyPrefab;
        }
        public static LAI_BGCHARCOMP HasBackgroundComponent(RoR2.UI.CharacterSelectController self)
        {
            if (self && self.gameObject.GetComponent<LAI_BGCHARCOMP>())
            {
                return self.gameObject.GetComponent<LAI_BGCHARCOMP>();
            }
            else
                return null;
        }

        // Classes

        public class LAI_BGCHARCOMP : MonoBehaviour
        {
            public Dictionary<SurvivorIndex, GameObject> survivorDisplays = new Dictionary<SurvivorIndex, GameObject>();
            public List<SurvivorIndex> currentlySelectedSurvivors = new List<SurvivorIndex>();
        }
        public class LAI_Delayer : MonoBehaviour
        {
            float stopwatch = 0f;
            public GameObject characterDisplay;
            public void OnEnable()
            {
                characterDisplay?.SetActive(false);
            }
            public void Update()
            {
                stopwatch += Time.deltaTime;
                if (stopwatch >= 0.5f)
                {
                    characterDisplay?.SetActive(true);
                    Destroy(this);
                }
            }
        }
    }
}
