using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

using System.Collections.ObjectModel;
using System.Globalization;
using RoR2.ConVar;
using RoR2.Networking;
using Unity;
using UnityEngine.Networking;
using RoR2.UI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BossDamageContribution
{
    [BepInPlugin("com.DestroyedClone.BossDamageContribution", "Boss Damage Contribution", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        // Figure out some way to cache the CharacterBody's name
        // Switch to submitchat for networking
        // Work it into a UI element
        // Minions should probably be included in the player's damage instead of their own bodies
        //public static bool hookState = false;

        public static List<BossGroup> activeBossGroups = new List<BossGroup>();

        public void Start()
        {
            BossGroup.onBossGroupStartServer += BossGroup_onBossGroupStartServer;
            BossGroup.onBossGroupDefeatedServer += BossGroup_onBossGroupDefeatedServer;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            Stage.onServerStageBegin += ResetStoredBossGroups;
        }

        private void ResetStoredBossGroups(Stage obj)
        {
            if (activeBossGroups.Count > 0)
            {
                //Chat.AddMessage("Restarting active bossgroups");
                activeBossGroups.Clear();
                Subscribe(false);
            }
        }

        public void Subscribe(bool add)
        {
            if (add)
                On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            else
                On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var damageDealt = self.health;
            orig(self, damageInfo);
            if (!damageInfo.attacker || !damageInfo.attacker.GetComponent<CharacterBody>()) return;
            damageDealt -= self.health;
            bool shouldBreak = false;
            foreach (var tracker in InstanceTracker.GetInstancesList<BossDamageTracker>())
            {
                foreach (var bossMaster in tracker.bossGroup.combatSquad.membersList)
                {
                    if (bossMaster.GetBody() == self.body)
                    {
                        var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody && attackerBody.master)
                        {
                            tracker.AddDamage(attackerBody.master, damageDealt);
                        }
                        shouldBreak = true;
                        break;
                    }
                }
                if (shouldBreak) break;
            }

        }

        private void BossGroup_onBossGroupStartServer(BossGroup bossGroup)
        {
            if (!bossGroup.GetComponent<BossDamageTracker>())
            {
                var bdt = bossGroup.gameObject.AddComponent<BossDamageTracker>();
                bdt.bossGroup = bossGroup;
            }
            if (activeBossGroups.Count == 0)
            {
                Subscribe(true);
                //Chat.AddMessage($"Bossgroup Count: {currentBossGroupCount}");
            }
            activeBossGroups.Add(bossGroup);
        }

        private void BossGroup_onBossGroupDefeatedServer(BossGroup bossGroup)
        {
            activeBossGroups.Remove(bossGroup);
            var bdt = bossGroup.gameObject.GetComponent<BossDamageTracker>();
            if (bdt)
            {
                bdt.AnnounceResults();
            }
            if (activeBossGroups.Count == 0)
            {
                Subscribe(false);
                //Chat.AddMessage("No more bossgroups!");
            }
        }

        private class BossDamageTracker : MonoBehaviour
        {
            public BossGroup bossGroup;
            public Dictionary<CharacterMaster, float> character_to_damage = new Dictionary<CharacterMaster, float>();
            //public Dictionary<CharacterMaster, string> cachedNames = new Dictionary<CharacterMaster, string>();
            public float totalDamageDealt = 0;

            public int places = 3;


            public void AddDamage(CharacterMaster attackerMaster, float damage)
            {
                if (character_to_damage.ContainsKey(attackerMaster))
                {
                    character_to_damage[attackerMaster] += damage;
                }
                else
                {
                    character_to_damage.Add(attackerMaster, damage);
                    //cachedNames.Add(attackerMaster, attackerMaster.GetBody().GetDisplayName());
                    var body = attackerMaster.GetBody();
                    //Chat.AddMessage("Starting tracking for " + (body.isPlayerControlled ? body.GetUserName() : body.GetDisplayName()));
                }
                totalDamageDealt += damage;
            }

            private void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            private void OnDisable()
            {
                InstanceTracker.Remove(this);
            }

            public void AnnounceResults()
            {
                var ordered = character_to_damage.OrderByDescending(key => key.Value);
                string results = "";

                int currentPlace = 1;
                float everyoneElseDamage = 0;
                //Chat.AddMessage($"Announcing places {places} with currentPlace {currentPlace}");
                foreach (var result in ordered)
                {
                    //Chat.AddMessage($"{currentPlace}");
                    string name = "???";
                    if (currentPlace <= places)
                    {
                        //Chat.AddMessage("placeCheck");
                        if (result.Key) // if the charactermaster exists
                        {
                            var resultBody = result.Key.GetBody();
                            if (resultBody)
                            {
                                if (resultBody.isPlayerControlled) //switch to tertiary operator?
                                {
                                    name = resultBody.GetUserName();
                                } else
                                {
                                    name = resultBody.GetDisplayName();
                                }
                            }
                        }
                        var percentageString = (result.Value / totalDamageDealt) * 100;
                        results += $"\n({currentPlace}) <style=cIsUtility>{name}</style> - <style=cIsDamage>{result.Value}</style> ({percentageString:F2}%)";
                        currentPlace++;
                        continue;
                    }
                    everyoneElseDamage += result.Value;
                }
                if (everyoneElseDamage > 0)
                {
                    var everyoneElsePercentage = (everyoneElseDamage / totalDamageDealt) * 100;
                    results += $"\n({currentPlace}) <style=cIsUtility>The Rest</style> - <style=cIsDamage>{everyoneElseDamage}</style> ({everyoneElsePercentage:F2}%)";
                }
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                {
                    baseToken = results
                });
            }
        }
    }
}
