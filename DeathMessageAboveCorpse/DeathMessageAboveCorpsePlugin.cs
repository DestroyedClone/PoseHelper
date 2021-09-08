using BepInEx;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using R2API;
using RoR2;
using TMPro;
using RoR2.UI;
using R2API.Networking;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace DeathMessageAboveCorpse
{
    [BepInPlugin("com.DestroyedClone.DeathMessageAboveCorpse", "Death Message Above Corpse", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI), nameof(NetworkingAPI))]
    public class DeathMessageAboveCorpsePlugin : BaseUnityPlugin
    {
        public static string[] deathMessages = new string[]
        {
            "shak pls.",
            "You are dead.",
            "You embrace the void.",
            "You had a lot more to live for.",
            "Your internal organs have failed.",
            "Your body was gone an hour later.",
            "Your family will never know how you died.",
            "You died painlessly.",
            "Your death was extremely painful.",
            "You have broken every bone in your body.",
            "You die a slightly embarrassing death.",
            "You die in a hilarious pose.",
            "You really messed up.",
            "You have died. Maybe next time..",
            "You have passed away. Try again?",
            "Choose a new character?",
            "Remember to activate use items.",
            "Remember that as time increases, so does difficulty.",
            "This planet has killed you.",
            "It wasn't your time to die...",
            "That was definitely not your fault.",
            "That was absolutely your fault.",
            "They will surely feast on your flesh.",
            "..the harder they fall.",
            "Beep.. beep.. beeeeeeeeeeeeeeeee",
            "Close!",
            "Come back soon!",
            "Crushed.",
            "Smashed.",
            "DEAD",
            "Get styled upon.",
            "Dead from blunt trauma to the face.",
            "ded",
            "rekt",
            "ur dead LOL get rekt",
            "Sucks to suck.",
            "You walk towards the light.",

            // TODO: Seperate based on difficulty (Run.instance.selectedDifficulty)
            "Try playing on \"Drizzle\" mode for an easier time.",
            "Consider lowering the difficulty.",
        };

        public static GameObject defaultTextObject;

        // Text displays larger for the client in the middle of the screen (https://youtu.be/vQRPpSx5WLA?t=1336)
        // 3 second delay after the corpse is on the ground before showing either client or server message
        //

        public void Awake()
        {
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += GlobalEventManager_OnPlayerCharacterDeath;
            defaultTextObject = CreateDefaultTextObject();
        }

        private void GlobalEventManager_OnPlayerCharacterDeath(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport, RoR2.NetworkUser victimNetworkUser)
        {
            orig(self, damageReport, victimNetworkUser);
            var deathMessage = GetDeathMessage();

            var textObject = Object.Instantiate<GameObject>(defaultTextObject);
            var tmp = textObject.transform.Find("TextCamScaler/TextRiser/TextMeshPro").GetComponent<TextMeshPro>();
            var ltmc = tmp.gameObject.GetComponent<LanguageTextMeshController>();
            ltmc.token = deathMessage;
            tmp.text = deathMessage;
            tmp.fontSize = 2f;
            textObject.transform.position = damageReport.victimBody.corePosition + Vector3.up * 2f;
            NetworkServer.Spawn(textObject);
        }

        public string GetDeathMessage()
        {
            return deathMessages[Random.Range(0, deathMessages.Length)];
        }

        public static GameObject CreateDefaultTextObject()
        {
            var textPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/BearProc"), "DeathMessageAboveCorpse_DefaultTextObject");
            textPrefab.name = "DeathMessageAboveCorpse_DefaultTextObject";
            UnityEngine.Object.Destroy(textPrefab.transform.Find("Fluff").gameObject);
            UnityEngine.Object.Destroy(textPrefab.GetComponent<EffectComponent>());
            //var tmp = textPrefab.transform.Find("TextCamScaler/TextRiser/TextMeshPro").GetComponent<TextMeshPro>();
            textPrefab.GetComponent<DestroyOnTimer>().duration = 15f;
            textPrefab.transform.Find("TextCamScaler/TextRiser").GetComponent<ObjectScaleCurve>().timeMax = 20f;
            textPrefab.AddComponent<NetworkIdentity>();

            if (textPrefab) { PrefabAPI.RegisterNetworkPrefab(textPrefab); }
            return textPrefab;
        }
    }
}