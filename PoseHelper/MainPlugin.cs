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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering;
using RoR2.CharacterAI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace PoseHelper
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(
       nameof(ItemAPI),
       nameof(BuffAPI),
       nameof(LanguageAPI),
       nameof(LoadoutAPI),
       nameof(ResourcesAPI),
       nameof(PlayerAPI),
       nameof(PrefabAPI),
       nameof(SoundAPI),
       nameof(OrbAPI),
       nameof(NetworkingAPI),
       nameof(EffectAPI),
       nameof(EliteAPI),
       nameof(LoadoutAPI),
       nameof(SurvivorAPI)
       )]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Pose Helper";
        public const string ModGuid = "com.DestroyedClone.PoseHelper";

        internal static BepInEx.Logging.ManualLogSource _logger;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _logger = Logger;
            CommandHelper.AddToConsoleWhenReady();
            Hooks();
            FreeTheLockedMage();

            //On.RoR2.BodyCatalog.Init += BodyCatalog_Init;

        }

        private void BodyCatalog_Init(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();
        }

        private void Hooks()
        {
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            //On.RoR2.SceneDirector.Start += SceneDirector_Start;
            EntityStates.LockedMage.UnlockingMage.onOpened += UnlockingMage_onOpened;
        }

        private void UnlockingMage_onOpened(Interactor obj)
        {
            GameObject mageMasterPrefab = MasterCatalog.FindMasterPrefab("MageMonsterMaster");
            //GameObject mageBodyPrefab = mageMasterPrefab.GetComponent<CharacterMaster>().bodyPrefab;

            GameObject mageBodyGameObject = UnityEngine.Object.Instantiate(mageMasterPrefab, gameObject.transform.position, Quaternion.identity);
            CharacterMaster mageCharacterMaster = mageBodyGameObject.GetComponent<CharacterMaster>();
            AIOwnership mageAIOwnership = mageBodyGameObject.GetComponent<AIOwnership>();

            CharacterMaster playerMaster = obj.gameObject.GetComponent<CharacterBody>().master;
            BaseAI mageBaseAI = gameObject.GetComponent<BaseAI>();
            if (mageCharacterMaster)
            {
                mageCharacterMaster.inventory.GiveItem(ItemIndex.BoostDamage, 10);
                mageCharacterMaster.inventory.GiveItem(ItemIndex.BoostHp, 10);
                GameObject bodyObject = playerMaster.GetBodyObject();
                if (bodyObject)
                {
                    Deployable component4 = mageBodyGameObject.GetComponent<Deployable>();
                    if (!component4) component4 = mageBodyGameObject.AddComponent<Deployable>();
                    playerMaster.AddDeployable(component4, DeployableSlot.ParentAlly);
                }
            }
            if (mageAIOwnership)
            {
                mageAIOwnership.ownerMaster = obj.gameObject.GetComponent<CharacterBody>().master;
            }
            if (mageBaseAI)
            {
                mageBaseAI.leader.gameObject = base.gameObject;
            }

            NetworkServer.Spawn(mageBodyGameObject);
            mageCharacterMaster.SpawnBody(mageBodyGameObject, gameObject.transform.position, Quaternion.identity);
        }

        private void FreeTheLockedMage()
        {
            GameObject magePrefab = Resources.Load<GameObject>("prefabs/networkedobjects/LockedMage");
            magePrefab.GetComponent<GameObjectUnlockableFilter>().forbiddenUnlockable = null;
            //magePrefab.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(ReleasingMage));
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "lobby":
                    //GameObject.Find("Directional Light").GetComponent<Light>().color = Color.white;
                    var localMaster = PlayerCharacterMasterController.instances[0].master;
                    if (localMaster)
                    {
                        localMaster.GetBody()?.characterMotor.Motor.SetPositionAndRotation(new Vector3(0.12f, 0.91f, 7.76f), Quaternion.identity, true);
                    }
                    break;
            }
        }

        private void CharacterBody_onBodyStartGlobal(RoR2.CharacterBody obj)
        {
            if (obj && obj.isPlayerControlled && obj.master)
            {
                if (!obj.masterObject.GetComponent<Commands.DesCloneCommandComponent>())
                    obj.masterObject.AddComponent<Commands.DesCloneCommandComponent>();
            }
        }


    }
}
