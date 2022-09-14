using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace RailgunnerGitGud
{
    [BepInPlugin("com.DestroyedClone.RailgunnerGitsGud", "Railgunner Gits Gud", "1.0")]
    public class Class1 : BaseUnityPlugin
    {
        /*
        public static ConfigEntry<float> cfgBodyShotDamage;
        public static ConfigEntry<bool> cfgBodyShotDamageCanKill;
        public static ConfigEntry<bool> cfgBodyShotFatal;
        public static ConfigEntry<float> cfgMissDamage;
        public static ConfigEntry<bool> cfgMissCanKill;
        public static ConfigEntry<bool> cfgMissFatal;
        */

        public static ConfigEntry<float> cfgBoostFailDamage;
        public static ConfigEntry<bool> cfgBoostFailDamageCanKill;
        public static ConfigEntry<bool> cfgBoostFailFatal;

        public static ConfigEntry<bool> cfgTauntEnable;

        public static DamageType fatalDamageType;

        public static string[] tauntsDamageVariant = new string[]
        {
        };

        // 0 = player
        // 1 = miss variants
        // 2 = love variants
        public static string[] taunts = new string[]
        {
            "<style=cIsDamage>{0}</style> <style=cIsHealth>{1}ed</style> a <style=cIsDamage>perfect reload!</style>",
            "<style=cIsDamage>{0}</style> {2} hitting <style=cIsHealth>failed reloads</style>!",
            //"<style=cIsDamage>{0}</style> missed that one, try another!",
            //"Hell of a shot, <style=cIsDamage>{0}</style>, try another!",
            "<style=cIsDamage>{0}</style> thought they were supposed to <i>avoid</i> <style=cIsDamage>perfect reloads</style>.",
            "<style=cIsUtility>Point and laugh</style> at <style=cIsDamage>{0}</style> for <style=cIsHealth>{1}ing</style> a <style=cIsDamage>perfect reload</style>!",
            "<style=cIsDamage>{0}</style> does not {2} dealing <style=cIsDamage>extra damage</style> on charged shots!"
        };

        public static string[] tauntsMissVar = new string[]
        {
            "miss",
            "whiff",
            "dodg",
            "avoid",
            "evad",
            "sidestepp",
            "elud",
        };

        public static string[] tauntsLoveVar = new string[]
        {
            "loves",
            "likes",
            "leans towards",
            "is inclined towards",
            "enjoys",
            "appreciates",
            "has a soft spot for",
            "delights in",
            "relishes in",
            "has a passion for",
            "has an appetite for",
            "is enthusiastic for",
        };

        public void Start()
        {
            //var boostState = Addressables.LoadAssetAsync<RoR2.EntityStateConfiguration>("RoR2/DLC1/Railgunner/EntityStates.Railgunner.Reload.Boosted.asset").WaitForCompletion();
            //var boosted = new EntityStates.Railgunner.Reload.Boosted();
            //taunts[6] = $"<style=cIsDamage>{{0}}</style> does not {{2}} dealing <style=cIsDamage>extra damage</style> on charged shots!";

            fatalDamageType = DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection;
            /*
            cfgBodyShotDamage = Config.Bind("Body Shot", "Self Damage", 0.1f, "Percentage of your max health to take in damage upon hitting a body shot");
            cfgBodyShotDamageCanKill = Config.Bind("Body Shot", "Self Damage Can Kill", true, "If true, then the damage taken can kill you.");
            cfgBodyShotFatal = Config.Bind("Body Shot", "Fatal", false, "If true, then you will instantly die upon hitting a body shot. Ignores \"Self Damage\" setting for \"Body Shot\".");

            cfgMissDamage = Config.Bind("Miss", "Self Damage", 0.25f, "Percentage of your max health to take in damage upon missing");
            cfgMissCanKill = Config.Bind("Miss", "Self Damage Can Kill", true, "If true, then the damage taken can kill you.");
            cfgMissFatal = Config.Bind("Miss", "Fatal", false, "If true, then you will instantly die upon missing. Ignores \"Self Damage\" setting for \"Miss\".");
            */
            cfgBoostFailDamage = Config.Bind("Perfect Reload Miss", "Self Damage", 0.25f, "Percentage of your max health to take in damage upon failing a perfect reload");
            cfgBoostFailDamageCanKill = Config.Bind("Perfect Reload Miss", "Self Damage Can Kill", true, "If true, then the damage taken can kill you.");
            cfgBoostFailFatal = Config.Bind("Perfect Reload Miss", "Fatal", false, "If true, then you will instantly die upon missing. Ignores \"Self Damage\" setting for \"Miss\".");

            cfgTauntEnable = Config.Bind("Taunt", "Taunt in Chat", true, "If true, then the player who missed their perfect reload will get taunted.");

            //On.EntityStates.Railgunner.Weapon.BaseFireSnipe.ModifyBullet += BaseFireSnipe_ModifyBullet;
            //On.EntityStates.Railgunner.Weapon.BaseFireSnipe.OnExit += BaseFireSnipe_OnExit;
            On.EntityStates.Railgunner.Reload.Reloading.AttemptBoost += Reloading_AttemptBoost;
        }

        /*
        private void BaseFireSnipe_OnExit(On.EntityStates.Railgunner.Weapon.BaseFireSnipe.orig_OnExit orig, EntityStates.Railgunner.Weapon.BaseFireSnipe self)
        {
            orig(self);
            if (self.outer.commonComponents.characterBody)
            {
                var cb = self.outer.commonComponents.characterBody;
                if (self.wasMiss)
                {
                    if (cfgMissFatal.Value)
                    {
                        cb.healthComponent.Suicide(cb.gameObject, cb.gameObject);
                        Chat.AddMessage("You missed! Kill yourself.");
                    }
                    else
                    {
                        DamageInfo damageInfo = new DamageInfo()
                        {
                            crit = true,
                            damage = cb.healthComponent.fullCombinedHealth * cfgMissDamage.Value,
                            damageColorIndex = DamageColorIndex.Sniper,
                            damageType = fatalDamageType | (cfgMissCanKill.Value ? DamageType.Generic : DamageType.NonLethal),
                            position = cb.corePosition,
                            procChainMask = default,
                            procCoefficient = 0
                        };
                        if ((cb.healthComponent.fullCombinedHealth - damageInfo.damage) <= 0)
                        {
                            damageInfo.attacker = cb.gameObject;
                            damageInfo.inflictor = cb.gameObject;
                        }
                        cb.healthComponent.TakeDamage(damageInfo);
                        Chat.AddMessage("You missed!");
                    }
                }
                else
                {
                    if (!self.wasAtLeastOneWeakpoint)
                    {
                        if (cfgBodyShotFatal.Value)
                        {
                            cb.healthComponent.Suicide(cb.gameObject, cb.gameObject);
                            Chat.AddMessage("You missed a weakpoint! Kill yourself.");
                        }
                        else
                        {
                            DamageInfo damageInfo = new DamageInfo()
                            {
                                crit = true,
                                damage = cb.healthComponent.fullCombinedHealth * cfgBodyShotDamage.Value,
                                damageColorIndex = DamageColorIndex.Sniper,
                                damageType = fatalDamageType | (cfgBodyShotDamageCanKill.Value ? DamageType.Generic : DamageType.NonLethal),
                                position = cb.corePosition,
                                procChainMask = default,
                                procCoefficient = 0
                            };
                            if ((cb.healthComponent.fullCombinedHealth - damageInfo.damage) <= 0)
                            {
                                damageInfo.attacker = cb.gameObject;
                                damageInfo.inflictor = cb.gameObject;
                            }
                            cb.healthComponent.TakeDamage(damageInfo);
                            Chat.AddMessage("You missed a weakpoint!");
                        }
                    }
                }
            }
        }
        */

        private void SendFailMessage(CharacterBody cb)
        {
            if (!cfgTauntEnable.Value)
                return;

            var token = taunts[UnityEngine.Random.Range(0, taunts.Length)];
            var love = tauntsLoveVar[UnityEngine.Random.Range(0, tauntsLoveVar.Length)];
            var miss = tauntsMissVar[UnityEngine.Random.Range(0, tauntsMissVar.Length)];

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
            {
                baseToken = string.Format(token, new object[] { cb.GetUserName(), miss, love })
            });
        }

        private bool Reloading_AttemptBoost(On.EntityStates.Railgunner.Reload.Reloading.orig_AttemptBoost orig, EntityStates.Railgunner.Reload.Reloading self)
        {
            var original = orig(self);
            if (!original)
            {
                if (NetworkServer.active)
                {
                    var cb = self.outer.commonComponents.characterBody;
                    if (cb)
                    {
                        if (cfgBoostFailFatal.Value)
                        {
                            cb.healthComponent.Suicide(cb.gameObject, cb.gameObject);
                        }
                        else
                        {
                            if (cfgBoostFailDamage.Value > 0)
                            {
                                DamageInfo damageInfo = new DamageInfo()
                                {
                                    crit = true,
                                    damage = cb.healthComponent.fullCombinedHealth * cfgBoostFailDamage.Value,
                                    damageColorIndex = DamageColorIndex.Sniper,
                                    damageType = fatalDamageType | (cfgBoostFailDamageCanKill.Value ? DamageType.Generic : DamageType.NonLethal),
                                    position = cb.corePosition,
                                    procChainMask = default,
                                    procCoefficient = 0
                                };
                                if ((cb.healthComponent.fullCombinedHealth - damageInfo.damage) <= 0)
                                {
                                    damageInfo.attacker = cb.gameObject;
                                    damageInfo.inflictor = cb.gameObject;
                                }
                                cb.healthComponent.TakeDamage(damageInfo);
                            }
                        }
                        SendFailMessage(cb);
                    }
                }
            }
            return original;
        }

        /*
        private void BaseFireSnipe_ModifyBullet(On.EntityStates.Railgunner.Weapon.BaseFireSnipe.orig_ModifyBullet orig, EntityStates.Railgunner.Weapon.BaseFireSnipe self, RoR2.BulletAttack bulletAttack)
        {
            orig(self, bulletAttack);
            if (self.outer.commonComponents.characterBody)
            {
                var cb = self.outer.commonComponents.characterBody;
                if (self.wasMiss)
                {
                    if (cfgMissFatal.Value)
                    {
                        cb.healthComponent.Suicide(cb.gameObject, cb.gameObject);
                        Chat.AddMessage("You missed! Kill yourself.");
                    }
                    else
                    {
                        DamageInfo damageInfo = new DamageInfo()
                        {
                            crit = true,
                            damage = cb.healthComponent.fullCombinedHealth * cfgMissDamage.Value,
                            damageColorIndex = DamageColorIndex.Sniper,
                            damageType = fatalDamageType | (cfgMissCanKill.Value ? DamageType.Generic : DamageType.NonLethal),
                            position = cb.corePosition,
                            procChainMask = default,
                            procCoefficient = 0
                        };
                        if ((cb.healthComponent.fullCombinedHealth - damageInfo.damage) <= 0)
                        {
                            damageInfo.attacker = cb.gameObject;
                            damageInfo.inflictor = cb.gameObject;
                        }
                        cb.healthComponent.TakeDamage(damageInfo);
                        Chat.AddMessage("You missed!");
                    }
                }
                else
                {
                    return;
                    if (!self.wasAtLeastOneWeakpoint)
                    {
                        if (cfgBodyShotFatal.Value)
                        {
                            cb.healthComponent.Suicide(cb.gameObject, cb.gameObject);
                            Chat.AddMessage("You missed a weakpoint! Kill yourself.");
                        }
                        else
                        {
                            DamageInfo damageInfo = new DamageInfo()
                            {
                                crit = true,
                                damage = cb.healthComponent.fullCombinedHealth * cfgBodyShotDamage.Value,
                                damageColorIndex = DamageColorIndex.Sniper,
                                damageType = fatalDamageType |  (cfgBodyShotDamageCanKill.Value ? DamageType.Generic : DamageType.NonLethal),
                                position = cb.corePosition,
                                procChainMask = default,
                                procCoefficient = 0
                            };
                            if ((cb.healthComponent.fullCombinedHealth - damageInfo.damage) <= 0)
                            {
                                damageInfo.attacker = cb.gameObject;
                                damageInfo.inflictor = cb.gameObject;
                            }
                            cb.healthComponent.TakeDamage(damageInfo);
                            Chat.AddMessage("You missed a weakpoint!");
                        }
                    }
                }
            }
        }*/
    }
}