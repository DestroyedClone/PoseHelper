using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using EntityStates;
using System;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace IndividualStateMachines
{
    [BepInPlugin("com.DestroyedClone.StateMachineMod", "StateMachineMod", "1.0.2")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource _logger;

        public static string primaryName = "PrimarySkill";
        public static string secondaryName = "SecondarySkill";
        public static string utilityName = "UtilitySkill";
        public static string specialName = "SpecialSkill";
        //[RoR2.SystemInitializer(dependencies: typeof(RoR2.EntityStateCatalog))]

        public static string[] bannedBodyNames = new string[]
        {
            "AncientWispBody",
            "ArchWispBody",
            "ArtifactShellBody"
        };
        public void Awake()
        {
            _logger = Logger;
        }

        [RoR2.SystemInitializer(dependencies: new Type[]{ 
            typeof(RoR2.EntityStateCatalog),
            typeof(RoR2.BodyCatalog)
        
        })]
        public static void Modify()
        {
            foreach (var body in BodyCatalog.allBodyPrefabs.ToList())
            {
                if (!body)
                    continue;
                if (bannedBodyNames.Contains(body.name))
                {
                    _logger.LogWarning($"Skipping {body.name}!");
                    continue;
                }

                _logger.LogMessage($"Working on {body.name}");
                var skillLocator = body.GetComponent<SkillLocator>();
                if (!skillLocator)
                {
                    _logger.LogError("No SkillLocator, continuing.");
                    continue;
                }

                var esmList = body.GetComponents<EntityStateMachine>().ToList();
                if (esmList.Count <= 0)
                {
                    _logger.LogWarning("No Entity State Machines, continuing.");
                    continue;
                }

                foreach (var entityStateMachines in body.GetComponents<EntityStateMachine>().ToList())
                {
                    if (entityStateMachines.customName == "Body") continue;
                    Destroy(entityStateMachines);
                }
                var primaryState = CreateEntityStateMachine(body, primaryName);
                var secondaryState = CreateEntityStateMachine(body, secondaryName);
                var utilityState = CreateEntityStateMachine(body, utilityName);
                var specialState = CreateEntityStateMachine(body, specialName);

                var networkStateMachine = body.GetComponent<NetworkStateMachine>();
                if (networkStateMachine)
                {
                    networkStateMachine.stateMachines = new EntityStateMachine[]
                    {
                        primaryState,
                        secondaryState,
                        utilityState,
                        specialState
                    };
                }

                if (skillLocator.primary)
                {
                    //skillLocator.primary.skillDef.activationStateMachineName = primaryState.customName;
                    foreach (var variant in skillLocator.primary.skillFamily.variants)
                    {
                        _logger.LogMessage($"Modifying PRIMARY skillDef {variant.skillDef.skillNameToken}");
                        variant.skillDef.activationStateMachineName = primaryState.customName;
                    }
                }
                if (skillLocator.secondary)
                {
                    //skillLocator.secondary.skillDef.activationStateMachineName = primaryState.customName;
                    foreach (var variant in skillLocator.secondary.skillFamily.variants)
                    {
                        _logger.LogMessage($"Modifying SECONDARY skillDef {variant.skillDef.skillNameToken}");
                        variant.skillDef.activationStateMachineName = primaryState.customName;
                    }
                }
                if (skillLocator.utility)
                {
                    //skillLocator.utility.skillDef.activationStateMachineName = primaryState.customName;
                    foreach (var variant in skillLocator.utility.skillFamily.variants)
                    {
                        _logger.LogMessage($"Modifying UTILITY skillDef {variant.skillDef.skillNameToken}");
                        variant.skillDef.activationStateMachineName = primaryState.customName;
                    }
                }
                if (skillLocator.special)
                {
                    //skillLocator.special.skillDef.activationStateMachineName = primaryState.customName;
                    foreach (var variant in skillLocator.special.skillFamily.variants)
                    {
                        _logger.LogMessage($"Modifying SPECIAL skillDef {variant.skillDef.skillNameToken}");
                        variant.skillDef.activationStateMachineName = primaryState.customName;
                    }
                }
            }
        }

        public static EntityStateMachine CreateEntityStateMachine(GameObject body, string name)
        {
            var stateMachine = body.AddComponent<EntityStateMachine>();
            stateMachine.customName = name;
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            return stateMachine;
        }
    }
}
