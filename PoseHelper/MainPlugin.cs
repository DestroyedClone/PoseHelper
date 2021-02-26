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
            CreateSkins();
            FreeTheLockedMage();
        }

        private void Hooks()
        {
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private void FreeTheLockedMage()
        {
            GameObject magePrefab = Resources.Load<GameObject>("prefabs/networkedobjects/LockedMage");
            Destroy(magePrefab.GetComponent<GameObjectUnlockableFilter>());
            magePrefab.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(ReleasingMage));
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
                        localMaster.GetBody().characterMotor.Motor.SetPositionAndRotation(new Vector3(0.12f, 0.91f, 7.76f), Quaternion.identity, true);
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

        public static GameObject characterPrefab = Resources.Load<GameObject>("MageBody");
        private static void CreateSkins()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            //ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            //GameObject coatObject = childLocator.FindChild("Coat").gameObject;

            #region SkeleSkin
            SkinDef defaultSkin = Skins.CreateSkinDef("MAGE_BODY_ALTAR_SKELETON",
                Resources.Load<Sprite>("textures/bufficons/texBuffDeathMarkIcon"),
                defaultRenderers,
                mainRenderer,
                model);
            /*
            defaultSkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = coatObject,
                    shouldActivate = false
                }
            };*/

            skins.Add(defaultSkin);
            #endregion

            skinController.skins = skins.ToArray();
        }

    }
}
