﻿using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace DynamicOverheadText
{
    [BepInPlugin("com.DestroyedClone.OverheadText", "Overhead Text", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI))]
    public class Class1 : BaseUnityPlugin
    {
        public static GameObject critText;
        public static GameObject overCritText;

        public static bool hypercritIsLoaded = false;

        public void Awake()
        {
            Hooks();
            CreateTextPrefabs();
            hypercritIsLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.Hypercrit");
        }

        public void CreateTextPrefabs()
        {
            critText = CreateTextPrefab(
                "<color=#69221a><b>Critical" +
                "\nHit!</b></color>",
                "CriticalHitText",
                "",
                2f);
            if (hypercritIsLoaded)
            {
                overCritText = CreateTextPrefab(
                    "<color=#3f140f><b>Hypercrit!</b></color>",
                    "HyperCritHitText",
                    "",
                    2f);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void ModCompatMethodTemplate(GameObject gameObject)
        {

        }

        public void CreateCritTextPrefab()
        {
            critText = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/BearProc"), "CriticalHitText");
            critText.name = "CriticalHitText";
            var tmp = critText.transform.Find("TextCamScaler/TextRiser/TextMeshPro").GetComponent<TextMeshPro>();
            var ltmc = tmp.gameObject.GetComponent<LanguageTextMeshController>();
            ltmc.token = "<color=red>Critical Hit!</color>";
            tmp.text = "<color=red>Critical Hit!</color>";
            tmp.fontSize = 2f;
            critText.AddComponent<NetworkIdentity>();

            if (critText) { PrefabAPI.RegisterNetworkPrefab(critText); }
            R2API.EffectAPI.AddEffect(critText);
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

        public void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += ShowDamageRelatedHits;
        }

        public void SpawnEffectNoParent(GameObject effectPrefab, Vector3 position)
        {
            EffectData effectData = new EffectData
            {
                origin = position
            };
            EffectManager.SpawnEffect(effectPrefab, effectData, true);
        }

        private void ShowDamageRelatedHits(DamageReport obj)
        {
            if (obj.damageInfo.crit)
            {
                //SpawnEffect(critText, obj.damageInfo.position);
                EffectData effectData = new EffectData
                {
                    origin = obj.damageInfo.position,
                    rootObject = obj.victim.gameObject
                };
                EffectManager.SpawnEffect(critText, effectData, true);
            }
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