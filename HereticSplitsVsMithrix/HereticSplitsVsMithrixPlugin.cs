using System;
using R2API;
using RoR2;
using BepInEx;
using UnityEngine;
using R2API.Utils;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HereticSplitsVsMithrix
{
    [BepInPlugin("com.DestroyedClone.HereticSplits", "Heretic Splits", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI), nameof(ProjectileAPI), nameof(DamageAPI), nameof(BuffAPI), nameof(DotAPI))]
    public class HereticSplitsVsMithrixPlugin : BaseUnityPlugin
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/HereticBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public void Awake()
        {
            SetupLanguage();
            On.RoR2.CharacterMaster.RespawnExtraLife += CharacterMaster_RespawnExtraLife;
        }

        private void SetupLanguage()
        {
            LanguageAPI.Add("DC_HERETIC_SEPERATES", "<style=cIsUtility>Heretic: We've seperated?</style>");
            //First, her many eyes were plucked from her skull and sealed in boiling glass, forced to gaze upon her failure...”
            //Visions (Primary)
            LanguageAPI.Add("DC_HERETIC_SEPERATES_NOPRIMARY", "<style=cIsUtility>Heretic: {0}, it's still dark...</style>");
            //“…her arms were warped into terrible blades, so she may no longer find joy in study or tooling…”
            //Hooks (Secondary)
            LanguageAPI.Add("DC_HERETIC_SEPERATES_NOSECONDARY", "<style=cIsUtility>Heretic: {0}, I can't feel my arms...</style>");
            //“Her legs were scattered to the two poles of the moon, twisted in a wicked position, in a field of obsidian thorns…"
            //Strides (Utility)
            LanguageAPI.Add("DC_HERETIC_SEPERATES_NOUTILITY", "<style=cIsUtility>Heretic: {0}, it's a bit hard to move...</style>");
            //“…and her heart, too wicked and full of hate, was left where she once stood – at the site of her betrayal.”
            //Essence (Special)
            LanguageAPI.Add("DC_HERETIC_SEPERATES_NOSPECIAL", "<style=cIsUtility>Heretic: {0}, it's chilling...</style>");
            LanguageAPI.Add("DC_HERETIC_SEPERATES_AMPUTEE", "<style=cIsUtility>Heretic: {0}, only you can put an end to this.</style>");
        }

        private void CharacterMaster_RespawnExtraLife(On.RoR2.CharacterMaster.orig_RespawnExtraLife orig, CharacterMaster self)
        {
            // one is already consumed before calling this
            var spawnHeretic = false;
            if (NetworkServer.active)
            {
                if (self && self.playerCharacterMasterController && self.inventory && self.inventory.GetItemCount(RoR2Content.Items.ExtraLife) > 0
                    && self.GetBody() && self.GetBody().name.StartsWith("Heretic"))
                {
                    spawnHeretic = true;
                    self.inventory.RemoveItem(RoR2Content.Items.ExtraLife); //heretic's
                }
            }
            orig(self);
            if (spawnHeretic)
            {
                self.inventory.GiveItem(RoR2Content.Items.ExtraLifeConsumed);
                var summon = SummonHeretic(self.bodyInstanceObject, self.deathFootPosition);
                var chatter = summon.gameObject.AddComponent<HereticChatter>();
                chatter.characterMaster = self;

                var preferredBody = self.playerCharacterMasterController.networkUser.bodyIndexPreference;


                #region heretic chatting
                bool primary = true;
                bool secondary = true;
                bool utility = true;
                bool special = true;
                Chat.SendBroadcastChat(new SubjectChatMessage()
                {
                   baseToken = "PLAYER_CONNECTED",
                   subjectAsCharacterBody = summon.GetBody(),
                });

                if (summon.inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) == 0)
                {
                    chatter.AddLine("DC_HERETIC_SEPERATES_NOPRIMARY");
                    primary = false;
                }
                self.inventory.RemoveItem(RoR2Content.Items.LunarPrimaryReplacement, summon.inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement));
                if (summon.inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement) == 0)
                {
                    chatter.AddLine("DC_HERETIC_SEPERATES_NOSECONDARY");
                    secondary = false;
                }
                self.inventory.RemoveItem(RoR2Content.Items.LunarSecondaryReplacement, summon.inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement));
                if (summon.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement) == 0)
                {
                    chatter.AddLine("DC_HERETIC_SEPERATES_NOUTILITY");
                    utility = false;
                }
                self.inventory.RemoveItem(RoR2Content.Items.LunarUtilityReplacement, summon.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement));
                if (summon.inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement) == 0)
                {
                    chatter.AddLine("DC_HERETIC_SEPERATES_NOSPECIAL");
                    special = false;
                }
                self.inventory.RemoveItem(RoR2Content.Items.LunarSpecialReplacement, summon.inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement));
                if (!primary && !secondary && !utility && !special)
                {
                    chatter.AddLine("DC_HERETIC_SEPERATES_AMPUTEE");
                }
                #endregion
                self.TransformBody(BodyCatalog.GetBodyName(preferredBody));
            }
        }

        public class HereticChatter : MonoBehaviour
        {
            public CharacterMaster characterMaster;
            public float ChatDelay = 4f;
            private float age;
            public List<string> ChatMessages = new List<string>();
            string thing;

            public void Start()
            {
                thing = characterMaster.playerCharacterMasterController ? characterMaster.GetBody().GetUserName() : characterMaster.GetBody().GetDisplayName();
            }

            public void AddLine(string message)
            {
                ChatMessages.Add(message);
            }
            public void FixedUpdate()
            {
                age += Time.fixedDeltaTime;
                if (age >= ChatDelay)
                {
                    if (ChatMessages.Count > 0)
                    {
                        age = 0;
                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        {
                            baseToken = ChatMessages[0],
                            paramTokens = new string[]
                            {
                                thing
                            }
                        });
                        ChatMessages.RemoveAt(0);
                    } else
                    {
                        enabled = false;
                    }
                }
            }

        }

        [Server]
        public CharacterMaster SummonHeretic(GameObject summonerBodyObject, Vector3 spawnPosition)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'SummonHeretic(UnityEngine.GameObject)' called on client");
                return null;
            }
            MasterSummon masterSummon = new MasterSummon
            {
                masterPrefab = MasterCatalog.FindMasterPrefab("HereticMonsterMaster"),
                position = spawnPosition,
                rotation = Quaternion.identity,
                summonerBodyObject = summonerBodyObject,
                ignoreTeamMemberLimit = true,
                useAmbientLevel = new bool?(true),
            };
            CharacterMaster characterMaster = masterSummon.Perform();

            if (characterMaster)
            {
                GameObject bodyObject = characterMaster.GetBodyObject();
                if (bodyObject)
                {
                    ModelLocator component = bodyObject.GetComponent<ModelLocator>();
                    if (component && component.modelTransform)
                    {
                        TemporaryOverlay temporaryOverlay = component.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                        temporaryOverlay.duration = 0.5f;
                        temporaryOverlay.animateShaderAlpha = true;
                        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        temporaryOverlay.destroyComponentOnEnd = true;
                        temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matSummonDrone");
                        temporaryOverlay.AddToCharacerModel(component.modelTransform.GetComponent<CharacterModel>());
                    }
                    var inventory = characterMaster.inventory;
                    if (inventory)
                    {
                        inventory.CopyItemsFrom(summonerBodyObject.GetComponent<CharacterBody>().inventory);
                    }
                }
            }
            if (characterMaster && characterMaster.bodyInstanceObject)
            {
                characterMaster.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
                GameObject gameObject = Resources.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
                if (gameObject)
                {
                    EffectManager.SpawnEffect(gameObject, new EffectData
                    {
                        origin = spawnPosition,
                        rotation = characterMaster.bodyInstanceObject.transform.rotation
                    }, true);
                }
            }
            return characterMaster;
        }
    }
}
