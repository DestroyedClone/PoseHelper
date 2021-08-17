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
    [BepInPlugin("com.DestroyedClone.CursorFreedEarlier", "Cursor Freed Earlier", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class CFEPlugin : BaseUnityPlugin
    {
        public static bool hasClosedStartingCursor = false;

        public void Awake()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            /*if (!hasClosedStartingCursor && (arg0.name == "loadingbasic" || arg0.name == "title"))
            {
                //ToggleCursorAlt();
            }*/

            if (!hasClosedStartingCursor)
            {
                ToggleCursor(arg0.name);
            }
        }

        /*public void ToggleCursorAlt() //still works but feels less efficient.
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            Debug.LogError("Cursors open: " + pes.cursorOpenerCount);
            switch (pes.cursorOpenerCount)
            {
                case 0:
                    hasClosedStartingCursor = false;
                    pes.cursorOpenerCount = 1;
                    break;

                case 2:
                    pes.cursorOpenerCount = 1;
                    hasClosedStartingCursor = true;
                    Debug.Log("Cursor closed");
                    break;
            }
            Debug.LogError("Cursors open after: " + pes.cursorOpenerCount);
        }*/

        public void ToggleCursor(string sceneName)
        {
            var pes = MPEventSystemManager.primaryEventSystem;
            //Debug.LogError("Cursors open: " + pes.cursorOpenerCount);

            switch (sceneName)
            {
                case "loadingbasic":
                    hasClosedStartingCursor = false;
                    //Debug.Log("Cursor Opened");
                    pes.cursorOpenerCount += 1;
                    break;

                case "title":
                    hasClosedStartingCursor = true;
                    //Debug.Log("Cursor Closed");
                    pes.cursorOpenerCount -= 1;
                    break;
            }
            //Debug.LogError("Cursors open after: " + pes.cursorOpenerCount);
        }
    }
}