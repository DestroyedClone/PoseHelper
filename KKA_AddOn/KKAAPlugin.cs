using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using BepInEx.Configuration;
using System.Collections.Generic;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace KKA_AddOn
{
    [BepInPlugin("com.DestroyedClone.KKA_AddOn", "KingKombatArena AddOn", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class KKAAPlugin : BaseUnityPlugin
    {
        /* 1. Custom scenes like LobbyAppearanceImprovements
         * 2. Reduce size of billboarded particle effects due to blocking the screen
         * 3. 
         * 
         * 
         */

        public static ConfigEntry<float> particleSizeReduction;

        public static Dictionary<string, Diorama> dioramas = new Dictionary<string, Diorama>()
        {
            {"Arena", new Diorama(){
                dioramaObject = Resources.Load<GameObject>("prefabs/stagedisplay/ArenaDioramaDisplay"),
            }}
        };

        public static bool shouldGiveExperience = true;

        public struct Diorama
        {
            public GameObject dioramaObject;
            public Vector3 positionOffset;
            public Vector3 rotation;
            public Vector3 scale;

            public void A()
            {

            }
        }

        public void Awake()
        {
            particleSizeReduction = Config.Bind("Visuals", "Particle Effect Size Multiplier", 0.5f, "Certain particle effects will get reduced in size." +
                "\nIncluding: ");
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            //CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.ExperienceManager.AwardExperience += DenyExperience;

        }

        private void DenyExperience(On.RoR2.ExperienceManager.orig_AwardExperience orig, ExperienceManager self, Vector3 origin, CharacterBody body, ulong amount)
        {
            if (shouldGiveExperience)
                orig(self, origin, body, amount);
            return;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (!obj.isPlayerControlled && obj.teamComponent.teamIndex == TeamIndex.Player)
            {
                var a = obj.gameObject.AddComponent<WeatherParticles>();
                a.lockPosition = true;
            }
        }


        [ConCommand(commandName = "lock_exp", flags = ConVarFlags.ExecuteOnServer, helpText = "lock_exp {true|false} - Prevents experience being awarded.")]
        public static void CMD_LockExp(ConCommandArgs args)
        {
            shouldGiveExperience = !shouldGiveExperience;
            Debug.Log($"Experience has been {(shouldGiveExperience ? "un" : "")}locked.");
        }

        [ConCommand(commandName = "spawnprefab", flags = ConVarFlags.ExecuteOnServer, helpText = "spawnprefab at your location {x} {y} {z}")]
        public static void ChangeLight(ConCommandArgs args)
        {
            var a = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(args.GetArgString(0)));
            a.transform.position = args.senderBody.corePosition;
        }
    }
}
