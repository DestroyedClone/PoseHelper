namespace MountainCount
{
    public static partial class ModSupport
    {
        public static bool IsAnyModLoaded = false;
        public static bool modloaded_ExtraChallengeShrines = false;
        public static bool modloaded_RiskOfOptions = false;

        public static void Initialize()
        {
            static bool IsModLoaded(string key)
            {
                var result = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(key);
                IsAnyModLoaded = IsAnyModLoaded || result;
                return result;
            }
            modloaded_ExtraChallengeShrines = IsModLoaded("com.themysticsword.extrachallengeshrines");
            modloaded_RiskOfOptions = IsModLoaded("com.rune580.riskofoptions");

            if (modloaded_ExtraChallengeShrines)
                MC_ExtraChallengeShrines.Initialize();
            if (modloaded_RiskOfOptions)
                MC_RiskOfOptions.Initialize();
        }
    }
}