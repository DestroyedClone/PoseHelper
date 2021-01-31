using BepInEx;
using R2API.Utils;

namespace PoseHelper
{
    public class MainPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Pose Helper";
        public const string ModGuid = "com.DestroyedClone.PoseHelper";

        internal static BepInEx.Logging.ManualLogSource _logger;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _logger = Logger;
            CommandHelper.AddToConsoleWhenReady();
            Hooks();
        }

        private void Hooks()
        {
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(RoR2.CharacterBody obj)
        {
            if (obj && obj.isPlayerControlled && obj.master)
            {
                if (!obj.masterObject.GetComponent<Commands.DesCloneCommandComponent>())
                    obj.masterObject.AddComponent<Commands.DesCloneCommandComponent>();
            }
        }
    }
}
