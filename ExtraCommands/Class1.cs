using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.VoidRaidCrab;
using EntityStates;
using System.Security;
using System.Security.Permissions;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
using RoR2.Skills;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
////dotnet build --configuration Release
///Severity	Code	Description	Project	File	Line	Suppression State
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ExtraCommands
{
    [BepInPlugin("com.DestroyedClone.ExtraCommands", "ExtraCommands", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    public class Class1 : BaseUnityPlugin
    {
        public static T Load<T>(string assetPath)
        {
            var loadedAsset = Addressables.LoadAssetAsync<T>(assetPath).WaitForCompletion();
            return loadedAsset;
        }

        [ConCommand(commandName = "create_asset", flags = ConVarFlags.None, helpText = "create_asset [filepath] (pos|player) - spawns requested asset at position")]
        public static void CCCreateGameObject(ConCommandArgs args)
        {
            var path = args.GetArgString(0);
            var loadedAsset = Load<GameObject>(path);
            if (loadedAsset == null)
            {
                Debug.LogError("Failed to load asset with given path.");
                return;
            }

            Vector3 position = Vector3.zero;
            if (args.senderBody!= null) { position = args.senderBody.corePosition; }
            if (args.Count > 1)
            {
                position = new Vector3(args.GetArgFloat(1), args.GetArgFloat(2), args.GetArgFloat(3));
            }

            var copy = UnityEngine.Object.Instantiate(loadedAsset);
            copy.transform.SetPositionAndRotation(position, Quaternion.identity);
        }

        [ConCommand(commandName = "create_material", flags = ConVarFlags.None, helpText = "create_material [filepath] - creates an empty gameobject with an attached material")]
        public static void CCCreateMaterial(ConCommandArgs args)
        {
            var path = args.GetArgString(0);
            var loadedAsset = Load<Material>(path);
            if (loadedAsset == null)
            {
                Debug.LogError("Failed to load material asset with given path.");
                return;
            }

            var go = new GameObject();
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.material = loadedAsset;

            int pos = path.LastIndexOf("/") + 1;

            go.name = path.Substring(pos, path.Length - pos);
        }

        [ConCommand(commandName = "enumtryparse", flags = ConVarFlags.None, helpText = "enumtryparse {entry}")]
        public static void CCEnumTryParse(ConCommandArgs args)
        {
            if (Enum.TryParse(args.GetArgString(0), true, out EquipmentIndex equipmentIndex))
            {
                var isValid = EquipmentCatalog.IsIndexValid(equipmentIndex);

                Debug.Log($"{args.GetArgString(0)} is {(isValid ? "valid" : "not valid")} as index {equipmentIndex}");
            }
        }
        
        [ConCommand(commandName = "setskilldef", flags = ConVarFlags.None, helpText = "setskilldef [filepath] [slot]")]
        public static void CCSetSkillDef(ConCommandArgs args)
        {
            var path = args.GetArgString(0);
            var loadedAsset = Load<SkillDef>(path);
            if (loadedAsset == null)
            {
                Debug.LogError("Failed to load asset with given path.");
                return;
            }


        }

    }
}