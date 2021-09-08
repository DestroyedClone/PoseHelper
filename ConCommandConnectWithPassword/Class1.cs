using BepInEx;
using RoR2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ConCommandConnectWithPassword
{
    [BepInPlugin("com.DestroyedClone.ConCommandConnectIncludesPassword", "Console Command Connect Includes Password", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.Networking.GameNetworkManager.CCConnect += GameNetworkManager_CCConnect;
        }

        private void GameNetworkManager_CCConnect(On.RoR2.Networking.GameNetworkManager.orig_CCConnect orig, ConCommandArgs args)
        {
            if (args.Count == 2)
            {
                RoR2.Console.instance.SubmitCmd(args.sender ? args.sender : null, $"cl_password {args.GetArgString(1)}", false);
            }
            orig(args);
        }
    }
}
