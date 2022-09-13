using System;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using RoR2.CharacterAI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace EnemyTelegraphing
{
    [BepInPlugin("com.DestroyedClone.EnemyTelegraphing", "Enemy Telegraphing", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class Main : BaseUnityPlugin
    {
        public static GameObject magmaWormPrefab = Resources.Load<GameObject>("prefabs/characterbodies/magmawormbody");
        public static GameObject elecWormPrefab = Resources.Load<GameObject>("prefabs/characterbodies/electricwormbody");
        public static GameObject wardPrefab;

        public static GameObject wispPrefab = Resources.Load<GameObject>("prefabs/characterbodies/wispbody");

        public void Start()
        {
            SetupWorms();
            SetupWisp();
            On.RoR2.BodyCatalog.Init += BodyCatalog_Init;
        }

        private void BodyCatalog_Init(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();
            foreach (var body in BodyCatalog.allBodyPrefabs)
            {
                body.AddComponent<LaserVision>();
            }
        }

        public void SetupWisp()
        {
            //var comp = wispPrefab.AddComponent<LaserVision>();
        }

        public class LaserVision : MonoBehaviour
        {
            public static float laserMaxWidth = 0.2f;
            public static GameObject laserPrefab => EntityStates.GolemMonster.ChargeLaser.laserPrefab;
            private GameObject laserEffect;
            private LineRenderer laserLineComponent;
            private bool laserOn = false;

            public InputBankTest inputBank;
            public BaseAI baseAI;
            public bool forceOn = false;

            public void Start()
            {
                inputBank = GetComponent<InputBankTest>();
                baseAI = inputBank.characterBody.master.GetComponent<BaseAI>();
                forceOn = !baseAI;

                Transform modelTransform = inputBank.characterBody.modelLocator.modelTransform;
                if (modelTransform)
                {
                    ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                    if (component)
                    {
                        Transform transform = component.FindChild("Muzzle");
                        if (transform)
                        {
                            if (laserPrefab)
                            {
                                laserEffect = UnityEngine.Object.Instantiate<GameObject>(laserPrefab, transform.position, transform.rotation);
                                laserEffect.transform.parent = transform;
                                laserLineComponent = laserEffect.GetComponent<LineRenderer>();
                            }
                        }
                    }
                }
            }

            public void FixedUpdate()
            {
                laserOn = forceOn || (baseAI && baseAI.hasAimConfirmation);
            }

            public void Update()
            {
                if (laserEffect && laserLineComponent)
                {
                    float maxDistance = 1000f;
                    Ray aimRay = inputBank.GetAimRay();
                    Vector3 position = laserEffect.transform.parent.position;
                    Vector3 point = aimRay.GetPoint(maxDistance);
                    if (Physics.Raycast(aimRay, out RaycastHit raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                    {
                        point = raycastHit.point;
                    }
                    laserLineComponent.SetPosition(0, position);
                    laserLineComponent.SetPosition(1, point);
                    var multiplier = laserOn ? 1 : 0;
                    laserLineComponent.startWidth = laserMaxWidth * multiplier;
                    laserLineComponent.endWidth = laserMaxWidth * multiplier;
                }
            }

            public void OnDestroy()
            {
                if (laserEffect)
                {
                    EntityStates.EntityState.Destroy(laserEffect);
                }
            }
        }

        public void SetupWorms()
        {
            wardPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/networkedobjects/mushroomward"), "WormTelegraphIndicator");
            Destroy(wardPrefab.GetComponent<TeamFilter>());
            Destroy(wardPrefab.GetComponent<HealingWard>());

            EditWorm(magmaWormPrefab);
            EditWorm(elecWormPrefab);
        }

        public void EditWorm(GameObject wormPrefab)
        {
            var com = wormPrefab.AddComponent<PositionRevealer>();
            com.wormBodyPositions = wormPrefab.GetComponent<WormBodyPositions2>();
            com.headBone = com.wormBodyPositions.bones.First();
        }

        public class PositionRevealer : MonoBehaviour
        {
            public WormBodyPositions2 wormBodyPositions;
            public Transform headBone;
            [Tooltip("The child range indicator object. Will be scaled to the radius.")]
            public Transform rangeIndicator;

            public void Start()
            {
                rangeIndicator = UnityEngine.Object.Instantiate(wardPrefab, transform).transform;
            }

            public void OnDestroy()
            {
                Destroy(rangeIndicator);
            }

            public void FixedUpdate()
            {
                if (Physics.Raycast(headBone.position, Vector3.down, out RaycastHit raycastHit, 10000f, LayerIndex.world.mask))
                {
                    rangeIndicator.transform.position = raycastHit.point;
                    rangeIndicator.transform.up = raycastHit.normal;
                }
            }
        }
    }
}
