using BepInEx;
using RoR2;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace VanillaDamageTyped
{
    [BepInPlugin("com.DestroyedClone.ChangeSkin", "Change Skin", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class ChangeSkinMod : BaseUnityPlugin
    {
        public static StringBuilder stringBuilder = new StringBuilder();

        public void Start()
        {
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        [ConCommand(commandName = "loadout_set_skin_variant", flags = ConVarFlags.None, helpText = "loadout_set_skin [bodyName|currentBody] [index]")]
        public static void CCLoadoutSetSkinVariant(ConCommandArgs args)
        {
            BodyIndex argBodyIndex = args.GetSenderBody().bodyIndex;
            UserProfile userProfile = args.GetSenderLocalUser().userProfile;
            if (args.Count == 0)
            {
                Debug.Log($"Skin Index: {userProfile.loadout.bodyLoadoutManager.GetSkinIndex(argBodyIndex)}");
                return;
            }
            else if (args.Count > 0)
            {
                if (args.TryGetArgString(0).ToLower() != "self")
                {
                    argBodyIndex = args.GetArgBodyIndex(0);
                }
            }
            int argInt = args.GetArgInt(1);
            Loadout loadout = new Loadout();
            userProfile.loadout.Copy(loadout);
            loadout.bodyLoadoutManager.SetSkinIndex(argBodyIndex, (uint)argInt);
            userProfile.SetLoadout(loadout);
            if (args.senderMaster)
            {
                args.senderMaster.SetLoadoutServer(loadout);
            }
            if (args.senderBody)
            {
                args.senderBody.SetLoadoutServer(loadout);
            }
        }
    }
}