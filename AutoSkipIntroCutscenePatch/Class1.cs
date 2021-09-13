using BepInEx;
using RoR2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace NS_AutoSkipIntroCutscene
{
    [BepInPlugin("com.Kingpinush.AutoSkipIntroCutscene", "AutoSkipIntroCutscene", "1.0.3")]
    public class AutoSkipIntroCutsceneMainPlugin : BaseUnityPlugin
    {
        public void Start()
        {

            var splashSkipConVar = (typeof(SplashScreenController).GetField("cvSplashSkip", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as BoolConVar);
            var introSkipConVar = (typeof(IntroCutsceneController).GetField("cvIntroSkip", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as BoolConVar);

            bool splashSkipOldValue = splashSkipConVar.value;
            bool introSkipOldValue = introSkipConVar.value;

            splashSkipConVar.SetBool(true);
            introSkipConVar.SetBool(true);


            On.RoR2.IntroCutsceneController.Finish += (orig, self) =>
            {
                splashSkipConVar.SetBool(splashSkipOldValue);
                introSkipConVar.SetBool(introSkipOldValue);

                orig(self);
            };
        }
    }
}
