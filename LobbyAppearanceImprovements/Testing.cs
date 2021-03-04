using BepInEx;
using R2API.Utils;
using RoR2;
using BepInEx.Configuration;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.ColorUtility;
using static LobbyAppearanceImprovements.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using R2API;
using UnityEngine.Networking;
using System.Reflection;
using Path = System.IO.Path;
using R2API.Networking;
using UnityEngine.Playables;
using System;
using static UnityEngine.ScriptableObject;
using System.Linq;
using System.Collections.Generic;
using EntityStates;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using RoR2.Projectile;
using static UnityEngine.Animator;
using LeTai.Asset.TranslucentImage;

namespace LobbyAppearanceImprovements
{
    public class Testing
    {

        /*foreach (var entry in textCameraSettings) //move to survivor catalog setup?
        {
            var bodyPrefab = GetBodyPrefab(entry.Key);
            if (bodyPrefab)
            {
                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                SurvivorIndex survivorIndex = survivorDef.survivorIndex;
                textCameraSettings.TryGetValue(entry.Key, out float[] cameraSetting);
                characterCameraSettings.Add(survivorIndex, cameraSetting);
            } else
            {
            }
        }
        foreach (var entry in characterCameraSettings)
        {
            Debug.Log(entry.Key + " : " + entry.Value);
        }
        Debug.Log(characterCameraSettings);*/
        public class DirtyCam : MonoBehaviour
        {
            public CameraRigController cameraRig;
            public float fov = 60f;
            public float pitch = 0f;
            public float yaw = 0f;
            public bool reset = false;

            public void Awake()
            {
                fov = 60f;
                pitch = 0f;
                yaw = 0f;
                reset = false;
                enabled = false;
            }

            public void FixedUpdate()
            {
                if (reset)
                {
                    Awake();
                    return;
                }
                cameraRig.baseFov = fov;
                cameraRig.pitch = pitch;
                cameraRig.yaw = yaw;
            }
        }
        public class CameraTweenController : MonoBehaviour
        {
            public CameraRigController cameraRig;
            readonly float incrementValue = 0.05f;
            float slerpValue = 0f;
            PitchYawPair targetPitchYaw = new PitchYawPair();
            PitchYawPair oldPitchYaw = new PitchYawPair();
            bool DisableToStartNewTween = false;
            public PitchYawPair testing = new PitchYawPair();
            public bool accept = false;

            public void Update()
            {
                if (accept)
                {
                    if (!DisableToStartNewTween)
                    {
                        oldPitchYaw = new PitchYawPair(cameraRig.pitch, cameraRig.yaw);
                        SetPitchYawPair(testing);
                        DisableToStartNewTween = true;
                    }

                    if (slerpValue < 1f)
                    {
                        slerpValue += incrementValue;
                        //var currentPitchYaw = new PitchYawPair(cameraRig.pitch, cameraRig.yaw);
                        var resultingPitchYaw = PitchYawPair.Lerp(oldPitchYaw, targetPitchYaw, slerpValue);
                        cameraRig.SetPitchYaw(resultingPitchYaw);
                    }
                }
            }
            public void SetPitchYawPair(PitchYawPair pitchYawPair)
            {
                slerpValue = 0f;
                DisableToStartNewTween = false;
                targetPitchYaw = pitchYawPair;
            }
        }
    }
}
