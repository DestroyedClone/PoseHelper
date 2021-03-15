using RoR2;
using UnityEngine;

namespace LobbyAppearanceImprovements
{
    public static class Commands
    {

        [ConCommand(commandName = "LAI_ChangeLobbyColor", flags = ConVarFlags.ExecuteOnServer, helpText = "changelight {r} {g} {b} {a} | For previewing, does not save.")]
        public static void ChangeLight(ConCommandArgs args)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
            {
                Methods.ChangeLobbyLightColor(new Color32((byte)args.GetArgInt(0), (byte)args.GetArgInt(1), (byte)args.GetArgInt(2), (byte)args.GetArgInt(3)));
            }
        }

        [ConCommand(commandName = "LAI_BringToLobby", flags = ConVarFlags.ExecuteOnServer, helpText = "LAI_BringToLobby - DestroyedClone's tool for helping add new values")]
        public static void SetupLobby(ConCommandArgs args)
        {
            var melon = args.senderMasterObject.AddComponent<LAIIntializer>();
            melon.sender = args.sender;
            RoR2.Console.instance.SubmitCmd(args.sender, "set_scene lobby", true);
        }

        [ConCommand(commandName = "LAI_DioramaTest", flags = ConVarFlags.ExecuteOnServer, helpText = "stagename x y z")]
        public static void Diorama(ConCommandArgs args)
        {
            var path = "prefabs/stagedisplay/"+args.GetArgString(0)+"DioramaDisplay";
            var gay = Resources.Load(path);
            var diorama = (GameObject)UnityEngine.Object.Instantiate(gay);
            diorama.transform.position = new Vector3(args.GetArgFloat(1), args.GetArgFloat(2), args.GetArgFloat(3));
        }
        public class LAIIntializer : MonoBehaviour
        {
            public NetworkUser sender;
            bool hasTeleported = false;
            private GameObject MainCamera;
            public bool enableMainCamera = false;
            private GameObject CharacterSelectUI;
            public bool enableCharacterSelectUI = false;
            private GameObject CharacterPadAlignments;
            public bool enableCharacterPadAlignments = false;
            bool PrintCurrentPosition = false;

            public void Awake()
            {
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
            }

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
                    if (MainCamera) MainCamera.SetActive(enableMainCamera);
                    if (CharacterPadAlignments)  CharacterPadAlignments.SetActive(enableCharacterPadAlignments);
                    if (CharacterSelectUI) CharacterSelectUI.SetActive(enableCharacterSelectUI);

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
