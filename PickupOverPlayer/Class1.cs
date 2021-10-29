using System;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
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

namespace PickupOverPlayer
{
    [BepInPlugin("com.DestroyedClone.PickupOverPlayer", "PickupOverPlayer", "1.0.1")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI), nameof(NetworkingAPI))]
    public class Main : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.Chat.AddMessage_ChatMessageBase += Chat_AddMessage_ChatMessageBase;
        }

        private void Chat_AddMessage_ChatMessageBase(On.RoR2.Chat.orig_AddMessage_ChatMessageBase orig, ChatMessageBase message)
        {
            orig(message);
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
            textPrefab.AddComponent<HoverOverHeadSafe>();

            if (textPrefab) { PrefabAPI.RegisterNetworkPrefab(textPrefab); }
            R2API.EffectAPI.AddEffect(textPrefab);
            return textPrefab;
        }

        public void FixedUpdate()
        {

        }

        public class HoverOverHeadSafe : MonoBehaviour
        {
            private Transform parentTransform;
            private Collider bodyCollider;
            public Vector3 bonusOffset;

            private void Start()
            {
                if (!transform.parent)
                {
                    enabled = false;
                    return;
                }
                parentTransform = transform.parent;
                bodyCollider = transform.parent.GetComponent<Collider>();
            }

            private void Update()
            {
                Vector3 a = parentTransform.position;
                if (bodyCollider)
                {
                    a = bodyCollider.bounds.center + new Vector3(0f, bodyCollider.bounds.extents.y, 0f);
                }
                transform.position = a + bonusOffset;
            }
        }
    }
}
