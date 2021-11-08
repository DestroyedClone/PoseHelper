using BepInEx;
using R2API.Utils;
using RoR2;
using System.Globalization;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ChatExtras
{
    [BepInPlugin("com.DestroyedClone.Greentext", "Greentext", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class GreentextMain : BaseUnityPlugin
    {
        public void Awake()
        {
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
            On.RoR2.Chat.UserChatMessage.ConstructChatString += UserChatMessage_ConstructChatString;
        }

        private string UserChatMessage_ConstructChatString(On.RoR2.Chat.UserChatMessage.orig_ConstructChatString orig, Chat.UserChatMessage self)
        {
            if (self.sender)
            {
                NetworkUser component = self.sender.GetComponent<NetworkUser>();
                if (component)
                {
                    if (self.text.StartsWith(">"))
                        return string.Format(CultureInfo.InvariantCulture, "{0}: <color=#789922>{1}</color>", Util.EscapeRichTextForTextMeshPro(component.userName), Util.EscapeRichTextForTextMeshPro(self.text));
                    else if (self.text.EndsWith("<"))
                        return string.Format(CultureInfo.InvariantCulture, "{0}: <color=#E0727F>{1}</color>", Util.EscapeRichTextForTextMeshPro(component.userName), Util.EscapeRichTextForTextMeshPro(self.text));
                    return orig(self);
                }
            }
            return null;
        }
    }
}