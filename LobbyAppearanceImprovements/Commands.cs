using RoR2;
using UnityEngine;

namespace LobbyAppearanceImprovements
{
    public static class Commands
    {

        [ConCommand(commandName = "changelight", flags = ConVarFlags.ExecuteOnServer, helpText = "changelight {r} {g} {b} {a} | only works in the lobby")]
        public static void ChangeLight(ConCommandArgs args)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
                Helpers.ChangeLobbyLightColor(new Color32((byte)args.GetArgInt(0), (byte)args.GetArgInt(1), (byte)args.GetArgInt(2), (byte)args.GetArgInt(3)));
        }

        [ConCommand(commandName = "LAI_BringToLobby", flags = ConVarFlags.ExecuteOnServer, helpText = "LAI_BringToLobby - For developers. See the README.")]
        public static void SetupLobby(ConCommandArgs args)
        {
            var melon = args.senderMasterObject.AddComponent<LAIIntializer>();
            melon.sender = args.sender;
            RoR2.Console.instance.SubmitCmd(args.sender, "set_scene lobby", true);
        }
        public class LAIIntializer : MonoBehaviour
        {
            public NetworkUser sender;
            bool hasTeleported = false;
            public GameObject MainCamera;
            public GameObject CharacterSelectUI;
            public GameObject CharacterPadAlignments;
            bool PrintCurrentPosition = false;

            public void FixedUpdate()
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby"
                    && PlayerCharacterMasterController.instances[0]?.master?.gameObject.GetComponent<LAIIntializer>())
                {
                    if (!hasTeleported)
                    {
                        if (sender.GetCurrentBody())
                        {
                            RoR2.Console.instance.SubmitCmd(sender, "setpos 1 1 11", true);

                            hasTeleported = true;
                        }
                    }
                    if (!MainCamera)
                    {
                        MainCamera = GameObject.Find("Main Camera").gameObject;
                    }
                    if (!CharacterSelectUI)
                    {
                        CharacterSelectUI = GameObject.Find("CharacterSelectUI").gameObject;
                    }
                    if (!CharacterPadAlignments)
                    {
                        CharacterPadAlignments = GameObject.Find("CharacterPadAlignments").gameObject;
                    }

                    // Pseudo-Commands
                    if (PrintCurrentPosition)
                    {
                        RoR2.Console.instance.SubmitCmd(sender, "getpos", true);

                        PrintCurrentPosition = false;
                    }
                }
            }
        }
    }
}
