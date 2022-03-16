using BepInEx;
using RoR2;
using RoR2.Skills;
using System;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using System.Collections.Generic;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace AutofireSkills
{
    [BepInPlugin("com.DestroyedClone.Autofire", "Autofire", "1.0.0")]
    public class AutofirePlugin : BaseUnityPlugin
    {
        public static AutofirePlugin instance;

        public static string[] skillDefs = {
                "CommandoBody/CommandoBodyFireFMJ",
                "CommandoBody/CommandoBodyBarrage",
                "CommandoBody/ThrowGrenade",

                "CrocoBody/CrocoSpit",
                "CrocoBody/CrocoDisease",

                "HuntressBody/FireArrowSnipe",

                "MageBody/MageBodyNovaBomb",
                "MageBody/MageBodyIceBomb",

                "LoaderBody/GroundSlam",

                "MercBody/MercBodyEvis",
                "MercBody/MercBodyEvisProjectile",

                //"ToolbotBody/ToolbotBodyStunDrone", doesnt change anything?

                "TreebotBody/TreebotBodyAimMortar2",
                "TreebotBody/TreebotBodyAimMortarRain",

                "RailgunnerBody/RailgunnerBodyFireSnipeHeavy",
                "RailgunnerBody/RailgunnerBodyFireMineBlinding",
            };

        public static string[] issueDefs =
        {
            "MercBody/MercBodyEvisProjectile",
            "TreebotBody/TreebotBodyAimMortar2",
            "TreebotBody/TreebotBodyAimMortarRain",
            "CommandoBody/CommandoBodyBarrage"
        };

        public void Awake()
        {
            instance = this;
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start1;
        }

        private void MainMenuController_Start1(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);

            //Dictionary<string, string> shorthand_to_Name = new Dictionary<string, string>();
            foreach (var skillDef in skillDefs)
            {
                //instance.Logger.LogMessage($"Loading SkillDefs/{skillDef}");
                var loadedDef = LegacyResourcesAPI.Load<SkillDef>($"SkillDefs/{skillDef}");
                var slashIndex = skillDef.IndexOf('/');
                var nameToken = skillDef.Substring(0, slashIndex-4).ToUpper()+"_BODY_NAME";
                var category = Language.GetString(nameToken);
                if (!loadedDef)
                {
                    //instance.Logger.LogMessage($"This is what's left: {slashIndex} {category}");
                    continue;
                }
                var skillName = Language.GetString(loadedDef.skillNameToken);
                var defaultDesc = "mustKeyPress";
                if (issueDefs.Contains(skillDef))
                {
                    defaultDesc += ". May cause irregular outcomes.";
                }
                var tempConfig = instance.Config.Bind(category, skillName, loadedDef.mustKeyPress, defaultDesc);

                loadedDef.mustKeyPress = tempConfig.Value;
            }

            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start1;
        }

        public static void MainMenuController_Start()
        {
            //orig(self);

        }

        /*[RoR2.SystemInitializer(dependencies: new Type[] {
            typeof(BodyCatalog),
            typeof(Language),
            typeof(LegacyResourcesAPI)
        })]*/
    }
}