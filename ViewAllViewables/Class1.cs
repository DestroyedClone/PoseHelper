using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ViewAllViewables
{
    [BepInPlugin("com.DestroyedClone.ViewAllViewables", "View All Viewables", "1.1.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> cfgPrintProgressToConsole;
        public static ConfigEntry<bool> cfgViewForAllLocalUsers;

        public void Awake()
        {
            cfgPrintProgressToConsole = Config.Bind("", "Print Viewed Content", false, "If true, then the game will print the names of the viewed content into console.");
            cfgViewForAllLocalUsers = Config.Bind("", "View For All Local Users", true, "If true, then the game will apply the viewed setting to all local users. If false, only the first user will be affected.");

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        [ConCommand(commandName = "viewall", flags = ConVarFlags.ExecuteOnServer, helpText = "Marks all unviewed icons as viewed. May cause the game to freeze momentarily. Use in lobby.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Console Command")]
        private static void ViewAllUnviewed(ConCommandArgs args)
        {
            UserProfile userProfile = args.GetSenderLocalUser().userProfile;
            var viewableNames = (from node in ViewablesCatalog.rootNode.Descendants()
                                 where node.shouldShowUnviewed(userProfile)
                                 select node.fullName);
            var amountScanned = 0;
            foreach (var viewableName in viewableNames.ToList())
            {
                if (string.IsNullOrEmpty(viewableName))
                {
                    continue;
                }
                amountScanned++;
                if (cfgPrintProgressToConsole.Value)
                    Debug.Log(viewableName);
                if (cfgViewForAllLocalUsers.Value)
                {
                    foreach (LocalUser localUser in LocalUserManager.readOnlyLocalUsersList)
                    {
                        localUser.userProfile.MarkViewableAsViewed(viewableName);
                    }
                }
                else
                {
                    LocalUserManager.readOnlyLocalUsersList[0].userProfile.MarkViewableAsViewed(viewableName);
                }
            }
            if (amountScanned != 0) Debug.Log($"Viewed {amountScanned} unviewed content!");
            else Debug.Log("Nothing left to view!");
        }
    }
}