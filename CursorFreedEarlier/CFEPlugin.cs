using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace CursorFreedEarlier
{
    [BepInPlugin("com.DestroyedClone.CursorFreedEarlier", "Cursor Freed Earlier", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class CFEPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (arg0.name == "loadingbasic")
                ToggleCursor();
        }

        public int ToggleCursor()
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            pes.cursorOpenerCount = pes.cursorOpenerCount > 0 ? 0 : 1;
            return pes.cursorOpenerCount;
        }
    }
}