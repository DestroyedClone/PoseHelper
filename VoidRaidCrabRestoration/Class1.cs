using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.VoidRaidCrab;
using EntityStates;
using System.Security;
using System.Security.Permissions;
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
namespace VoidRaidCrabRestoration
{
    [BepInPlugin("com.DestroyedClone.VoidlingRestored", "Voidling Restored", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2API.Utils.R2APISubmoduleDependency(nameof(PrefabAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public static CharacterSpawnCard cscVoidRaidCrab, cscVoidRaidCrabJoint;
        public static GameObject restoredBossEncounter;

        public static StringBuilder stringBuilder = new StringBuilder();

        public static SceneDef dampCaveSD, golemPlainsTrailer, scnNetTest, scnNetTest2, slice1, slice2, space, stage1;
        public static SceneDef[] sceneDefs = new SceneDef[]
        {
            dampCaveSD, golemPlainsTrailer, scnNetTest, scnNetTest2, slice1, slice2, space, stage1
        };
        static int i = -1;

        public void Start()
        {
            cscVoidRaidCrab = Load<CharacterSpawnCard>("RoR2/DLC1/VoidRaidCrab/cscVoidRaidCrab.asset");
            cscVoidRaidCrabJoint = Load<CharacterSpawnCard>("RoR2/DLC1/VoidRaidCrab/cscVoidRaidCrabJoint.asset");
            /*
            static SceneDef CreateSceneDef(string assetReference)
            {
                i++;
                AssetReferenceScene ars = new AssetReferenceScene(assetReference);
                var sd = ScriptableObject.CreateInstance<SceneDef>();
                sd.baseSceneNameOverride = $"sceneDef{i}";
                sd.cachedName = $"sceneDef{i}";
                sd.blockOrbitalSkills = false;
                sd.suppressNpcEntry = false;
                sd.suppressPlayerEntry = false;
                sd.sceneAddress = ars;
                sd.dioramaPrefab = null;
                sd.sceneType = SceneType.Stage;
                sd.loreToken = $"SCENE{i}_LORE";
                sd.nameToken = $"SCENE{i}_NAME";
                sd.subtitleToken = $"SCENE{i}_SUBTITLE";
                (sd as ScriptableObject).name = $"sceneDef{i}";
                sd.shouldIncludeInLogbook = false;
                ContentAddition.AddSceneDef(sd);
                return sd;
            }

            dampCaveSD = CreateSceneDef("RoR2/Junk/dampcave/dampcave.unity");
            golemPlainsTrailer = CreateSceneDef("RoR2/Junk/golemplains_trailer/golemplains_trailer.unity");
            scnNetTest = CreateSceneDef("RoR2/Junk/scnNetTest/scnNetTest.unity");
            scnNetTest2 = CreateSceneDef("RoR2/Junk/scnNetTest2/scnNetTest2.unity");
            slice1 = CreateSceneDef("RoR2/Junk/slice1/slice1.unity");
            slice2 = CreateSceneDef("RoR2/Junk/slice2/slice2.unity");
            space = CreateSceneDef("RoR2/Junk/space/space.unity");
            stage1 = CreateSceneDef("RoR2/Junk/stage1/stage1.unity");
            */
            //On.RoR2.PhasedInventorySetter.FixedUpdate += PhasedInventorySetter_FixedUpdate;
            CreateBossEncounter();
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            //On.RoR2.GivePickupsOnStart.Start += GivePickupsOnStart_Start;
            //On.RoR2.EquipmentSlot.FireFireBallDash += EquipmentSlot_FireFireBallDash;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            //On.RoR2.BackstabManager.Init += BackstabManager_Init;
            //RoR2.CharacterBody.onBodyStartGlobal += GiveCutStarterItems;
            //On.EntityStates.Mage.Weapon.FireRoller.OnEnter += AttuneToItemCount;
        }

        private void AttuneToItemCount(On.EntityStates.Mage.Weapon.FireRoller.orig_OnEnter orig, EntityStates.Mage.Weapon.FireRoller self)
        {
            orig(self);
        }

        private void GiveCutStarterItems(RoR2.CharacterBody characterBody)
        {
            var survivorIndex = SurvivorCatalog.GetSurvivorIndexFromBodyIndex(characterBody.bodyIndex);
            if (survivorIndex == RoR2.RoR2Content.Survivors.Loader.survivorIndex)
            {
                if (!characterBody.GetComponent<LoaderStaticChargeComponent>())
                {
                    var staticCharge = characterBody.gameObject.AddComponent<LoaderStaticChargeComponent>();
                    staticCharge.characterBody = characterBody;
                }
            }
            else if (survivorIndex == RoR2Content.Survivors.Mage.survivorIndex)
            {
                if (characterBody.inventory)
                {
                    characterBody.inventory.GiveItem(JunkContent.Items.MageAttunement);
                }
                if (!characterBody.GetComponent<MageCalibrationController>())
                {
                    var controller = characterBody.gameObject.AddComponent<MageCalibrationController>();
                    controller.characterBody = characterBody;
                }
            }
            else if (survivorIndex == RoR2Content.Survivors.Treebot.survivorIndex)
            {
                if (!characterBody.GetComponent<TreebotSunBuffGranter>())
                {
                    var sunbuff = characterBody.gameObject.AddComponent<TreebotSunBuffGranter>();
                    sunbuff.characterBody = characterBody;
                }
            }
        }

        private void BackstabManager_Init(On.RoR2.BackstabManager.orig_Init orig)
        {
            orig();
            BackstabManager.enableVisualizerSystem = true;
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            obj.gameObject.AddComponent<SpawnObjController>();
        }

        private bool EquipmentSlot_FireFireBallDash(On.RoR2.EquipmentSlot.orig_FireFireBallDash orig, EquipmentSlot self)
        {
            if (self.inventory.GetItemCount(RoR2Content.Items.LunarDagger) > 0)
            {
                return EquipmentSlot_FireLunarFireBallDash(self);
            } 
            return orig(self);
        }

        private bool EquipmentSlot_FireLunarFireBallDash(EquipmentSlot self)
        {
            var lunarFireballVehicle = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Misc/LunarFireballVehicle.prefab").WaitForCompletion();
            Ray aimRay = self.GetAimRay();
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(lunarFireballVehicle, aimRay.origin, Quaternion.LookRotation(aimRay.direction));
            gameObject.GetComponent<VehicleSeat>().AssignPassenger(base.gameObject);
            CharacterBody characterBody = self.characterBody;
            NetworkUser networkUser;
            if (characterBody == null)
            {
                networkUser = null;
            }
            else
            {
                CharacterMaster master = characterBody.master;
                if (master == null)
                {
                    networkUser = null;
                }
                else
                {
                    PlayerCharacterMasterController playerCharacterMasterController = master.playerCharacterMasterController;
                    networkUser = ((playerCharacterMasterController != null) ? playerCharacterMasterController.networkUser : null);
                }
            }
            NetworkUser networkUser2 = networkUser;
            if (networkUser2)
            {
                NetworkServer.SpawnWithClientAuthority(gameObject, networkUser2.gameObject);
            }
            else
            {
                NetworkServer.Spawn(gameObject);
            }
            self.subcooldownTimer = 2f;
            return true;
        }

        private void GivePickupsOnStart_Start(On.RoR2.GivePickupsOnStart.orig_Start orig, GivePickupsOnStart self)
        {
            orig(self);
            var sb = new StringBuilder();
            sb.Append($"For Body: {self.inventory.GetComponent<CharacterMaster>()?.GetBody()?.name}");
            foreach (var item in self.itemDefInfos)
            {
                sb.AppendLine($"{item.itemDef.name} ({item.itemDef.itemIndex}) x{item.count}, dontExceed:{item.dontExceedCount}");
            }

            foreach (var item in self.itemInfos)
            {
                sb.AppendLine($"{item.itemString} x{item.count}");
            }
            Debug.Log(sb.ToString());
        }

        [ConCommand(commandName = "fightvoidcrab", flags = ConVarFlags.None, helpText = "fightvoidcrab [xyz|corePosition]")]
        public static void CCSpawnEncounter(ConCommandArgs args)
        {
            Vector3 position = args.senderBody.corePosition;
            if (args.Count > 0)
            {
                position = new Vector3(args.GetArgFloat(0), args.GetArgFloat(1), args.GetArgFloat(2));
            }
            SpawnEncounter(position);
        }


        [ConCommand(commandName = "spawnobj_parent", flags = ConVarFlags.None, helpText = "become {nameOfGameObject}")]
        public static void CCSpawnObjectParent(ConCommandArgs args)
        {
            GameObject loadedAsset = null;
            Vector3 position = args.senderBody.corePosition;
            if (args.Count > 0)
            {
                loadedAsset = Addressables.LoadAssetAsync<GameObject>(args.GetArgString(0)).WaitForCompletion();
            }
            if (args.Count > 1)
            {
                position = new Vector3(args.GetArgFloat(1), args.GetArgFloat(2), args.GetArgFloat(3));
            }
            if (loadedAsset)
            {
                var obj = UnityEngine.Object.Instantiate(loadedAsset, position, Quaternion.identity);
                obj.AddComponent<DestroyOnDisable>();
                obj.transform.SetParent(SpawnObjController.instance.field_transform);
            }
            else
            {
                Debug.LogWarning($"Failed to load \"{args.GetArgString(0)}\"");
            }
        }

        [ConCommand(commandName = "spawnproj", flags = ConVarFlags.None, helpText = "spawnproj [object] [x,y,z|user pos] [owner|self] [rotation|aimLook]")]
        public static void CCSpawnProjectile(ConCommandArgs args)
        {
            GameObject loadedAsset = null;
            Vector3 spawnedPos = args.senderBody.corePosition;
            GameObject projOwner = args.senderBody.gameObject;
            Quaternion rotation = Quaternion.Euler(args.senderBody.inputBank.aimDirection);

            if (args.Count > 0)
            {
                loadedAsset = Addressables.LoadAssetAsync<GameObject>(args.GetArgString(0)).WaitForCompletion();
            }
            if (args.Count > 1)
            {
                spawnedPos = new Vector3(args.GetArgFloat(1), args.GetArgFloat(2), args.GetArgFloat(3));
            }
            if (args.Count > 3)
            {
                var bodyName = args.GetArgString(4);
                if (bodyName.ToLower() == "none")
                {
                    projOwner = null;
                } else
                {
                    bool foundBody = false;
                    foreach (var body in CharacterBody.readOnlyInstancesList)
                    {
                        if (body.name == bodyName)
                        {
                            projOwner = body.gameObject;
                            break;
                        }
                    }
                    if (!foundBody)
                    {
                        Debug.Log($"Couldn't find body with name \"{bodyName}\"");
                    }
                }
            }
            if (args.Count > 4)
            {
                rotation = Quaternion.Euler(args.GetArgFloat(5), args.GetArgFloat(6), args.GetArgFloat(7));
            }
            if (loadedAsset)
            {
                //var obj = UnityEngine.Object.Instantiate(loadedAsset, position, Quaternion.identity);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
                {
                    projectilePrefab = loadedAsset,
                    position = spawnedPos,
                    owner = projOwner,
                    rotation = rotation
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            else
            {
                Debug.LogWarning($"Failed to load \"{args.GetArgString(0)}\"");
            }
        }

        [ConCommand(commandName = "setstate", flags = ConVarFlags.None, helpText = "setstate path [EntityStateMachine Name|Body]")]
        public static void CCSetState(ConCommandArgs args)
        {
            var esmName = "Body";
            if (args.Count > 1)
            {
                esmName = args.GetArgString(1);
            }
            EntityState sest = null;
            if (args.Count > 0)
            {
                sest = Addressables.LoadAssetAsync<EntityState>(args.GetArgString(0)).WaitForCompletion();
            }
            foreach (var esm in args.senderBody.GetComponents<EntityStateMachine>())
            {
                if (esm.customName == esmName)
                {
                    esm.SetState(sest);
                    return;
                }
            }
        }

        [ConCommand(commandName = "setskilldef", flags = ConVarFlags.None, helpText = "setskilldef path [Slot|0]")]
        public static void CCSetSkillDef(ConCommandArgs args)
        {
            SkillDef sest = null;
            if (args.Count > 0)
            {
                sest = Addressables.LoadAssetAsync<SkillDef>(args.GetArgString(0)).WaitForCompletion();
            }
            int slot = 0;
            if (args.Count > 1)
            {
                slot = args.GetArgInt(1);
            }
            var skillLoc = args.senderBody.GetComponent<SkillLocator>();
            GenericSkill gs = null;
            if (skillLoc)
            {
                switch (slot)
                {
                    case 0:
                        gs = skillLoc.primary;
                        break;
                    case 1:
                        gs = skillLoc.secondary;
                        break;
                    case 2:
                        gs = skillLoc.utility;
                        break;
                    case 3:
                        gs = skillLoc.special;
                        break;
                }
            }
            gs.SetBaseSkill(sest);
        }

        [ConCommand(commandName = "setskill", flags = ConVarFlags.None, helpText = "setskill path [Slot|0]")]
        public static void CCSetSkill(ConCommandArgs args)
        {
            SerializableEntityStateType sest = Addressables.LoadAssetAsync<SerializableEntityStateType>(args.GetArgString(0)).WaitForCompletion();;
            int slot = 0;
            if (args.Count > 1)
            {
                slot = args.GetArgInt(1);
            }
            var skillLoc = args.senderBody.GetComponent<SkillLocator>();
            GenericSkill gs = null;
            if (skillLoc)
            {
                switch (slot)
                {
                    case 0:
                        gs = skillLoc.primary;
                        break;
                    case 1:
                        gs = skillLoc.secondary;
                        break;
                    case 2:
                        gs = skillLoc.utility;
                        break;
                    case 3:
                        gs = skillLoc.special;
                        break;
                }
            }
            gs.skillDef.activationState = sest;
        }

        public class SpawnObjController : MonoBehaviour
        {
            public Transform field_transform;
            public GameObject field_gameObject;
            public Material material;

            public static SpawnObjController instance;

            public void Start()
            {
                instance = this;
            }
        }
        
        [ConCommand(commandName = "list_buffs", flags = ConVarFlags.None, helpText = "list_buffs")]
        public static void CCListBuffs(ConCommandArgs args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var buffDef in BuffCatalog.buffDefs)
            {
                sb.AppendLine($"{buffDef.buffIndex} {buffDef.name} {buffDef.isDebuff}");
            }
            Debug.Log(sb.ToString());
        }

        [ConCommand(commandName = "give_buff", flags = ConVarFlags.None, helpText = "give_buff buffIndex duration stacks")]
        public static void CCGiveBuff(ConCommandArgs args)
        {
            var buffDef = BuffCatalog.GetBuffDef((BuffIndex)args.GetArgInt(0));
            var duration = args.GetArgInt(1);
            var maxStacks = args.GetArgInt(2);
            args.senderBody.AddTimedBuff(buffDef, duration);
        }

        [ConCommand(commandName = "clear_buff", flags = ConVarFlags.None, helpText = "clear_buff buffIndex")]
        public static void CCClearBuff(ConCommandArgs args)
        {
            var buffDef = BuffCatalog.GetBuffDef((BuffIndex)args.GetArgInt(0));
            args.senderBody.ClearTimedBuffs(buffDef);
        }


        [ConCommand(commandName = "become", flags = ConVarFlags.None, helpText = "become")]
        public static void CCBecomeCopy(ConCommandArgs args)
        {
            args.senderMaster.bodyPrefab = SpawnObjController.instance.field_gameObject;
            args.senderMaster.Respawn(args.senderBody.footPosition, Quaternion.identity);
            Debug.Log($"Spawned as {SpawnObjController.instance.field_gameObject.name}");
        }

        private void PhasedInventorySetter_FixedUpdate(On.RoR2.PhasedInventorySetter.orig_FixedUpdate orig, PhasedInventorySetter self)
        {
            orig(self);
            stringBuilder.Clear();
            int i = 0;
            foreach (var phase in self.phases)
            {
                stringBuilder.AppendLine($"Phase {i}");
                foreach (var itemCountPair in phase.itemCounts)
                {
                    stringBuilder.AppendLine($"{itemCountPair.itemDef.name} {itemCountPair.itemDef.nameToken} x{itemCountPair.count}");
                }
                i++;
            }
            Logger.LogMessage(stringBuilder.ToString());
            On.RoR2.PhasedInventorySetter.FixedUpdate -= PhasedInventorySetter_FixedUpdate;
        }

        public void CreateBossEncounter()
        {
            restoredBossEncounter = PrefabAPI.InstantiateClone(new GameObject(), "VoidRaidCrabAndJointEncounter");
            var bossGroup = restoredBossEncounter.AddComponent<BossGroup>();
            var combatSquad = restoredBossEncounter.AddComponent<CombatSquad>();
        }

        public static T Load<T>(string assetPath)
        {
            var loadedAsset = Addressables.LoadAssetAsync<T>(assetPath).WaitForCompletion();
            return loadedAsset;
        }

        [ConCommand(commandName = "spawnmat", flags = ConVarFlags.None, helpText = "spawnmat")]
        public static void CCSpawnMaterial(ConCommandArgs args)
        {
            Material loadedAsset = null;
            if (args.Count > 0)
            {
                loadedAsset = Addressables.LoadAssetAsync<Material>(args.GetArgString(0)).WaitForCompletion();
            }
            if (loadedAsset)
            {
                SpawnObjController.instance.material = loadedAsset;
            }
            else
            {
                Debug.LogWarning($"Failed to load \"{args.GetArgString(0)}\"");
            }
        }

        public static void SpawnEncounter(Vector3 position)
        {
            List<CharacterMaster> characterMasters = new List<CharacterMaster>();

            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule()
            {
                maxDistance = 0,
                minDistance = 0,
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position,
            };
            DirectorSpawnRequest dsrVoidRaidCrab = new DirectorSpawnRequest(cscVoidRaidCrab, directorPlacementRule, Run.instance.spawnRng);
            DirectorSpawnRequest dsrJoint = new DirectorSpawnRequest(cscVoidRaidCrabJoint, directorPlacementRule, Run.instance.spawnRng);

            dsrVoidRaidCrab.ignoreTeamMemberLimit = true;
            dsrVoidRaidCrab.onSpawnedServer += SpawnLeg;
            dsrVoidRaidCrab.teamIndexOverride = TeamIndex.Void;

            dsrJoint.ignoreTeamMemberLimit = true;
            dsrJoint.teamIndexOverride = TeamIndex.Void;

            var goVoidRaidCrab = DirectorCore.instance.TrySpawnObject(dsrVoidRaidCrab);
            var charMaster = goVoidRaidCrab.GetComponent<CharacterMaster>();
            characterMasters.Add(charMaster);
            var body = charMaster.GetBody();
            var voidRaidCrabHealthBarOverlayProvider = body.GetComponent<VoidRaidCrabHealthBarOverlayProvider>(); //needs to activate post-boss healthbar
            var centralLegController = body.GetComponent<CentralLegController>();
            var legOriginTransforms = centralLegController.legControllers[0].transform.parent;

            /*
            * backLeg.thigh.l
            * backLeg.thigh.r
            * frontLeg.thigh.l
            * frontLeg.thigh.r
            * midLeg.thigh.l
            * migLeg.thigh.r
             */
            //Returns CharacterMaster
            LegController leg(int index)
            {
                return centralLegController.legControllers[index];
            }
            Transform o(string name)
            {
                return legOriginTransforms.Find(name);
            }
            CharacterMaster AddLeg(LegController legController, Transform originTransform)
            {
                var goJoint = DirectorCore.instance.TrySpawnObject(dsrJoint);
                goJoint.GetComponent<MinionOwnership>().SetOwner(charMaster);
                var jointMaster = goJoint.GetComponent<CharacterMaster>();
                var jointBody = jointMaster.GetBody();
                jointBody.transform.parent = originTransform;
                jointBody.transform.localPosition = Vector3.zero;

                jointMaster.transform.parent = originTransform;
                jointMaster.transform.localPosition = Vector3.zero;

                var jointChildLocator = jointBody.GetComponent<ChildLocator>();
                jointBody.GetComponent<ChildLocatorMirrorController>().targetLocator = jointChildLocator;

                legController.SetJointMaster(jointMaster, jointChildLocator);
                characterMasters.Add(jointMaster);
                return jointMaster;
            }
            var backLegL = AddLeg(leg(0), o("BackLegLOrigin"));
            var backLegR = AddLeg(leg(1), o("BackLegROrigin"));
            var frontLegL = AddLeg(leg(2), o("FrontLegLOrigin"));
            var frontLegR = AddLeg(leg(3), o("FrontLegROrigin"));
            var midLegL = AddLeg(leg(4), o("MidLegLOrigin"));
            var midLegR = AddLeg(leg(5), o("MidLegROrigin"));

            var foundBossGroups = UnityEngine.Object.FindObjectsOfType<BossGroup>();
            foreach (var bossGroup in foundBossGroups)
            {
                if (bossGroup.transform.name == "RestoredBossGroup"
                || bossGroup.bossMemoryCount > 0)
                {
                    //bossGroup.AddBossMemory(charMaster);
                    var firstMemory = bossGroup.bossMemories[0];
                    firstMemory.cachedMaster.gameObject.SetActive(false);
                    firstMemory.cachedBody.gameObject.SetActive(false);
                    bossGroup.bossMemories = new BossGroup.BossMemory[] { };
                    bossGroup.RememberBoss(charMaster);
                    bossGroup.AddBossMemory(charMaster);
                }
            }
        }

        [ConCommand(commandName = "loadsceneasset", flags = ConVarFlags.None, helpText = "loadsceneasset [assetpath]")]
        public static void CCLoadScene(ConCommandArgs args)
        {
            //UnityEngine.ResourceManagement.ResourceProviders.SceneInstance loadedScene;
            
            //AssetReference scene;
            if (args.Count > 0)
            {
                //loadedScene = Addressables.LoadSceneAsync(args.GetArgString(0)).WaitForCompletion();
                //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadedScene.Scene.buildIndex, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }

        [ConCommand(commandName = "spawnobj", flags = ConVarFlags.None, helpText = "spawnobj [object] [x,y,z|user pos]")]
        public static void CCSpawnObject(ConCommandArgs args)
        {
            GameObject loadedAsset = null;
            Vector3 position = args.senderBody.corePosition;
            if (args.Count > 0)
            {
                loadedAsset = Addressables.LoadAssetAsync<GameObject>(args.GetArgString(0)).WaitForCompletion();
            }
            if (args.Count > 1)
            {
                position = new Vector3(args.GetArgFloat(1), args.GetArgFloat(2), args.GetArgFloat(3));
            }
            if (loadedAsset)
            {
                var obj = UnityEngine.Object.Instantiate(loadedAsset, position, Quaternion.identity);
                obj.AddComponent<DestroyOnDisable>();
            }
            else
            {
                Debug.LogWarning($"Failed to load \"{args.GetArgString(0)}\"");
            }
        }
        public class DestroyOnDisable : MonoBehaviour
        {
            public void OnDisable()
            {
                Destroy(transform.gameObject);
            }
        }

        public static void SpawnLeg(SpawnCard.SpawnResult spawnResult)
        {
        }
    }
}