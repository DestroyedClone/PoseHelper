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
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace OctoberTwenty
{
    [BepInPlugin("com.DestroyedClone.OctoberTwenty", "OctoberTwenty", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency("SceneAssetApi")]
    public class Class1 : BaseUnityPlugin
    {
        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();
            R2API.SceneAssetAPI.AddAssetRequest("moon", GetSceneObjects);
        }

        Action<GameObject[]> GetSceneObjects;


        public void GetMoonObject()
        {

        }

        [ConCommand(commandName = "spawn_prefab", flags = ConVarFlags.ExecuteOnServer, helpText = "path x y z")]
        public static void CMD_SpawnPrefab(ConCommandArgs args)
        {
            var path = args.GetArgString(0);
            var gay = Resources.Load(path);
            var diorama = (GameObject)UnityEngine.Object.Instantiate(gay);
            diorama.transform.position = new Vector3(args.GetArgFloat(1), args.GetArgFloat(2), args.GetArgFloat(3));
        }
    }
}
