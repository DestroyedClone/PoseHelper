using BepInEx;
using R2API;
using R2API.Networking;
using RoR2;
using RoR2.VoidRaidCrab;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;
//using ThreeEyedGames;
using ThreeEyedGames;
using BepInEx.Configuration;
using UnityEngine.Networking.Types;
using R2API.Networking.Interfaces;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace SprayMod
{
    [BepInPlugin("com.DestroyedClone.SprayMod", "SprayMod", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    public class SprayModMain : BaseUnityPlugin
    {

        public static SprayModMain instance;

        public static Dictionary<NetworkUser, string> server_MasterDict = new Dictionary<NetworkUser, string>();
        public static Dictionary<NetworkUser, string> server_tempDict = new Dictionary<NetworkUser, string>();

        public static ConfigEntry<string> cfgMySprayURL;


        public void Start()
        {
            cfgMySprayURL = Config.Bind("", "Spray URL", "", "");

            instance = this;

            Assets.Setup();

            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;

            // Regular syncing
            Run.onRunStartGlobal += Server_OnRunStart;
            Stage.onServerStageBegin += Server_OnServerStageBegin;
        }


        private void Server_OnServerStageBegin(Stage stage)
        {
            if (NetworkServer.active)
                Server_SyncSprayInformation();
        }

        private void Server_OnRunStart(Run run)
        {
            if (NetworkServer.active)
                Server_SyncSprayInformation();
        }

        private void Server_SyncSprayInformation()
        {
            //First, we have to ask everyone for their spray URL.
            new Networking.ServerRequestingClientInfo().Send(NetworkDestination.Clients);
            foreach (var networkUser in NetworkUser.instancesList)
            {
                //this is where we would sanitize
                //if we fucking knew how to do it
                if (server_MasterDict.TryGetValue(networkUser, out string URL))
                {

                } else
                {
                    server_MasterDict.Add(networkUser, URL);
                }

            }
        }

        [ConCommand(commandName = "loadTex", flags = ConVarFlags.None, helpText = "loadTex [URL]")]
        public static void CCSpawnEncounter(ConCommandArgs args)
        {
            instance.DownloadImage(args.GetArgString(0));
            Assets.defaultSprayMaterial.mainTexture = Assets.loadedTexture;
            Assets.defaultSprayObjectPrefab.transform.Find("FX/Decal").GetComponent<Decal>().Material = Assets.defaultSprayMaterial;
            Debug.Log("Updated spray material");
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.isPlayerControlled)
            {
                obj.gameObject.AddComponent<SprayComponent>().charBody = obj;
            }
        }

        public class SprayComponent : MonoBehaviour
        {
            public GameObject sprayObjInstance;
            public CharacterBody charBody;

            public void OnDestroy()
            {
                if (sprayObjInstance)
                {
                    Destroy(sprayObjInstance);
                }
            }

            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (!sprayObjInstance)
                    {
                        sprayObjInstance = Object.Instantiate(Assets.defaultSprayObjectPrefab);
                    }

                    if (charBody.inputBank.GetAimRaycast(1000f, out RaycastHit hitInfo))
                    {
                        sprayObjInstance.transform.SetPositionAndRotation(hitInfo.point,
                            Quaternion.Euler(hitInfo.normal));
                    }
                }
            }
        }

        IEnumerator<UnityWebRequestAsyncOperation> DownloadImage(string MediaUrl)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);
            else
                Assets.loadedTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}