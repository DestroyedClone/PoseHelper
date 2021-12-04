using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace SunDimmer
{
    [BepInPlugin("com.DestroyedClone.SunDimmer", "Sun Dimmer", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class SunDimmerPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> cfgLightIntensity;

        public static GameObject sunPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/GrandParentSun");

        public void Awake()
        {
            cfgLightIntensity = Config.Bind("", "Light Intensity", 75f, "The intensity of the light cast from the Sun." +
                "\nThe default game value is about 1000.");

            ModifyPrefab();
        }

        public void ModifyPrefab()
        {
            var pointLight = sunPrefab.transform.Find("VfxRoot/LightSpinner/LightSpinner/Point Light");
            //pointLight.GetComponent<FlickerLight>().enabled = false;
            var light = pointLight.GetComponent<Light>();
            light.intensity = cfgLightIntensity.Value;
        }
    }
}