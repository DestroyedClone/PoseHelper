using BepInEx;
using R2API.Utils;
using R2API;
using UnityEngine.Networking;
using RoR2;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using Path = System.IO.Path;
using R2API.Networking;
using UnityEngine.Playables;
using System;
using static UnityEngine.ScriptableObject;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ExtinguishInWater
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class ExtinguishMain : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "ExtinguishingWater";
        public const string ModGuid = "com.DestroyedClone.ExtinguishingWater";

        public static SurfaceDef waterSD = Resources.Load<SurfaceDef>("surfacedefs/sdWater");
        public static ConfigEntry<bool> AllowPlayers { get; set; }
        public static ConfigEntry<bool> AllowAllies { get; set; }
        public static ConfigEntry<bool> AllowEnemies { get; set; }
        public static ConfigEntry<bool> PreventUnderwaterIgnition { get; set; }
        public static ConfigEntry<bool> Commands { get; set; }

        public void Awake()
        {
            AllowPlayers = Config.Bind("Filter", "Allow Players", true, "Allow players, regardless of team, to get extinguished.");
            AllowAllies = Config.Bind("Filter", "Allow Allies", true, "Allow allies, excluding players, to get extinguished.");
            AllowEnemies = Config.Bind("Filter", "Allow Enemies", true, "Allow enemies to get extinguished.");
            Commands = Config.Bind("Other", "Enable Burn Commands", true, "Enable commands to set yourself on fire. More for debugging than anything.");
            PreventUnderwaterIgnition = Config.Bind("Filter", "Prevent Underwater Ignition", true, "Prevent attacks from igniting if the victim is submerged.");

            if (Commands.Value)
                R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            if (AllowPlayers.Value || AllowAllies.Value || AllowEnemies.Value)
            {
                On.RoR2.GlobalEventManager.OnCharacterHitGround += ExtinguishInWaterJump;
                On.RoR2.FootstepHandler.Footstep_string_GameObject += ExtinguishFootstep;
                On.RoR2.DotController.InflictDot += ExtinguishInflict;
            }


        }

        private void ExtinguishInflict(On.RoR2.DotController.orig_InflictDot orig, GameObject victimObject, GameObject attackerObject, DotController.DotIndex dotIndex, float duration, float damageMultiplier)
        {
            if (victimObject)
                if (CheckForWater(victimObject.transform.position, false) || CheckForWater(victimObject.transform.position, true))
                {
                    if (dotIndex == DotController.DotIndex.PercentBurn || dotIndex == DotController.DotIndex.Burn)
                    {
                        Chat.AddMessage("prevented underwater ignition");
                        duration = 0f;
                    }
                }
                else
                {
                    var component = victimObject.AddComponent<Extinguisher>();
                    component.characterBody = victimObject.GetComponent<CharacterBody>();
                }
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier);
        }

        private void ExtinguishFootstep(On.RoR2.FootstepHandler.orig_Footstep_string_GameObject orig, FootstepHandler self, string childName, GameObject footstepEffect)
        {
            orig(self, childName, footstepEffect);
            var charBody = self.gameObject.GetComponent<CharacterBody>();
            if (charBody && CheckForWater(transform.position)) Extinguish(charBody);
        }
        private void ExtinguishInWaterJump(On.RoR2.GlobalEventManager.orig_OnCharacterHitGround orig, GlobalEventManager self, CharacterBody characterBody, Vector3 impactVelocity)
        {
            orig(self, characterBody, impactVelocity);
            if (characterBody)
            {
                CharacterMotor characterMotor = characterBody.characterMotor;
                if (characterMotor && Run.FixedTimeStamp.now - characterMotor.lastGroundedTime > 0.2f)
                {
                    if (CheckForWater(characterBody.footPosition)) Extinguish(characterBody);
                }
            }
        }

        public static Vector3 GetHeadPosition(CharacterBody characterBody)
        {
            var dist = Vector3.Distance(characterBody.corePosition, characterBody.footPosition);
            return characterBody.corePosition + Vector3.up * dist;
        }

        private bool CheckForWater(Vector3 position, bool below = true)
        {
            if (Physics.Raycast(new Ray(position + Vector3.up * 1.5f, below ? Vector3.down : Vector3.up), out RaycastHit raycastHit, below ? 4f : 8f, LayerIndex.world.mask | LayerIndex.water.mask, QueryTriggerInteraction.Collide))
            {
                SurfaceDef objectSurfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(raycastHit.collider, raycastHit.point);
                if (objectSurfaceDef)
                {
                    if (objectSurfaceDef == waterSD)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool AllowedToExtinguish(CharacterBody characterBody)
        {
            bool allowExtinguish = false;
            if (characterBody.isPlayerControlled)
                allowExtinguish = AllowPlayers.Value;
            else
                if (characterBody.teamComponent)
                if (characterBody.teamComponent.teamIndex == TeamIndex.Player)
                    allowExtinguish = AllowAllies.Value;
                else
                    allowExtinguish = AllowEnemies.Value;
            return allowExtinguish;
        }

        public static void Extinguish(CharacterBody characterBody)
        {
            if (!AllowedToExtinguish(characterBody)) return;

            if (characterBody.HasBuff(BuffIndex.OnFire))
            {
                characterBody.ClearTimedBuffs(BuffIndex.OnFire);

                if (DotController.dotControllerLocator.TryGetValue(characterBody.gameObject.GetInstanceID(), out DotController dotController))
                {
                    //var burnEffectController = dotController.burnEffectController;
                    var dotStacks = dotController.dotStackList;

                    int i = 0;
                    int count = dotStacks.Count;
                    while (i < count)
                    {
                        if (dotStacks[i].dotIndex == DotController.DotIndex.Burn
                            || dotStacks[i].dotIndex == DotController.DotIndex.Helfire
                            || dotStacks[i].dotIndex == DotController.DotIndex.PercentBurn)
                        {
                            dotStacks[i].damage = 0f;
                            dotStacks[i].timer = 0f;
                        }
                        i++;
                    }
                }
            }
        }

        [ConCommand(commandName = "burn_self", flags = ConVarFlags.ExecuteOnServer, 
            helpText = "burn_self {stacks} {duration}")]
        public static void MyCommandName(ConCommandArgs args)
        {
            DotController.DotIndex index = (DotController.DotIndex)Array.FindIndex(DotController.dotDefs, (dotDef) => dotDef.associatedBuff == BuffIndex.OnFire);
            for (int y = 0; y < args.GetArgInt(0); y++)
            {
                DotController.InflictDot(args.senderBody.gameObject, args.senderBody.gameObject, index, args.GetArgInt(1), 0.25f);
                args.senderBody.AddTimedBuffAuthority(BuffIndex.OnFire, args.GetArgInt(1));
            }
        }

        public class Extinguisher : MonoBehaviour
        {
            public CharacterBody characterBody;

            public void FixedUpdate()
            {
                if (CheckForWater(characterBody.corePosition))
                {
                    Debug.Log("Extinguished!");
                    ExtinguishMain.Extinguish(characterBody);
                    Destroy(this);
                }
            }

            private bool CheckForWater(Vector3 position)
            {
                var halfBodyHeight = Vector3.Distance(characterBody.corePosition, characterBody.footPosition);
                var castDistance = halfBodyHeight * 1.5f;
                var layerMask = LayerIndex.world.mask | LayerIndex.water.mask;
                if (Physics.Raycast(new Ray(position + Vector3.up * halfBodyHeight, Vector3.down), out RaycastHit raycastHit, castDistance, layerMask, QueryTriggerInteraction.Collide)
                    || Physics.Raycast(new Ray(position + Vector3.down * halfBodyHeight, Vector3.up), out raycastHit, castDistance, layerMask, QueryTriggerInteraction.Collide))
                {
                    SurfaceDef objSurfDefDown = SurfaceDefProvider.GetObjectSurfaceDef(raycastHit.collider, raycastHit.point);
                    if (objSurfDefDown && objSurfDefDown == waterSD)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
