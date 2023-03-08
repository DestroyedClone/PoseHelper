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
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RoR2.Stats;
using UnityEngine;

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


        [ConCommand(commandName = "arena_next", flags = ConVarFlags.None, helpText = "arena_next [bool:tp to next cell] - Progresses the Void Fields by one cell")]
        public static void CCArenaProgress(ConCommandArgs args)
        {
            if (ArenaMissionController.instance)
            {
                ArenaMissionController.instance.EndRound();
                if (args.Count > 0
                    && args.GetArgBool(0) == true)
                {
                    TeleportHelper.TeleportBody(args.senderBody, ArenaMissionController.instance.nullWards[ArenaMissionController.instance.currentRound].transform.position);
                }
            }
        }

        [ConCommand(commandName = "arena_end", flags = ConVarFlags.None, helpText = "arena_end - Repeatedly progresses the Void Fields by one cell until the event ends")]
        public static void CCArenaRepeatNext(ConCommandArgs _)
        {
            if (ArenaMissionController.instance)
            {
                var amc = ArenaMissionController.instance;
                while (amc.currentRound < amc.totalRoundsMax)
                {
                    amc.ReadyNextNullWard();
                }
            }
        }

        [ConCommand(commandName = "moon_tp", flags = ConVarFlags.None, helpText = "moon_tp - Teleports the player to the moon")]
        public static void CCMoonStart(ConCommandArgs args)
        {
            TeleportHelper.TeleportBody(args.senderBody, new Vector3(165.1803f, 497.2362f, 105.2121f));
        }

        [ConCommand(commandName = "create_items", flags = ConVarFlags.None, helpText = "create_items [itemindices delimited by commas] [offset] - Requires DebugToolkit")]
        public static void CCSpawnItemPickups(ConCommandArgs args)
        {
            if (args.Count == 0)
            {
                Debug.Log("Nothing?");
                return;
            }
            var baseValue = args.GetArgString(0);
            char[] newDelimiter = new char[] { ',' };
            var newValues = baseValue.Split(newDelimiter, StringSplitOptions.None);
            foreach (var index in newValues)
            {
                int.TryParse(index, out int resultingIndex);
                var itemIndex = PickupCatalog.itemIndexToPickupIndex[resultingIndex];
                RoR2.Console.instance.SubmitCmd(args.sender, $"create_pickup {itemIndex} item", false);
                if (args.Count == 2 && args.GetArgBool(1))
                {
                    TeleportHelper.TeleportBody(args.senderBody, args.senderBody.footPosition + Vector3.up * 5f);
                }
            }
        }

        [ConCommand(commandName = "create_equips", flags = ConVarFlags.None, helpText = "create_equips [equipindices delimited by commas] [offset] - Requires DebugToolkit")]
        public static void CCSpawnEquipPickups(ConCommandArgs args)
        {
            if (args.Count == 0)
            {
                Debug.Log("Nothing?");
                return;
            }
            var baseValue = args.GetArgString(0);
            char[] newDelimiter = new char[] { ',' };
            var newValues = baseValue.Split(newDelimiter, StringSplitOptions.None);
            foreach (var index in newValues)
            {
                int.TryParse(index, out int resultingIndex);
                var itemIndex = PickupCatalog.equipmentIndexToPickupIndex[resultingIndex];
                RoR2.Console.instance.SubmitCmd(args.sender, $"create_pickup {itemIndex} equip", false);
                if (args.Count == 2 && args.GetArgBool(1))
                {
                    TeleportHelper.TeleportBody(args.senderBody, args.senderBody.footPosition + Vector3.up * 5f);
                }
            }
        }

        public static bool toggleKillOnStart = false;
        [ConCommand(commandName = "enemies_instadie", flags = ConVarFlags.None, helpText = "enemies_instadie")]
        public static void CCMoonSpeedrun(ConCommandArgs args)
        {
            toggleKillOnStart = !toggleKillOnStart;
            if (toggleKillOnStart)
            {
                CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            } else
            {
                CharacterBody.onBodyStartGlobal -= CharacterBody_onBodyStartGlobal;
            }
            Debug.Log($"enemies_instadie set to {toggleKillOnStart}");
        }

        private static void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.teamComponent.teamIndex != TeamIndex.Player)
            {
                obj.healthComponent.Suicide();
            }
        }

        [ConCommand(commandName = "holdout_finish", flags = ConVarFlags.None, helpText = "holdout_finish [opt:all]")]
        public static void CCHoldoutFinish(ConCommandArgs args)
        {
            var zones = UnityEngine.Object.FindObjectsOfType<HoldoutZoneController>();
            if (args.Count > 0 && args.GetArgString(0).ToLower() == "all")
            {
                int count = 0;
                foreach (var zone in zones)
                {
                    if (zone != null)
                    {
                        zone.FullyChargeHoldoutZone();
                        count++;
                    }
                }
                Debug.Log($"Charged {count} holdout zones.");
                return;
            }


            foreach (var zone in zones)
            {
                if (zone != null
                    && zone.IsBodyInChargingRadius(args.senderBody))
                {
                    zone.FullyChargeHoldoutZone();
                    Debug.Log("Charged current zone.");
                    return;
                }
            }
            Debug.LogWarning("Couldn't find a holdout zone that holds you.");
        }
    }
}