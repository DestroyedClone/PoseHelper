using BepInEx;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace Spectator
{
    [BepInPlugin("com.DestroyedClone.Spectator", "Spectator", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public void Awake()
        {
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        [ConCommand(commandName = "spectate", flags = ConVarFlags.ExecuteOnServer, helpText = "spectate ENEMY")]
        private static void DeathStateClear(ConCommandArgs args)
        {
            var playerNetworkUser = NetworkUser.readOnlyLocalPlayersList[0];
            var playerMasterObject = playerNetworkUser.masterObject;
            var playerCameraRigController = playerNetworkUser.cameraRigController;

            CharacterMaster enemyToSpectate = null ;
            if (args.Count == 1)
            {
                var masters = CharacterMaster.instancesList;
                foreach (var master in masters)
                {
                    if (master.name.StartsWith(args.GetArgString(0)))
                    {
                        enemyToSpectate = master;
                    }
                }
            }
            if (enemyToSpectate != null)
            {
                playerNetworkUser.masterObject = enemyToSpectate.gameObject;
                playerNetworkUser.masterObject = enemyToSpectate.GetBodyObject();
            }
        }

        /*
         * 1. Get Player's NetworkUser (pNU) /
                2. Get reference to pNU's MasterObject (pMO)/
            3. Get reference to pNU's CameraRigController (pCRC)/
            4. set pMO to MO of enemy's master (eMO)/
            5. wait/
            6. set pMO to BodyObject of enemy (eBO)
         * */


        public class SpectatorComponent : MonoBehaviour
        {
            bool DoNextPart = false;
            public NetworkUser networkUser;
            public CharacterMaster enemyMaster;

            public void FixedUpdate()
            {
                if (DoNextPart)
                {
                    DoNextPart = false;
                }
            }

            public void WatchEnemy()
            {
                if (DoNextPart)
                {
                    Debug.Log("Can't do it yet!");
                }
                if (networkUser)
                {
                    var cameraRigController = networkUser.cameraRigController;
                    if (networkUser.masterObject && cameraRigController)
                    {
                        networkUser.masterObject = enemyMaster.gameObject;
                        DoNextPart = true;
                    }
                }
            }
        }
    }
}
