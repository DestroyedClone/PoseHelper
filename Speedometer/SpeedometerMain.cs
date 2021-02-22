using BepInEx;
using R2API.Utils;
using UnityEngine;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace Speedometer
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class SpeedometerMain : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Speedometer";
        public const string ModGuid = "com.DestroyedClone.Speedometer";

        private void Awake()
        {
            On.RoR2.UI.HUD.Awake += ShowUnusedHUDElements;
        }
        private void ShowUnusedHUDElements(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            var mainUIArea = GameObject.Find("HUDSimple(Clone)").transform.Find("MainContainer").transform.Find("MainUIArea").transform;
            var speedometer = mainUIArea.Find("UpperRightCluster").transform.Find("TimerRoot").transform.Find("SpeedometerPanel").gameObject;
            speedometer.transform.parent = speedometer.transform.parent.transform.Find("RightInfoBar").transform;
            speedometer.SetActive(true);

            //mainUIArea.Find("UpperLeftCluster").transform.Find("InputStickVisualizer").gameObject.SetActive(true);
            mainUIArea.Find("ScoreboardPanel").transform.Find("PP").gameObject.SetActive(false);
        }
    }
}
