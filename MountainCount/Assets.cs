//using R2API.Utils;
using RoR2;
using System.Runtime.CompilerServices;
using static MountainCount.Assets;

namespace MountainCount
{
    public class Assets
    {
        public abstract class ShrineReferenceBase
        {
            public abstract string SayCountToken { get; }
            public abstract string AppendToken { get; }
            public string SayCountExpandedToken => SayCountToken + "_EXPANDED";

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public abstract int GetCount();

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public abstract void GetCountExpanded(out object value1, out object value2, out object value3);

            public abstract void ModifyShrineUseToken(ref Chat.SubjectFormatChatMessage subjectFormatChatMessage);

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public virtual void AppendInfo()
            {
                var line = Language.GetStringFormatted(AppendToken, GetCount());
                MountainCountPlugin.Append(line);
            }

            public virtual void SayCount()
            {
                if (!TeleporterInteraction.instance)
                    return;
                if (Config.cfgExpandedInfo.Value)
                    SayCountExpanded();
                else
                    SayCountLimited();
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public virtual void SayCountLimited()
            {
                RoR2.Chat.SendBroadcastChat(new RoR2.Chat.SimpleChatMessage()
                {
                    baseToken = SayCountToken,
                    paramTokens = new string[] { GetCount().ToString() }
                });
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public abstract void SayCountExpanded();
        }

        public static int GetStackingLinear(int baseValue, int stackValue, int count)
        {
            if (count == 0) return 0;
            return baseValue + stackValue * (count - 1);
        }


        public class ShrineMountain : ShrineReferenceBase
        {
            public override string SayCountToken => "MOUNTAINCOUNT_SAYAMOUNT_MOUNTAIN";

            public override string AppendToken => "MOUNTAINCOUNT_SAYAMOUNT_MOUNTAIN_COMBINED";

            public override int GetCount()
            {
                if (TeleporterInteraction.instance)
                    return TeleporterInteraction.instance.shrineBonusStacks;
                return 0;
            }

            public override void GetCountExpanded(out object expectedLimitedValue, out object expandedValue1, out object expandedValue2)
            {
                expectedLimitedValue = GetCount();
                expandedValue1 = (int)expectedLimitedValue + 1;
                if ((int)expectedLimitedValue == 0)
                {
                    expandedValue1 = 0;
                }
                expandedValue2 = -1;
            }

            public override void ModifyShrineUseToken(ref Chat.SubjectFormatChatMessage subjectFormatChatMessage)
            {
                subjectFormatChatMessage.baseToken = "MOUNTAINCOUNT_SHRINE_BOSS_USE_MESSAGE";
                subjectFormatChatMessage.paramTokens = new string[] { GetCount().ToString() };
            }

            public override void SayCountExpanded()
            {
                GetCountExpanded(out object useCount, out object expandedCount, out object _);

                RoR2.Chat.SendBroadcastChat(new RoR2.Chat.SimpleChatMessage()
                {
                    baseToken = SayCountExpandedToken,
                    paramTokens = new string[] { useCount.ToString(), expandedCount.ToString() }
                });
            }
        }
    }
}