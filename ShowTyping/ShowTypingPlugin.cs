﻿using BepInEx;
using R2API;
using R2API.Utils;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ShowTyping
{
    [BepInPlugin("com.DestroyedClone.ShowTyping", "Show Typing", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI), nameof(NetworkingAPI))]
    public class ShowTypingPlugin : BaseUnityPlugin
    {
        public static GameObject typingText;
        public static GameObject unfocusedText;

        public void Awake()
        {
            //UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            On.RoR2.UI.ChatBox.Start += ChatBox_Start;
            On.RoR2.UI.ChatBox.FocusInputField += ChatBox_FocusInputField;
            On.RoR2.UI.ChatBox.UnfocusInputField += ChatBox_UnfocusInputField;
            NetworkingAPI.RegisterMessageType<Networking.TypingTextMessage>();
            NetworkingAPI.RegisterMessageType<Networking.UnfocusedTextMessage>();

            typingText = CreateTextPrefab(
                "TYPING...",
                "TypingText",
                "",
                2f);
            unfocusedText = CreateTextPrefab(
                "TABBED OUT",
                "UnfocusedText",
                "",
                2f
                ); ;
        }
        private void ChatBox_FocusInputField(On.RoR2.UI.ChatBox.orig_FocusInputField orig, ChatBox self)
        {
            orig(self);
            var comp = self.gameObject.GetComponent<InGameChattingIndicator>();
            if (comp)
            {
                comp.isChatting = true;
            }
        }

        private void ChatBox_UnfocusInputField(On.RoR2.UI.ChatBox.orig_UnfocusInputField orig, ChatBox self)
        {
            orig(self);
            var comp = self.gameObject.GetComponent<InGameChattingIndicator>();
            if (comp)
            {
                comp.isChatting = false;
            }
        }


        private void ChatBox_Start(On.RoR2.UI.ChatBox.orig_Start orig, ChatBox self)
        {
            orig(self);
            if (!PlayerCharacterMasterController.instances[0].body)
                return;
            if (!self.gameObject.GetComponent<InGameChattingIndicator>())
                self.gameObject.AddComponent<InGameChattingIndicator>();
        }

        public static GameObject CreateTextPrefab(string text, string prefabName, string soundName = "", float fontSize = 1f)
        {
            var textPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/BearProc"), prefabName);
            textPrefab.name = prefabName;
            UnityEngine.Object.Destroy(textPrefab.transform.Find("Fluff").gameObject);
            textPrefab.GetComponent<EffectComponent>().soundName = soundName;
            var tmp = textPrefab.transform.Find("TextCamScaler/TextRiser/TextMeshPro").GetComponent<TextMeshPro>();
            var ltmc = tmp.gameObject.GetComponent<LanguageTextMeshController>();
            ltmc.token = text;
            tmp.text = text;
            tmp.fontSize = fontSize;
            textPrefab.AddComponent<NetworkIdentity>();
            //textPrefab.AddComponent<HoverOverHead>().bonusOffset = new Vector3(0,1,0);
            UnityEngine.Object.Destroy(textPrefab.GetComponent<EffectComponent>());

            if (textPrefab) { PrefabAPI.RegisterNetworkPrefab(textPrefab); }
            return textPrefab;
        }


        public class InGameChattingIndicator : MonoBehaviour
        {
            PlayerCharacterMasterController localPlayer;

            GameObject typingIndicatorInstance;
            public bool isChatting = false;

            GameObject unfocusedIndicatorInstance;
            bool windowUnfocused = false;

            float stopwatch;
            readonly float duration = 1.5f;

            public void Awake()
            {
                localPlayer = PlayerCharacterMasterController.instances[0];
            }

            public void Update()
            {
                if (!localPlayer.body)
                    return;
                stopwatch += Time.deltaTime;

                NetworkIdentity identity = localPlayer.body.gameObject.GetComponent<NetworkIdentity>();
                if (!identity)
                {
                    Debug.LogWarning("PlaySoundInServer: The body did not have a NetworkIdentity component!");
                    return;
                }

                if (stopwatch >= duration)
                {
                    stopwatch = 0f;
                    if (typingIndicatorInstance != isChatting)
                    {
                        if (isChatting)
                        {
                            //typingIndicatorInstance = Instantiate(typingText, localPlayer.body.transform);
                            new Networking.TypingTextMessage(identity.netId).Send(NetworkDestination.Server);
                            return;
                        }
                        UnityEngine.Object.Destroy(typingIndicatorInstance);
                        typingIndicatorInstance = null;
                    }
                    if (unfocusedIndicatorInstance != windowUnfocused)
                    {
                        if (windowUnfocused)
                        {
                            //unfocusedIndicatorInstance = Instantiate(unfocusedText, localPlayer.body.transform);
                            new Networking.UnfocusedTextMessage(identity.netId).Send(NetworkDestination.Server);
                            return;
                        }
                        UnityEngine.Object.Destroy(unfocusedIndicatorInstance);
                        unfocusedIndicatorInstance = null;
                    }
                }
            }

            private void OnApplicationFocus(bool hasFocus) { windowUnfocused = !hasFocus; }
        }

        #region InLobby
        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene sceneName, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (sceneName.name == "lobby")
            {
                LobbyShit();
            }
        }

        private void LobbyShit()
        {
            var localGameObject = new GameObject();

            localGameObject.AddComponent<ShowChattingInLobby>();
        }

        private class ShowChattingInLobby : MonoBehaviour
        {
            bool isChatting = false;
            GameObject gameObjectToWatch;
            GameObject showTypingTextObject;
            GameObject section;
            
            public void Start()
            {
                gameObjectToWatch = GameObject.Find("CharacterSelectUI/SafeArea/ChatboxPanel/Chatbox (Clone)/ExpandedRect");


                section = new GameObject();
                section.transform.SetParent(GameObject.Find("CharacterSelectUI/SafeArea/").transform);
                section.name = "TopPanel";
                section.transform.position = new Vector3(0, 400, 0);


                var textObj = GameObject.Find("CharacterSelectUI/SafeArea/LeftHandPanel (Layer: Main)/SurvivorInfoPanel, Active (Layer: Secondary)/SurvivorNamePanel");
                showTypingTextObject.transform.SetParent(section.transform);
            }

            public void Update()
            {
                isChatting = gameObjectToWatch.transform.childCount > 0;
                if (isChatting)
                {

                }
            }
        }
        #endregion
    }
}
