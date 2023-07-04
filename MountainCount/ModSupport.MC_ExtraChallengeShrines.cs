using ExtraChallengeShrines;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using static MountainCount.Assets;

namespace MountainCount
{
    public static partial class ModSupport
    {
        public static class MC_ExtraChallengeShrines
        {
            /* public - external
             * Crown = Sky
             * Rock = Earth
             * Eye = Wind
             */
            public static AssetBundle ecs_AssetBundle;
            public static ShrineCrown shrineCrown;
            public static ShrineRock shrineRock;
            public static ShrineEye shrineEye;

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void Initialize()
            {
                ecs_AssetBundle = ExtraChallengeShrines.ExtraChallengeShrinesPlugin.AssetBundle;
                shrineCrown = new ShrineCrown();
                shrineRock = new ShrineRock();
                shrineEye = new ShrineEye();
            }

            public abstract class ExtraChallengeShrinesShrineReferenceBase : ShrineReferenceBase
            {
                public ExtraChallengeShrinesTeleporterComponent TeleporterComponent
                {
                    get
                    {
                        if (!_teleporterComponent)
                        {
                            if (TeleporterInteraction.instance && TeleporterInteraction.instance.TryGetComponent(out ExtraChallengeShrines.ExtraChallengeShrinesTeleporterComponent tpComponent))
                            {
                                _teleporterComponent = tpComponent;
                            }
                        }

                        return _teleporterComponent;
                    }
                    set
                    {
                        _teleporterComponent = value;
                        // Add any additional logic that needs to be executed when the value is set
                    }
                }

                private ExtraChallengeShrinesTeleporterComponent _teleporterComponent;
            }

            public class ShrineCrown : ExtraChallengeShrinesShrineReferenceBase
            {
                public override string SayCountToken => "MOUNTAINCOUNT_EXTRACHALLENGESHRINES_SAYAMOUNT_CROWN";

                public override string AppendToken => "MOUNTAINCOUNT_EXTRACHALLENGESHRINES_SAYAMOUNT_CROWN_COMBINED";

                public override int GetCount()
                {
                    if (TeleporterComponent)
                        return TeleporterComponent.crownShrineStacks;
                    return 0;
                }

                public override void GetCountExpanded(out object getCount, out object total, out object _)
                {
                    var baseValue = ExtraChallengeShrines.Interactables.ShrineCrown.redDrops.Value;
                    var stackValue = ExtraChallengeShrines.Interactables.ShrineCrown.redDropsPerStack.Value;
                    getCount = GetCount();
                    total = GetStackingLinear(baseValue, stackValue, (int)getCount);
                    _ = null;
                }

                public override void SayCountExpanded()
                {
                    GetCountExpanded(out object useCount, out object totalValue, out object _);
                    RoR2.Chat.SendBroadcastChat(new RoR2.Chat.SimpleChatMessage()
                    {
                        baseToken = SayCountExpandedToken,
                        paramTokens = new string[] { useCount.ToString(), totalValue.ToString() }
                    });
                }
            }

            public class ShrineRock : ExtraChallengeShrinesShrineReferenceBase
            {
                public override string SayCountToken => "MOUNTAINCOUNT_EXTRACHALLENGESHRINES_SAYAMOUNT_ROCK";

                public override string AppendToken => "MOUNTAINCOUNT_EXTRACHALLENGESHRINES_SAYAMOUNT_ROCK_COMBINED";

                public override int GetCount()
                {
                    if (TeleporterComponent)
                        return TeleporterComponent.rockShrineStacks;
                    return -1;
                }

                public override void GetCountExpanded(out object useCount, out object totalValue, out object _)
                {
                    useCount = GetCount();
                    var baseValue = ExtraChallengeShrines.Interactables.ShrineRock.extraDrops.Value;
                    var stackValue = ExtraChallengeShrines.Interactables.ShrineRock.extraDropsPerStack.Value;
                    totalValue = Assets.GetStackingLinear(baseValue, stackValue, (int)useCount);
                    _ = null;
                }

                public override void SayCountExpanded()
                {
                    GetCountExpanded(out object useCount, out object totalValue, out object _);
                    RoR2.Chat.SendBroadcastChat(new RoR2.Chat.SimpleChatMessage()
                    {
                        baseToken = SayCountExpandedToken,
                        paramTokens = new string[] { useCount.ToString(), totalValue.ToString() }
                    });
                }
            }

            public class ShrineEye : ExtraChallengeShrinesShrineReferenceBase
            {
                public override string SayCountToken => "MOUNTAINCOUNT_EXTRACHALLENGESHRINES_SAYAMOUNT_EYE";

                public override string AppendToken => "MOUNTAINCOUNT_EXTRACHALLENGESHRINES_SAYAMOUNT_EYE_COMBINED";

                public override int GetCount()
                {
                    if (TeleporterComponent)
                        return TeleporterComponent.eyeShrineStacks;
                    return -1;
                }

                public override void GetCountExpanded(out object chanceString, out object _, out object __)
                {
                    var bossDropChance = TeleporterComponent.bossGroup.bossDropChance;
                    chanceString = ((int)bossDropChance).ToString("##.0%");
                    _ = null;
                    __ = null;
                }

                public string GetSelectedBossName()
                {
                    if (TeleporterComponent
                        && TeleporterComponent.eyeSelectedBody != BodyIndex.None)
                    {
                        var characterBody = BodyCatalog.GetBodyPrefabBodyComponent(TeleporterComponent.eyeSelectedBody);
                        if (!characterBody)
                            return string.Empty;
                        return Language.GetString(characterBody.baseNameToken);
                    }
                    return string.Empty;
                }

                public override void AppendInfo()
                {
                    var line = Language.GetStringFormatted(AppendToken, GetCount(), GetSelectedBossName());
                    MountainCountPlugin.Append(line);
                }

                public override void SayCountLimited()
                {
                    RoR2.Chat.SendBroadcastChat(new RoR2.Chat.SimpleChatMessage()
                    {
                        baseToken = SayCountToken,
                        paramTokens = new string[] { GetCount().ToString(), GetSelectedBossName() }
                    });
                }

                public override void SayCountExpanded()
                {
                    GetCountExpanded(out object chanceString, out object _, out object _);
                    RoR2.Chat.SendBroadcastChat(new RoR2.Chat.SimpleChatMessage()
                    {
                        baseToken = SayCountExpandedToken,
                        paramTokens = new string[] { GetCount().ToString(), GetSelectedBossName(), chanceString.ToString() }
                    });
                }
            }
        }
    }
}