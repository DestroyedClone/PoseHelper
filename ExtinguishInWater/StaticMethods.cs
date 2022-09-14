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
using static ExtinguishInWater.ExtinguishMain;

namespace ExtinguishInWater
{
    public static class StaticMethods
    {

        public static bool CheckForWater(Vector3 position, float castDistance = 8f)
        {
            var layerMask = LayerIndex.world.mask | LayerIndex.water.mask;
            if (Physics.Raycast(new Ray(position + Vector3.up * castDistance, Vector3.down), out RaycastHit raycastHit, castDistance, layerMask, QueryTriggerInteraction.Collide)
                || Physics.Raycast(new Ray(position + Vector3.down * castDistance, Vector3.up), out raycastHit, castDistance, layerMask, QueryTriggerInteraction.Collide))
            {
                SurfaceDef objSurfDefDown = SurfaceDefProvider.GetObjectSurfaceDef(raycastHit.collider, raycastHit.point);
                if (objSurfDefDown && objSurfDefDown == waterSD)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AllowedToExtinguish(CharacterBody characterBody)
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

            if (characterBody.HasBuff(RoR2Content.Buffs.OnFire))
            {
                characterBody.ClearTimedBuffs(RoR2Content.Buffs.OnFire);

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

                    //Debug.Log("Extinguished!");
                }
            }
        }
    }
}