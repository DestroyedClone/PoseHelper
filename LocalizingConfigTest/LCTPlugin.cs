using BepInEx;
using RoR2;
using Zio;
using Zio.FileSystems;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LocalizingConfigTest
{
    [BepInPlugin("com.DestroyedClone.LocalizeConfig", "Localize Config", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class LCTPlugin : BaseUnityPlugin
    {
        public static string cfgCurrentLanguage;

        public static float cfgBaseAmount;
        public static float cfgStackAmount;
        public static string cfgCurrentLang;

        public static PluginInfo pluginInfo;
        public static FileSystem fileSystem { get; private set; }

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            pluginInfo = Info;

            FunnyLanguage();
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
        }

        private void Language_onCurrentLanguageChanged()
        {
            SetupConfig();
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);

            SetupConfig();

        }

        public void SetupConfig()
        {
            cfgCurrentLanguage = Config.Bind("LANG", GetToken("CFG_LanguageString", Language.currentLanguageName), Language.currentLanguageName).Value;
            if (cfgCurrentLanguage != Language.currentLanguageName)
            {
                _logger.LogMessage($"{cfgCurrentLanguage} does not match {Language.currentLanguageName}");
                ResetConfig();
            }

            var categoryName = GetToken("CFG_Category1");
            cfgBaseAmount = Config.Bind(categoryName, GetToken("CFG_BaseAmount"), 1f, GetToken("CFG_BaseAmountDescription")).Value;
            cfgStackAmount = Config.Bind(categoryName, GetToken("CFG_StackAmount"), 0.25f, GetToken("CFG_StackAmountDescription")).Value;
        }

        private void ResetConfig()
        {
            cfgCurrentLanguage = Language.currentLanguageName;

            PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();
            fileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(assemblyDir), true);
            if (fileSystem.DirectoryExists("./config/")) //Uh, it exists and we make sure to not shit up R2Api
            {
                if (fileSystem.FileExists("./config/com.DestroyedClone.LocalizeConfig.cfg"))
                {
                    _logger.LogMessage("Localizeconfig found");
                    fileSystem.DeleteFile("./config/com.DestroyedClone.LocalizeConfig.cfg");
                    SetupConfig();
                }
            }
            //cfgCurrentLanguage = Config.Bind(GetToken("CFG_CategoryLanguage"), GetToken("CFG_LanguageString", Language.currentLanguageName), "").Value;
        }

        private void FunnyLanguage()
        {
            PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();
            fileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(assemblyDir), true);
            if (fileSystem.DirectoryExists("/Language/")) //Uh, it exists and we make sure to not shit up R2Api
            {
                Language.collectLanguageRootFolders += delegate (List<DirectoryEntry> list)
                {
                    list.Add(fileSystem.GetDirectoryEntry("/Language/"));
                };
            }
        }

        internal static string assemblyDir
        {
            get
            {
                return System.IO.Path.GetDirectoryName(pluginInfo.Location);
            }
        }

        public static string GetToken(string token)
        {
            return Language.GetString(token);
        }

        public static string GetToken(string token, params object[] args)
        {
            return Language.GetStringFormatted(token, args);
        }
    }
}