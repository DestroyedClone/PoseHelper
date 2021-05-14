using BepInEx;
using RoR2;
using UnityEngine;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using R2API.Utils;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ViewAllViewables
{
    [BepInPlugin("com.DestroyedClone.ViewAllViewables", "View All Viewables", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        [ConCommand(commandName = "viewall", flags = ConVarFlags.ExecuteOnServer, helpText = "Marks all unviewed icons as viewed. May cause the game to freeze momentarily. Use in lobby.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Console Command")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Empty Arg required")]
        private static void ViewAllUnviewed(ConCommandArgs args)
        {
            UserProfile userProfile = args.GetSenderLocalUser().userProfile;
            var viewableNames = (from node in ViewablesCatalog.rootNode.Descendants()
                                 where node.shouldShowUnviewed(userProfile)
                                 select node.fullName);
            var hasScanned = false;
            foreach (var viewableName in viewableNames.ToList())
            {
                Debug.Log(viewableName);
                if (string.IsNullOrEmpty(viewableName))
                {
                    continue;
                }
                foreach (LocalUser localUser in LocalUserManager.readOnlyLocalUsersList)
                {
                    localUser.userProfile.MarkViewableAsViewed(viewableName);
                    hasScanned = true;
                }
            }
            if (!hasScanned) Debug.Log("Nothing left to view!");
        }
    }
}
