using BepInEx;
using RoR2;

namespace MoonToOutro
{
    [BepInPlugin("com.DestroyedClone.MoonToOutro", "Immediate Moon To Outro", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "moon")
                UnityEngine.Object.FindObjectOfType<EscapeSequenceController>().CompleteEscapeSequence();
        }
    }
}
