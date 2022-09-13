using BepInEx;

namespace NoBackButtonLobby
{
    [BepInPlugin("com.DestroyedClone.NoBackButtonLobby", "No Back Button Lobby", "1.0.1")]
    public class Class1 : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.UI.CharacterSelectController.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.transform.Find("SafeArea/FooterPanel/NakedButton (Quit)").gameObject.AddComponent<DisableOnStart>();
            };
        }
    }
}