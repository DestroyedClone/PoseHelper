using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace ConfigurablePlayerDeathEffect
{
    [BepInPlugin("com.DestroyedClone.CFGPlayerDeathEffect", "Configurable Player Death Effect", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class Class1 : BaseUnityPlugin
    {
        public static GameObject effect = Resources.Load<GameObject>("prefabs/temporaryvisualeffects/PlayerDeathEffect");

        public static ConfigEntry<bool> cfgEnableDeathEffect;

        public void Start()
        {
            cfgEnableDeathEffect = Config.Bind<bool>("", "Enable Death Effect", false, "If true, disables the red post processing from appearing.");
            if (!cfgEnableDeathEffect.Value)
            {
                effect.transform.Find("CameraEffect/PP").gameObject.SetActive(false);
                //effect.transform.Find("CameraEffect/PP").GetComponent<PostProcessDuration>().maxDuration = 0f;
            }
        }
    }
}