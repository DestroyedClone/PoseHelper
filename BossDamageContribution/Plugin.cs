using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BossDamageTracker
{
    [BepInPlugin("com.DestroyedClone.BossDamageTracker", "Boss Damage Tracker", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Plugin : BaseUnityPlugin
    {
        // Figure out some way to cache the CharacterBody's name
        // Work it into a UI element
        // Localize tokens
        // Track non-attributed damage and self damage by the boss (Pots, REX self harm, fall damage, etc)
        // Damage right now only tracks effective damage (health lost) not damage taken, so maybe add an option for that?
        //public static bool hookState = false;

        public static List<BossGroup> activeBossGroups = new List<BossGroup>();

        // config
        public static ConfigEntry<bool> cfgMinionDamageIsOwner;

        public static ConfigEntry<int> cfgPlaces;

        public static ConfigEntry<bool> cfgMinionShowsOwnerName;

        public void Start()
        {
            string minionCat = "Minions";

            cfgMinionDamageIsOwner = Config.Bind(minionCat, "Owner Gets Minion Damage", true, "If true, then the damage dealt by minions will be attributed to the owner of those minions.");
            cfgMinionShowsOwnerName = Config.Bind(minionCat, "Minion Shows Owner Name", false, "If true, then the result shown will show the owner of the minion after their name." +
                "\nEx: Engineer Turret (TheEngi)" +
                "\nThis setting is incompatible if \"Owner Gets Minion Damage\" is true, since the minion won't be included in the list.");
            cfgPlaces = Config.Bind("", "Top Damage Places", 3, "The number of places available. There will be a last place which is accumulative of the rest." +
                "\nEx: With 2 places, plrA dealt 1500, plrB dealt 500, plrC dealt 250, and plrD dealt 125" +
                "\nThe result would look something like:" +
                "\n1: plrA (1500)" +
                "\n2: plrB (500)" +
                "\nThe Rest: (375)");

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

        private void RecordDamage(GameObject attackerObject, CharacterBody victimBody, float damageDealt)
        {
            bool shouldBreak = false;
            foreach (var tracker in InstanceTracker.GetInstancesList<BossDamageTracker>())
            {
                foreach (var bossMaster in tracker.bossGroup.combatSquad.membersList)
                {
                    if (bossMaster.GetBody() == victimBody)
                    {
                        var attackerBody = attackerObject.GetComponent<CharacterBody>();
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

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var damageDealt = self.health;
            orig(self, damageInfo);
            damageDealt -= self.health;
            if (damageInfo.attacker)
            {
                RecordDamage(damageInfo.attacker, self.body, damageDealt);
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

            public void AddDamage(CharacterMaster attackerMaster, float damage)
            {
                var master = attackerMaster;
                if (cfgMinionDamageIsOwner.Value)
                {
                    //https://discord.com/channels/562704639141740588/562704639569428506/759856897536163840
                    if (attackerMaster.minionOwnership &&
                        attackerMaster.minionOwnership.ownerMaster)
                    {
                        master = attackerMaster.minionOwnership.ownerMaster;
                    }
                }

                if (character_to_damage.ContainsKey(master))
                {
                    character_to_damage[master] += damage;
                }
                else
                {
                    character_to_damage.Add(master, damage);
                    //cachedNames.Add(attackerMaster, attackerMaster.GetBody().GetDisplayName());
                    //var body = attackerMaster.GetBody();
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
                string results = $"={bossGroup.bestObservedName}";

                int currentPlace = 1;
                float everyoneElseDamage = 0;
                //Chat.AddMessage($"Announcing places {places} with currentPlace {currentPlace}");
                bool tryGetMinionName = cfgMinionShowsOwnerName.Value && !cfgMinionDamageIsOwner.Value;
                foreach (var result in ordered)
                {
                    //Chat.AddMessage($"{currentPlace}");
                    string name = "???";
                    if (currentPlace <= cfgPlaces.Value)
                    {
                        //Chat.AddMessage("placeCheck");
                        var resultMaster = result.Key;
                        if (resultMaster) // if the charactermaster exists
                        {
                            var resultBody = resultMaster.GetBody();
                            if (resultBody)
                            {
                                if (resultBody.isPlayerControlled) //switch to tertiary operator?
                                {
                                    name = resultBody.GetUserName();
                                }
                                else
                                {
                                    name = resultBody.GetDisplayName();
                                    if (tryGetMinionName)
                                    {
                                        if (resultMaster.minionOwnership && resultMaster.minionOwnership.ownerMaster)
                                        {
                                            var minionOwnerBody = resultMaster.minionOwnership.ownerMaster.GetBody();
                                            if (minionOwnerBody)
                                            {
                                                name += $" ({(minionOwnerBody.isPlayerControlled ? minionOwnerBody.GetUserName() : minionOwnerBody.GetDisplayName())})";
                                            } //eeeee this looks so gross
                                        }
                                    }
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