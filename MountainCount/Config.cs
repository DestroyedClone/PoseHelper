using BepInEx;
using BepInEx.Configuration;
using System;

namespace MountainCount
{
    public static class Config
    {
        public static ConfigEntry<string> cfgPrintOnTeleporterEnd;
        public static ConfigEntry<bool> cfgEditLanguage;
        public static ConfigEntry<bool> cfgAddChatMessage;
        public static ConfigEntry<bool> cfgExpandedInfo;

        public static void Initialize(ConfigFile Config)
        {
            cfgPrintOnTeleporterEnd = Config.Bind("", "Message On Teleporter Completion", "x?", "If true, then this message will be sent to the server. Recommended usage is x? or xtotal?");
            cfgEditLanguage = Config.Bind("", "Edit Language", true, "Modifies boss shrine usage text to show the current amount.");
            cfgAddChatMessage = Config.Bind("", "Add Command", true, "If true, adds a chat message." +
                "\n'x?' will show the amount.");
            cfgExpandedInfo = Config.Bind("", "Expanded Information", false, "If true, then the query messages will be changed based on the info needed." +
                "\nExample: Mountain info will also show the amount of items each person gets.");

            cfgPrintOnTeleporterEnd.SettingChanged += CfgPrintOnTeleporterEnd_SettingChanged;
            cfgAddChatMessage.SettingChanged += CfgAddChatMessage_SettingChanged;
            cfgAddChatMessage.SettingChanged += CfgAddChatMessage_SettingChanged;
        }

        public static void CfgAddChatMessage_SettingChanged(object sender, EventArgs e)
        {
            On.RoR2.Chat.SendBroadcastChat_ChatMessageBase -= MountainCountPlugin.Chat_SendBroadcastChat_ChatMessageBase;
            if (cfgAddChatMessage.Value || cfgEditLanguage.Value)
                On.RoR2.Chat.SendBroadcastChat_ChatMessageBase += MountainCountPlugin.Chat_SendBroadcastChat_ChatMessageBase;
        }

        public static void CfgPrintOnTeleporterEnd_SettingChanged(object sender, EventArgs e)
        {
            On.RoR2.BossGroup.DropRewards -= MountainCountPlugin.BossGroup_DropRewards;
            if (!cfgPrintOnTeleporterEnd.Value.IsNullOrWhiteSpace())
                On.RoR2.BossGroup.DropRewards += MountainCountPlugin.BossGroup_DropRewards;
        }
    }
}