using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using static MountainCount.Config;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MountainCount
{
    [BepInPlugin("com.DestroyedClone.MountainCount", "MountainCount", "1.1.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency("com.themysticsword.extrachallengeshrines", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class MountainCountPlugin : BaseUnityPlugin
    {
        public static Assets.ShrineMountain shrineMountain;

        public static StringBuilder ModStringBuilder = null;
        public static BepInEx.Logging.ManualLogSource _logger;

        public void Start()
        {
            shrineMountain = new Assets.ShrineMountain();
            ModStringBuilder = new StringBuilder();
            _logger = Logger;
            MountainCount.Config.Initialize(Config);

            if (!cfgPrintOnTeleporterEnd.Value.IsNullOrWhiteSpace())
                On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            if (cfgAddChatMessage.Value || cfgEditLanguage.Value)
                On.RoR2.Chat.SendBroadcastChat_ChatMessageBase += Chat_SendBroadcastChat_ChatMessageBase;

            Language.onCurrentLanguageChanged += AutoGenerateLanguageTokens;
            ModSupport.Initialize();
        }

        private void AutoGenerateLanguageTokens()
        {
            void Add(string originalToken, string appendingString = " (x{1})")
            {
                LanguageAPI.Add("MOUNTAINCOUNT_" + originalToken, Language.GetString(originalToken, Language.currentLanguage.name) + appendingString, Language.currentLanguage.name);
                LanguageAPI.Add("MOUNTAINCOUNT_" + originalToken + "_2P", Language.GetString(originalToken + "_2P", Language.currentLanguage.name) + appendingString, Language.currentLanguage.name);
            }
            Add("SHRINE_BOSS_USE_MESSAGE");
            Add("EXTRACHALLENGESHRINES_SHRINE_CROWN_USE_MESSAGE");
            Add("EXTRACHALLENGESHRINES_SHRINE_ROCK_USE_MESSAGE");
            Add("EXTRACHALLENGESHRINES_SHRINE_EYE_USE_MESSAGE", " (x{1}|{2})");
        }

        public static void Chat_SendBroadcastChat_ChatMessageBase(On.RoR2.Chat.orig_SendBroadcastChat_ChatMessageBase orig, ChatMessageBase message)
        {
            if (cfgEditLanguage.Value && message is Chat.SubjectFormatChatMessage subjectMsg)
            {
                switch (subjectMsg.baseToken)
                {
                    case "SHRINE_BOSS_USE_MESSAGE":
                        shrineMountain.ModifyShrineUseToken(ref subjectMsg);
                        break;

                    case "EXTRACHALLENGESHRINES_SHRINE_CROWN_USE_MESSAGE":
                        ModSupport.MC_ExtraChallengeShrines.shrineCrown.ModifyShrineUseToken(ref subjectMsg);
                        break;

                    case "EXTRACHALLENGESHRINES_SHRINE_ROCK_USE_MESSAGE":
                        ModSupport.MC_ExtraChallengeShrines.shrineRock.ModifyShrineUseToken(ref subjectMsg);
                        break;

                    case "EXTRACHALLENGESHRINES_SHRINE_EYE_USE_MESSAGE":
                        ModSupport.MC_ExtraChallengeShrines.shrineEye.ModifyShrineUseToken(ref subjectMsg);
                        break;
                }
            }

            // x? command
            bool isAskingMountain = false;
            bool isAskingSky = false;
            bool isAskingEarth = false;
            bool isAskingWind = false;
            bool isAskingAll = false;
            bool isAskingTotal = false;
            if (cfgAddChatMessage.Value && message is Chat.UserChatMessage chatMsg)
            {
                switch (chatMsg.text.ToLower())
                {
                    case "x?":
                    case "x ?":
                        //isAskingMountain = true;
                        //isAskingSky = true;
                        //isAskingEarth = true;
                        //isAskingWind = true;
                        if (ModSupport.modloaded_ExtraChallengeShrines)
                        {
                            isAskingAll = true;
                        } else
                        {
                            isAskingMountain = true;
                        }
                        break;

                    case "xm?":
                    case "xmtn?":
                    case "xГоры?":
                        isAskingMountain = true;
                        break;

                    case "xsky?": //crown
                    case "xНеба?": //no fuckin idea if this is right
                        isAskingSky = true;
                        break;

                    case "xearth?":
                    case "xЗемли?":
                        isAskingEarth = true;
                        break;

                    case "xwind?":
                    case "xВетра?":
                        isAskingWind = true;
                        break;

                    case "xtotal?":
                        isAskingTotal = true;
                        break;
                }
            }
            orig(message);

            if (Run.instance)
            {
                SetupStringBuilder();
                if (isAskingAll)
                {
                    shrineMountain.AppendInfo();
                    if (ModSupport.modloaded_ExtraChallengeShrines)
                    {
                        ModSupport.MC_ExtraChallengeShrines.shrineCrown?.AppendInfo();
                        ModSupport.MC_ExtraChallengeShrines.shrineEye?.AppendInfo();
                        ModSupport.MC_ExtraChallengeShrines.shrineRock?.AppendInfo();
                    }
                }
                else
                {
                    if (isAskingMountain)
                    {
                        shrineMountain.SayCount();
                    }
                    if (ModSupport.modloaded_ExtraChallengeShrines)
                    {
                        if (isAskingSky)
                        {
                            ModSupport.MC_ExtraChallengeShrines.shrineCrown.SayCount();
                        }
                        if (isAskingEarth)
                        {
                            ModSupport.MC_ExtraChallengeShrines.shrineRock.SayCount();
                        }
                        if (isAskingWind)
                        {
                            ModSupport.MC_ExtraChallengeShrines.shrineEye.SayCount();
                        }
                    }
                    if (isAskingTotal)
                    {
                        SetupStringBuilder();
                        var totalExpectedItemsPerPlayer = 1;
                        totalExpectedItemsPerPlayer += shrineMountain.GetCount();
                        if (ModSupport.modloaded_ExtraChallengeShrines)
                        {
                            ModSupport.MC_ExtraChallengeShrines.shrineCrown.GetCountExpanded(out object _, out object shrineCrownAddition, out object _);
                            var extraRedCount = (int)shrineCrownAddition;

                            totalExpectedItemsPerPlayer += extraRedCount;

                            ModSupport.MC_ExtraChallengeShrines.shrineRock.GetCountExpanded(out object _, out object shrineRockTotalCount, out object _);
                            totalExpectedItemsPerPlayer += (int)shrineRockTotalCount;
                        }
                        var output = Language.GetStringFormatted("MOUNTAINCOUNT_SAYAMOUNT_EXPECTEDPERPERSON", totalExpectedItemsPerPlayer);
                        Append(output);
                    }
                }
                SayStringBuilder();
            }
        }

        public static void SetupStringBuilder()
        {
            ModStringBuilder.Clear();
        }

        public static void Append(object line)
        {
            ModStringBuilder.Append("|" + line);
        }

        public static void SayStringBuilder()
        {
            var output = ModStringBuilder.ToString();
            if (!output.IsNullOrWhiteSpace())
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                {
                    baseToken = output
                });
            }
            //HG.StringBuilderPool.ReturnStringBuilder(ModStringBuilder);
            //ModStringBuilder = null;
        }

        public static void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
        {
            orig(self);
            var teleporter = TeleporterInteraction.instance;
            if (teleporter && self == teleporter.bossGroup)
            {
                //hacky shit
                var chatMessage = new Chat.UserChatMessage()
                {
                    text = MountainCount.Config.cfgPrintOnTeleporterEnd.Value
                };
                Chat.SendBroadcastChat(chatMessage);

                /*
                SayAmount_MountainShrine();
                if (ModSupport.modloaded_ExtraChallengeShrines)
                {
                    ModSupport.MC_ExtraChallengeShrines.Append_Crown();
                    ModSupport.MC_ExtraChallengeShrines.SayAmount_Rock();
                    ModSupport.MC_ExtraChallengeShrines.SayAmount_Eye();
                }*/
            }
        }
    }
}