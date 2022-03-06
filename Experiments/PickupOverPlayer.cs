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
        public static GameObject chatObject;

        public void Start()
        {
            On.RoR2.Chat.AddMessage_ChatMessageBase += Chat_AddMessage_ChatMessageBase;
            On.RoR2.Chat.AddPickupMessage += Chat_AddPickupMessage;

            chatObject = CreateTextPrefab("", "ChatPickupPrefab", "", 1);
            UnityEngine.Object.Destroy(chatObject.GetComponent<EffectComponent>());
        }

        private void Chat_AddPickupMessage(On.RoR2.Chat.orig_AddPickupMessage orig, CharacterBody body, string pickupToken, Color32 pickupColor, uint pickupQuantity)
        {
            On.RoR2.Skills.SkillDef.CanExecute += SkillDef_CanExecute;

            var message = new Chat.PlayerPickupChatMessage
            {
                subjectAsCharacterBody = body,
                baseToken = "PLAYER_PICKUP",
                pickupToken = pickupToken,
                pickupColor = pickupColor,
                pickupQuantity = pickupQuantity
            };
            var newText = message.ConstructChatString();
            var obj = UnityEngine.Object.Instantiate(chatObject);
            obj.transform.parent = body.transform;
            obj.GetComponent<TextTracker>().UpdateText(newText);
        }

        private bool SkillDef_CanExecute(On.RoR2.Skills.SkillDef.orig_CanExecute orig, RoR2.Skills.SkillDef self, GenericSkill skillSlot)
        {
            BuffDef whiteFlag = new BuffDef();
            if (skillSlot.characterBody && !skillSlot.characterBody.HasBuff(whiteFlag))
            {
                return orig(self, skillSlot);
            }
            return false;
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

            var tt = textPrefab.AddComponent<TextTracker>();
            tt.tmp = tmp;
            tt.ltmc = ltmc;

            if (textPrefab) { PrefabAPI.RegisterNetworkPrefab(textPrefab); }
            R2API.EffectAPI.AddEffect(textPrefab);
            return textPrefab;
        }

        public class TextTracker : MonoBehaviour
        {
            public TextMeshPro tmp;
            public LanguageTextMeshController ltmc;

            public void UpdateText(string text)
            {
                tmp.text = text;
                ltmc.token = text;
            }
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
