using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

using Mono.Cecil.Cil;
using MonoMod.Cil;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace GoombaStomp
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(ArtifactAPI), nameof(LoadoutAPI), nameof(LanguageAPI))]
    public class StompMain : BaseUnityPlugin
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "GoombaStompArtifact";
        public const string ModGuid = "com.DestroyedClone.GoombaStompArtifact";

        public static ArtifactDef GoombaArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
        public static ConfigEntry<float> maxDistance { get; set; }
        public static ConfigEntry<float> minFallSpeed { get; set; }
        public static ConfigEntry<float> bounceForce { get; set; }
        public static ConfigEntry<float> angleCheck { get; set; }
        public static ConfigEntry<bool> friendlyFire { get; set; }
        public static ConfigEntry<bool> deathMessages { get; set; }
        public static ConfigEntry<float> goombaDamage { get; set; }
        public static ConfigEntry<bool> multiplyDmgPerBounce { get; set; }
        public static ConfigEntry<bool> goombaInChat { get; set; }
        public static GameObject goombaGameObject = new GameObject();
        private static readonly string goombaDeathToken = "You have been Goomba Stomped!";
        private static readonly string goombaDeathMultiplayerToken = "{0} has been Goomba Stomped!";
        public static ArtifactDef FrailtyRef = Resources.Load<ArtifactDef>("artifactdefs/WeakAssKnees");

        private void Awake()
        {
            SetupConfig();
            InitializeArtifact();
            On.RoR2.CharacterMotor.Awake += CharacterMotor_Awake;
            On.RoR2.CharacterMotor.OnHitGroundServer += CharacterMotor_OnHitGroundServer;

            if (deathMessages.Value)
                On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += GlobalEventManager_OnPlayerCharacterDeath;

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        private void SetupConfig()
        {
            maxDistance = Config.Bind("Function", "Max Distance", 7f, "Max distance after landing to check for valid enemies.");
            minFallSpeed = Config.Bind("Function", "Fall Speed Required", 30f, "Minimum speed to become eligible for a goomba stomp.");
            bounceForce = Config.Bind("Function", "Bounce Force", 3000f, "Push force applied after bouncing onto an enemy.");
            angleCheck = Config.Bind("Function", "Angle Check", 135f, "Max Angle to check for enemies below.");
            friendlyFire = Config.Bind("Function", "Friendly Fire", true, "Allow teammates to get goomba'd if the friendly fire artifact is on.");
            goombaInChat = Config.Bind("Function", "Chat", false, "Prints Goomba! in chat. Should be clientside.");
            deathMessages = Config.Bind("Experimental", "Goomba Death Messages", true, "Enable death messages for being Goomba'd" +
                "\nExperimental because the hook is kinda ugly and I can't test it properly.");
            goombaDamage = Config.Bind("Experimental", "Goomba Damage Per Bounce", 500f, "Base damage for goomba bouncing.");
            multiplyDmgPerBounce = Config.Bind("Function", "Multiply Damage By Bounces", true, "The damage dealt is the product of the base damage times the amount of bounces.");
        }

        private void GlobalEventManager_OnPlayerCharacterDeath(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, GlobalEventManager self, DamageReport damageReport, NetworkUser victimNetworkUser)
        {
            if (!victimNetworkUser)
            {
                return;
            }
            if (damageReport.damageInfo.inflictor == goombaGameObject)
            {
                string text = "PLAYER_DEATH_QUOTE_GOOMBADEATH";
                if (victimNetworkUser.masterController)
                {
                    victimNetworkUser.masterController.finalMessageTokenServer = text;
                }
                Chat.SendBroadcastChat(new Chat.PlayerDeathChatMessage
                {
                    subjectAsNetworkUser = victimNetworkUser,
                    baseToken = text
                });
            }
            else
            {
                orig(self, damageReport, victimNetworkUser);
                return;
            }
        }

        private HurtBox GetValidEnemy(System.Collections.Generic.List<HurtBox> hurtBoxes)
        {
            HurtBox enemyHurtbox = null;
            foreach (var choice in hurtBoxes)
            {
                if (choice && choice.healthComponent && choice.healthComponent.alive)
                    if (!choice.healthComponent.godMode && choice.healthComponent.body)
                    {
                        var body = choice.healthComponent.body;
                        if (!body.HasBuff(RoR2Content.Buffs.HiddenInvincibility) || !body.HasBuff(RoR2Content.Buffs.Immune) || !body.HasBuff(RoR2Content.Buffs.Intangible))
                            if (choice.healthComponent.body && choice.healthComponent.body.inventory && choice.healthComponent.body.inventory.GetItemCount(RoR2Content.Items.Ghost) == 0)
                            {
                                enemyHurtbox = choice;
                                break;
                            }
                    }
            }
            return enemyHurtbox;
        }

        private void CharacterMotor_OnHitGroundServer(On.RoR2.CharacterMotor.orig_OnHitGroundServer orig, CharacterMotor self, CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            if (!RunArtifactManager.instance.IsArtifactEnabled(GoombaArtifactDef.artifactIndex))
            {
                orig(self, hitGroundInfo);
                return;
            }

            bool hasGoombad = false;
            bool restoreFallDamage = false;
            var goombaComponent = self.GetComponent<GoombaComponent>();

            if (self.body)
            {
                if (goombaComponent.inGoombaState || Math.Abs(hitGroundInfo.velocity.y) >= minFallSpeed.Value)
                {
                    TeamMask teamMask = TeamMask.GetEnemyTeams(self.body.teamComponent.teamIndex);
                    if (FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off && friendlyFire.Value)
                    {
                        teamMask = TeamMask.allButNeutral;
                    }
                    //Chat.AddMessage("Speed: " + Math.Abs(hitGroundInfo.velocity.y) + "/" + minFallSpeed);
                    var enemySearch = new BullseyeSearch()
                    {
                        filterByDistinctEntity = false,
                        filterByLoS = false,
                        maxDistanceFilter = maxDistance.Value,
                        minDistanceFilter = 0f,
                        minAngleFilter = 0f,
                        maxAngleFilter = angleCheck.Value,
                        teamMaskFilter = teamMask,
                        sortMode = BullseyeSearch.SortMode.Distance,
                        viewer = self.body,
                        searchDirection = Vector3.down,
                        searchOrigin = hitGroundInfo.position
                    };
                    enemySearch.RefreshCandidates();
                    var listOfEnemies = enemySearch.GetResults().ToList();

                    /*var str = "Nearest Enemies: ";

                    foreach(var enemy in listOfEnemies)
                    {
                        if (enemy)
                            str += enemy.healthComponent.body.GetDisplayName() + ", ";
                    }
                    Debug.Log(str);*/

                    HurtBox enemyHurtbox = GetValidEnemy(listOfEnemies);
                    if (enemyHurtbox)
                    {
                        //var headPos = Helpers.GetHeadPosition(enemyHurtbox.healthComponent.body);
                        //var distance = Vector3.Distance(hitGroundInfo.position, headPos);
                        //Chat.AddMessage("Distance to enemy is "+ distance);
                        goombaComponent.inGoombaState = true;
                        if (enemyHurtbox.healthComponent != goombaComponent.currentHealthComponent)
                        {
                            goombaComponent.bounces = 1;
                            goombaComponent.currentHealthComponent = enemyHurtbox.healthComponent;
                        }
                        enemyHurtbox.healthComponent.TakeDamage(new DamageInfo()
                        {
                            attacker = self.body.gameObject,
                            damage = multiplyDmgPerBounce.Value ? goombaDamage.Value * goombaComponent.bounces : goombaDamage.Value,
                            inflictor = goombaGameObject,
                            position = hitGroundInfo.position,
                            damageColorIndex = DamageColorIndex.Bleed
                        });
                        if ((self.body.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == CharacterBody.BodyFlags.None)
                        {
                            self.body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                            restoreFallDamage = true;
                        }
                        if (goombaInChat.Value) Chat.AddMessage("Goomba!");
                        hasGoombad = true;
                    }
                }
            }

            orig(self, hitGroundInfo);
            if (hasGoombad)
            {
                self.Motor.ForceUnground();
                self.ApplyForce(Vector3.up * bounceForce.Value);
                goombaComponent.bounces++;
            }
            else
            {
                goombaComponent.ExitGoombaState();
            }
            if (restoreFallDamage)
            {
                self.body.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            }

        }

        private void CharacterMotor_Awake(On.RoR2.CharacterMotor.orig_Awake orig, CharacterMotor self)
        {
            orig(self);
            if (RunArtifactManager.instance.IsArtifactEnabled(GoombaArtifactDef.artifactIndex))
            {
                var a = self.gameObject.AddComponent<GoombaComponent>();
                a.characterMotor = self;
                //Debug.Log("component added");
            }
        }

        public static void InitializeArtifact()
        {
            GoombaArtifactDef.nameToken = "Artifact of Goombastomping";
            GoombaArtifactDef.descriptionToken = "Deal substantial damage upon landing on an enemy's head.";
            GoombaArtifactDef.smallIconDeselectedSprite = LoadoutAPI.CreateSkinIcon(Color.black, Color.white, Color.white, Color.white);
            GoombaArtifactDef.smallIconSelectedSprite = LoadoutAPI.CreateSkinIcon(Color.gray, Color.white, Color.white, Color.white);
            ArtifactAPI.Add(GoombaArtifactDef);

            goombaGameObject.name = "GoombaStomp";

            if (deathMessages.Value)
            {
                LanguageAPI.Add("PLAYER_DEATH_QUOTE_GOOMBADEATH", goombaDeathToken);
                LanguageAPI.Add("PLAYER_DEATH_QUOTE_GOOMBADEATH_2P", goombaDeathMultiplayerToken);
            }
        }

        public class GoombaComponent : MonoBehaviour
        {
            public CharacterMotor characterMotor;
            public int bounces = 1;
            public bool inGoombaState = false;
            public HealthComponent currentHealthComponent;

            public void ExitGoombaState()
            {
                bounces = 1;
                inGoombaState = false;
            }
        }
    }

    public static class Commands
    {
        [ConCommand(commandName = "goomba_maxDistance", flags = ConVarFlags.ExecuteOnServer, helpText = "")]
        private static void A(ConCommandArgs args)
        {
            StompMain.maxDistance.Value = args.GetArgFloat(0);
        }
        [ConCommand(commandName = "goomba_minFallSpeed", flags = ConVarFlags.ExecuteOnServer, helpText = "")]
        private static void B(ConCommandArgs args)
        {
            StompMain.minFallSpeed.Value = args.GetArgFloat(0);
        }
        [ConCommand(commandName = "goomba_bounceForce", flags = ConVarFlags.ExecuteOnServer, helpText = "")]
        private static void C(ConCommandArgs args)
        {
            StompMain.bounceForce.Value = args.GetArgFloat(0);
        }
    }

    public static class Helpers
    {
        public static Vector3 GetHeadPosition(CharacterBody characterBody)
        {
            var dist = Vector3.Distance(characterBody.corePosition, characterBody.footPosition);
            return characterBody.corePosition + Vector3.up * dist;
        }
    }
}