﻿using BepInEx;
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EntityStates;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using RoR2.Projectile;
using static UnityEngine.Animator;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace GoombaStomp
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(ItemAPI), nameof(DifficultyAPI), nameof(BuffAPI))]
    public class StompMain : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "GoomaStompArtifact";
        public const string ModGuid = "com.DestroyedClone.GoomaStompArtifact";

        void Awake()
        {
            Artifacts.RegisterArtifacts();
        }
    }
    public static class Artifacts
    {
        public static ArtifactDef GoombaArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
        private static readonly float maxDistance = 10f;
        private static readonly float minFallSpeed = 30f;
        private static readonly float bounceForce = 2000f;
        public static GameObject goombaGameObject = new GameObject();
        public static float goombaDamage = 1000f;
        private static readonly string goombaDeathToken = "You have been Goomba Stomped!";
        private static readonly string goombaDeathMultiplayerToken = "{0} has been Goomba Stomped!";

        public static void RegisterArtifacts()
        {
            GoombaArtifactDef.nameToken = "Artifact of Goombastomping";
            GoombaArtifactDef.descriptionToken = "Deal substantial damage upon landing on an enemy's head.";
            GoombaArtifactDef.smallIconDeselectedSprite = LoadoutAPI.CreateSkinIcon(Color.white, Color.white, Color.white, Color.white);
            GoombaArtifactDef.smallIconSelectedSprite = LoadoutAPI.CreateSkinIcon(Color.gray, Color.white, Color.white, Color.white);

            goombaGameObject.name = "GoombaStomp";

            LanguageAPI.Add("PLAYER_DEATH_QUOTE_GOOMBADEATH", goombaDeathToken);
            LanguageAPI.Add("PLAYER_DEATH_QUOTE_GOOMBADEATH_2P", goombaDeathMultiplayerToken);

            ArtifactCatalog.getAdditionalEntries += (list) =>
            {
                list.Add(GoombaArtifactDef);
            };

            On.RoR2.CharacterMotor.OnHitGround += CharacterMotor_OnHitGround;
        }


        private static void CharacterMotor_OnHitGround(On.RoR2.CharacterMotor.orig_OnHitGround orig, CharacterMotor self, CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            bool hasGoombad = false;
            bool restoreFallDamage = false;
            if (RunArtifactManager.instance.IsArtifactEnabled(GoombaArtifactDef.artifactIndex))
            {
                if (self.body)
                {
                    if (Math.Abs(hitGroundInfo.velocity.y) >= minFallSpeed)
                    {
                        Chat.AddMessage("Speed: " + Math.Abs(hitGroundInfo.velocity.y) + "/" + minFallSpeed);
                        var bodySearch = new BullseyeSearch() //let's just get the nearest enemy
                        {
                            viewer = self.body,
                            sortMode = BullseyeSearch.SortMode.Distance,
                            teamMaskFilter = TeamMask.GetEnemyTeams(self.body.teamComponent.teamIndex),
                        };
                        bodySearch.RefreshCandidates();
                        Debug.Log("Nearest Enemies: " + bodySearch.GetResults().ToList());

                        var nearestBody = bodySearch.GetResults().ToList();

                        // We very likely landed on an enemy.
                        if (nearestBody.Count > 0)
                        {
                            Chat.AddMessage("hit count high enough");
                            var firstBody = nearestBody.FirstOrDefault();
                            var distance = Vector3.Distance(hitGroundInfo.position, Helpers.GetHeadPosition(firstBody.healthComponent.body));
                            if (distance <= maxDistance)
                            {
                                firstBody.healthComponent.TakeDamage(new DamageInfo()
                                {
                                    attacker = self.body.gameObject,
                                    damage = goombaDamage,
                                    inflictor = goombaGameObject
                                });
                                if ((self.body.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == CharacterBody.BodyFlags.None)
                                {
                                    self.body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                                    restoreFallDamage = true;
                                }
                                Chat.AddMessage("Goomba!");
                                hasGoombad = true;
                            }
                        }
                    }
                }
            }
            orig(self, hitGroundInfo);
            if (hasGoombad)
            {
                self.Motor.ForceUnground();
                self.ApplyForce(Vector3.up * bounceForce);
            }
            if (restoreFallDamage)
            {
                self.body.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            }
        }
    }
    public static class Helpers
    {
        public static Vector3 GetHeadPosition(CharacterBody characterBody)
        {
            var dist = Vector3.Distance(characterBody.corePosition, characterBody.footPosition);
            return characterBody.corePosition + Vector3.up * dist;
        }
    }
}
