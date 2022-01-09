using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MountainCount
{
    [BepInPlugin("com.DestroyedClone.MountainCount", "MountainCount", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public static ConfigEntry<bool> cfgPrintOnTeleporterEnd;
        public static ConfigEntry<bool> cfgEditLanguage;
        public static ConfigEntry<bool> cfgAddChatMessage;
        public static ConfigEntry<bool> cfgIncrement;

        public void Start()
        {
            cfgPrintOnTeleporterEnd = Config.Bind("", "Activate on Teleporter Boss Death", true, "If true, then the amount will be printed in chat when the teleporter boss dies.");
            cfgEditLanguage = Config.Bind("", "Edit Language", true, "Modifies boss shrine usage text to show the current amount.");
            cfgAddChatMessage = Config.Bind("", "Add Command", true, "If true, adds a chat message." +
                "\n'x?' will show the amount.");
            cfgIncrement = Config.Bind("", "Increment Value", false, "If true, the value printed will be incremented by 1." +
                "\nWith the value increased by 1, this shows how many items each player should get.");

            if (cfgPrintOnTeleporterEnd.Value)
                On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            if (cfgAddChatMessage.Value || cfgEditLanguage.Value)
                On.RoR2.Chat.SendBroadcastChat_ChatMessageBase += Chat_SendBroadcastChat_ChatMessageBase;


            if (cfgEditLanguage.Value)
            {
                Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
            }
        }

        private void Language_onCurrentLanguageChanged()
        {
            LanguageAPI.Add("QOL_SHRINE_BOSS_USE_MESSAGE", Language.GetString("SHRINE_BOSS_USE_MESSAGE", Language.currentLanguage.name) + " (x{1})", Language.currentLanguage.name);
            LanguageAPI.Add("QOL_SHRINE_BOSS_USE_MESSAGE_2P", Language.GetString("SHRINE_BOSS_USE_MESSAGE_2P", Language.currentLanguage.name) + " (x{1})", Language.currentLanguage.name);
        }

        private int GetShrineCount()
        {
            if (TeleporterInteraction.instance)
                return TeleporterInteraction.instance.shrineBonusStacks + (cfgIncrement.Value ? 1 : 0);
            return 0;
        }

        private void Chat_SendBroadcastChat_ChatMessageBase(On.RoR2.Chat.orig_SendBroadcastChat_ChatMessageBase orig, ChatMessageBase message)
        {
            if (cfgEditLanguage.Value && message is Chat.SubjectFormatChatMessage subjectMsg)
            {
                if (subjectMsg.baseToken == "SHRINE_BOSS_USE_MESSAGE")
                {
                    subjectMsg.baseToken = "QOL_SHRINE_BOSS_USE_MESSAGE";
                    subjectMsg.paramTokens = new string[] { $"{GetShrineCount()}" };
                }
            }

            // x? command
            bool isAskingForShrineAmount = false;
            if (cfgAddChatMessage.Value && message is Chat.UserChatMessage chatMsg)
            {
                switch (chatMsg.text)
                {
                    case "x?":
                    case "x ?":
                        isAskingForShrineAmount = true;
                        break;
                }
            }
            orig(message);
            if (isAskingForShrineAmount)
            {
                SayMountainShrineAmount();
            }
        }

        private void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
        {
            orig(self);
            var teleporter = TeleporterInteraction.instance;
            if (teleporter && self == teleporter.bossGroup)
            {
                SayMountainShrineAmount();
            }
        }

        private void SayMountainShrineAmount()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
            {
                baseToken = "<style=cIsDamage>Amount: </style><style=cIsUtility>{0}x</style>",
                paramTokens = new string[] { $"{GetShrineCount()}" }
            });
        }
    }
}