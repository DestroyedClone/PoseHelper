//using R2API.Utils;
using RoR2;
using System.Runtime.CompilerServices;

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
    }
}