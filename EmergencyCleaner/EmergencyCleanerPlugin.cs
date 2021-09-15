using System;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using RoR2.Skills;
using EntityStates;
using System.Collections;

namespace EmergencyCleaner
{
    [BepInPlugin("com.DestroyedClone.EmergencyCleaner", "EmergencyCleaner", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class EmergencyCleanerPlugin : BaseUnityPlugin
    {
        public static bool frameRateHazard = false;

        public void Awake()
        {
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.EffectComponent.Start += EffectComponent_Start;
        }

        private void EffectComponent_Start(On.RoR2.EffectComponent.orig_Start orig, EffectComponent self)
        {
            orig(self);
            if (frameRateHazard)
            {
                DestroyImmediate(self.gameObject);
            }
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            var cock = new GameObject();
            cock.name = "dicks";
            cock.AddComponent<Cleaner>();
        }

        public class Cleaner : MonoBehaviour
        {
            int frameRate;
            readonly int minFrameRate;
            public void Update()
            {
                frameRate = (int)(1.0 / Time.deltaTime);
            }


            public IEnumerator Konodioda()
            {
                frameRateHazard = true;
                while (frameRate < minFrameRate)
                {
                    yield return new WaitForSecondsRealtime(5f);
                }
                frameRateHazard = false;

            }
        }

        public class Cleaner2 : MonoBehaviour
        {
            public uint EmergencyFramerate = 9U;
            public int DurationBeforeEmergency = 5;
            private int SecondsUnderThreshold = 0;
            public int frameRate = 60;
            bool isPaused = false;

            public void Update()
            {
                frameRate = (int)(1.0 / Time.deltaTime);
            }

            public void FixedUpdate()
            {
                if (isPaused)
                {
                    return;
                }

                if (SecondsUnderThreshold < DurationBeforeEmergency)
                {
                    if (frameRate <= EmergencyFramerate)
                    {
                        Chat.AddMessage($"Cleaner::FixedUpdate: Warning, framerate has fallen below {EmergencyFramerate}! {DurationBeforeEmergency - SecondsUnderThreshold} more seconds before clearing!");
                        SecondsUnderThreshold++;
                    } else
                    {
                        SecondsUnderThreshold = 0;
                    }
                } else
                {
                    StartCoroutine(nameof(Konodioda));
                }
            }

            public IEnumerator Konodioda()
            {
                isPaused = true;
                Chat.AddMessage($"Cleaner::FixedUpdate: Entering Fuck Mode");
                ClearEffects();
                RoR2.Console.instance.SubmitCmd(null, "time_scale 0.001");
                yield return new WaitForSecondsRealtime(5f);
                isPaused = false;
                SecondsUnderThreshold = 0;
                Chat.AddMessage($"Cleaner::FixedUpdate: Exiting Fuck Mode");
                RoR2.Console.instance.SubmitCmd(null, "time_scale 1");

            }

            public void ClearEffects()
            {
                foreach (var effect in UnityEngine.Object.FindObjectsOfType<EffectComponent>())
                {
                    DestroyImmediate(effect.gameObject);
                }
            }
        }
    }
}
