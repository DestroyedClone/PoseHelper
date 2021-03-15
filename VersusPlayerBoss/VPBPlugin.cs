using BepInEx;
using BepInEx.Configuration;
using LeTai.Asset.TranslucentImage;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

namespace VersusPlayerBoss
{
    [BepInPlugin("com.DestroyedClone.VersusSuperHail", "VersusSuperHail", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class VSHPlugin : BaseUnityPlugin
    {
        
        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();
        }

        public class HaleTracker : MonoBehaviour
        {
            private List<NetworkUser> networkUsers = new List<NetworkUser>();
            public NetworkUser currentHale = new NetworkUser();
            public string ControlPanel = "-------------------------------------------";
            public NetworkUser ChosenHale = new NetworkUser();
            public bool SetHaleConfirm = false;
            //public bool UpdateNetworkList = false;
            public BossGroup bossGroup;
            public BossGroup.BossMemory bossMemory;
            public CombatSquad combatSquad;


            public void Awake()
            {
                networkUsers = new List<NetworkUser>(NetworkUser.readOnlyInstancesList);
            }

            public void Update()
            {
                if (SetHaleConfirm)
                {
                    SetHaleConfirm = false;
                    SetHale(ChosenHale);
                }
            }

            public void SetHale(NetworkUser networkUser)
            {
                if (networkUser)
                {
                    if (networkUsers.Contains(networkUser))
                    {
                        currentHale = networkUser;
                        bossMemory = new BossGroup.BossMemory()
                        {
                            cachedMaster = currentHale.master,
                            cachedBody = currentHale.GetCurrentBody()
                        };
                        combatSquad = new CombatSquad();
                        combatSquad.AddMember(currentHale.master);
                        bossGroup = new BossGroup()
                        {
                            combatSquad = this.combatSquad,
                            bestObservedName = currentHale.userName,
                            bestObservedSubtitle = "The Boss",
                        };
                        bossGroup.AddBossMemory(currentHale.master);
                        bossGroup.combatSquad = combatSquad;
                    }
                    else
                    {
                        Debug.LogError("Couldn't find NetworkUser" + networkUser + "in list of available NetworkUsers");
                    }
                }
                else
                {
                    Debug.LogError("NetworkUser " + networkUser + " does not exist!");
                }
            }
        }
    }

    public static class Commands
    {
        [ConCommand(commandName = "vsh_enable", flags = ConVarFlags.ExecuteOnServer, helpText = "Enables VSH")]
        public static void Diorama(ConCommandArgs args)
        {
            if (!HasHaleTracker(Stage.instance.gameObject))
                Stage.instance.gameObject.AddComponent<VSHPlugin.HaleTracker>();
        }

        public static VSHPlugin.HaleTracker HasHaleTracker(GameObject gameObject)
        {
            if (gameObject)
                return gameObject.GetComponent<VSHPlugin.HaleTracker>();
            return null;
        }
    }
}
