using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

using System.Collections.ObjectModel;
using System.Globalization;
using RoR2.ConVar;
using RoR2.Networking;
using Unity;
using UnityEngine.Networking;
using RoR2.UI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace TreesIgnoreLOD
{
    [BepInPlugin("com.DestroyedClone.TreesIgnoreLOD", "Trees Ignore LOD", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class TreesIgnoreLODPlugin : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            var lodGroups = UnityEngine.Object.FindObjectsOfType<LODGroup>();
            foreach (var item in lodGroups)
            {
                if (item.fadeMode == LODFadeMode.SpeedTree && item.GetComponent<Rigidbody>())
                {
                    item.ForceLOD(1);
                }
            }
        }
    }
}
