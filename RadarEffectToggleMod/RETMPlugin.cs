using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace RadarEffectToggleMod
{
    [BepInPlugin("com.DestroyedClone.RadarEffectToggle", "Radar Effect Toggle", "1.1.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class RETMPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> PostProcessing { get; set; }
        public static ConfigEntry<bool> PointLight { get; set; }
        public static ConfigEntry<bool> Shake { get; set; }
        public static ConfigEntry<bool> Kill { get; set; }


        public void Awake()
        {
            PostProcessing = Config.Bind("Default", "Remove Post Processing", true, "Enable to remove the bright light that fades out on scan." );
            PointLight = Config.Bind("Default", "Remove Point Light", true, "Enable to remove the small light that's emitted at your location.");
            Shake = Config.Bind("Default", "Remove Shaking", true, "Enable to remove the shaking upon scanning. More noticeable when you constantly scan.");
            Kill = Config.Bind("Default", "Disable Effect", true, "Enable to immediately remove the effect entirely. Doesn't affect the actual scan.");


            On.RoR2.RoR2Application.Awake += RoR2Application_Awake;
        }
        private void RoR2Application_Awake(On.RoR2.RoR2Application.orig_Awake orig, RoR2Application self)
        {
            orig(self);
            if (!self.GetComponent<RadarEffectToggleModComponent>())
            {
                var component = self.gameObject.AddComponent<RadarEffectToggleModComponent>();
                component.prefab = Resources.Load<GameObject>("prefabs/effects/ActivateRadarTowerEffect");
                if (PostProcessing.Value || Kill.Value) component.prefab.transform.Find("PP").gameObject.SetActive(false);
                if (PointLight.Value || Kill.Value) component.prefab.transform.Find("Point Light").gameObject.SetActive(false);
                if (Shake.Value || Kill.Value)
                {
                    foreach (var shakers in component.prefab.GetComponents<ShakeEmitter>())
                        shakers.enabled = false;
                }
                if (Kill.Value)
                {
                    component.prefab.GetComponent<DestroyOnTimer>().duration = 0f;
                }
            }
        }
        public class RadarEffectToggleModComponent : MonoBehaviour
        {
            public GameObject prefab;
        }
    }
}
