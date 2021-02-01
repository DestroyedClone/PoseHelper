using BepInEx;
using R2API.Utils;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Networking;
using UnityEngine.Playables;

namespace MoonToOutro
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(
       nameof(ResourcesAPI),
       nameof(PlayerAPI),
       nameof(PrefabAPI),
       nameof(NetworkingAPI)
       )]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Immediate Moon To Outro";
        public const string ModGuid = "com.DestroyedClone.MoonToOutro";

        private void Awake()
        {
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "moon":
                    //var EscapeSequenceController = GameObject.Find("EscapeSequenceController");
                    //var EscapeSequenceObjects = EscapeSequenceController.transform.Find("EscapeSequenceObjects");
                    //EscapeSequenceObjects.gameObject.SetActive(true);
                    //GameObject.Find("Ending Trigger").transform.position = new Vector3(2654, 206, 723);
                    UnityEngine.Object.FindObjectOfType<EscapeSequenceController>().CompleteEscapeSequence();
                    break;
                case "outro":
                    GameObject.Find("CutsceneController").GetComponent<PlayableDirector>().initialTime = 40f;
                    break;
            }
        }
    }
}
