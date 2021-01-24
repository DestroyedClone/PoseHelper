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

        private void Awake()
        {
            _logger = Logger;
            CommandHelper.AddToConsoleWhenReady();
        }
    }
}
