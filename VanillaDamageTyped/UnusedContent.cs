
using RoR2;
using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates;
using R2API;
using RoR2.Skills;
using RoR2;
using VanillaDamageTyped.Modules;
using UnityEngine.Networking;
using RoR2.Projectile;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]


namespace VanillaDamageTyped
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin("com.destroyedclone.unusedcontent", "unusedcontent", "1.0")]
    public class UnusedContent : BaseUnityPlugin
    {
        public static GameObject bellPrefab;

        public void Start()
        {
            OnLoad();
            //RoR2Application.onLoad += OnLoad;
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
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
                obj.AddComponent<DestroyOnBoolean>();
            } else
            {
                Debug.LogWarning($"Failed to load \"{args.GetArgString(0)}\"");
            }
        }

        //public void FireProjectile(GameObject prefab, Vector3 position, Quaternion rotation, GameObject owner,
        //float damage, float force, bool crit, DamageColorIndex damageColorIndex = DamageColorIndex.Default,
        //GameObject target = null, float speedOverride = -1f)
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

        public class DestroyOnBoolean : MonoBehaviour
        {
            public bool EnableToDestroy = false;
            public void FixedUpdate()
            {
                if (EnableToDestroy)
                    Destroy(transform.gameObject);
            }
        }
        [ConCommand(commandName = "setstate", flags = ConVarFlags.None, helpText = "setstate type [EntityStateMachine Name|Body]")]
        public static void CCSetState(ConCommandArgs args)
        {
            var esmName = "Body";
            if (args.Count > 1)
            {
                esmName = args.GetArgString(1);
            }
            EntityState sest = null;
            switch (args.GetArgString(0))
            {
                case "BeetleQueenMonster.WeakState":
                    sest = new EntityStates.BeetleQueenMonster.WeakState();
                    break;
                case "BeetleQueenMonster.BeginBurrow":
                    sest = new EntityStates.BeetleQueenMonster.BeginBurrow(); //zero duration
                    break;
                case "BrotherMonster.ThroneSpawnState":
                    sest = new EntityStates.BrotherMonster.ThroneSpawnState();
                    break;
                case "Weapon.CallSupplyDropForce":
                    sest = new EntityStates.Captain.Weapon.CallSupplyDropForce();
                    break;
                case "Weapon.CallSupplyDropPlating":
                    //sest = new SerializableEntityStateType(typeof(EntityStates.Captain.Weapon.Weapon.CallSupplyDropPlating));
                    sest = new EntityStates.Captain.Weapon.CallSupplyDropPlating();
                    break;
                case "Weapon.CaptainSupplyDrop.DepletionState":
                    //sest = new SerializableEntityStateType(typeof(EntityStates.Captain.Weapon.Weapon.CallSupplyDropPlating));
                    sest = new EntityStates.CaptainSupplyDrop.DepletionState();
                    break;
                case "CaptainSupplyDrop.PreDepletionState":
                    //sest = new SerializableEntityStateType(typeof(EntityStates.Captain.Weapon.Weapon.CallSupplyDropPlating));
                    sest = new EntityStates.CaptainSupplyDrop.PreDepletionState();
                    break;
                default:
                    Debug.LogWarning("No valid entitystate stated, aborting for safety.");
                    break;
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


        public static void OnLoad()
        {
            //bellPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBody.prefab").WaitForCompletion();
            //bellPrefab.GetComponent<GenericSkill>().skillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Bell.BellWeapon.BuffBeam));

            foreach (var a in new Modules.CharacterMain[] {
                //new Commando(),
                //new Engineer(),
                new Railgunner(),
                //new VoidSurvivor(),

                
                //new Beetle(),
                //new BeetleQueen(),
                //new Bell()

            })
            {
                Debug.Log($"Setting up for {a}");
                a.Init();
            }
            EntityStates.BeetleQueenMonster.BeginBurrow.duration += 2f;
        }

        public static void SetupSkillDefs()
        {
            
        }
    }
}
