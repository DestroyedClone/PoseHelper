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

namespace LeBuilder
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
   nameof(SurvivorAPI),

        //scene building
   nameof(DirectorAPI)
   )]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "LeBuilder";
        public const string ModGuid = "com.DestroyedClone.LeBuilder";

        internal static BepInEx.Logging.ManualLogSource _logger;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _logger = Logger;
            CommandHelper.AddToConsoleWhenReady();
            Other();
        }

        private void Other()
        {
            On.RoR2.SceneDirector.Start += CoverUpArtifactHoles;
            On.RoR2.GlobalEventManager.OnCharacterHitGround += ExtinguishInWaterJump;
            On.RoR2.FootstepHandler.Footstep_string_GameObject += ExtinguishFootstep;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }



        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            //var component = obj.gameObject.GetComponent<TrollPhysics>();
            //if (!component)
            //{
            var component = obj.gameObject.AddComponent<TrollPhysics>();
            component.characterBody = obj;
            //}
        }

        public static SurfaceDef waterSD = Resources.Load<SurfaceDef>("surfacedefs/sdWater");
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

        private bool CheckForWater(Vector3 position)
        {
            if (Physics.Raycast(new Ray(position + Vector3.up * 1.5f, Vector3.down), out RaycastHit raycastHit, 4f, LayerIndex.world.mask | LayerIndex.water.mask, QueryTriggerInteraction.Collide))
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

        private void Extinguish(CharacterBody characterBody)
        {
            if (characterBody.HasBuff(BuffIndex.OnFire))
            {
                Chat.AddMessage("cured " + characterBody.GetUserName());
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

        private void CoverUpArtifactHoles(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "artifactworld")
            {
                var gameplaySpace = GameObject.Find("HOLDER: Gameplay Space");
                var holder = new GameObject("MODDEDHOLDER: Caulks");
                //holder.transform.parent = gameplaySpace.transform;
                var blockRef1x3 = gameplaySpace.transform.Find("AW Basic Bridge 1").transform.Find("AW_Cube1x3x1").gameObject;
                var blockRef1x0p51 = gameplaySpace.transform.Find("AW Basic Bridge 1").transform.Find("AW_Cube1x0.5x1").gameObject;
                //var blockRef1x3 = GameObject.Find("HOLDER: Gameplay Space").transform.Find("AW Basic Bridge 1 (1)").transform.Find("AW_Cube2x3x1 (2)").gameObject;

                CreateObject(blockRef1x3, new Vector3(88f, -15.3f, 67f), new Vector3(0f, 0f, 270f), 0, holder);
                CreateObject(blockRef1x3, new Vector3(69f, -15.3f, 85f), new Vector3(0f, 0f, 270f), 1, holder);
                CreateObject(blockRef1x0p51, new Vector3(82f, -15.3f, 1.5f), new Vector3(0f, 0f, 270f), 2, holder);
                CreateObject(blockRef1x3, new Vector3(48f, 0.7f, 83f), new Vector3(0f, 0f, 270f), 3, holder);
            }
        }

        private void CreateObject(GameObject reference, Vector3 position, Vector3 rotation, int objNum, GameObject holder)
        {
            var block1 = Instantiate(reference, position, Util.QuaternionSafeLookRotation(rotation));
            var refMeshFilter = reference.GetComponent<MeshFilter>();
            var blockMeshFilter = block1.GetComponent<MeshFilter>();
            Debug.Log("Placed caulk #"+objNum);
            block1.name = "Caulk"+ objNum;
            blockMeshFilter.sharedMesh = refMeshFilter.sharedMesh;
            blockMeshFilter.mesh = refMeshFilter.sharedMesh;
            block1.transform.parent = holder.transform;
        }

        public class TrollPhysics : MonoBehaviour
        {
            public CharacterBody characterBody;
            public void FixedUpdate()
            {
                if (characterBody && characterBody.HasBuff(BuffIndex.ClayGoo))
                {
                    characterBody.rigidbody.velocity += Vector3.up * 20f;
                }
            }
        }
    }
}
