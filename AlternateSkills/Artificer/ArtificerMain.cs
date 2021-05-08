using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using RoR2.Projectile;

namespace AlternateSkills.Artificer
{
    public class ArtificerMain
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/MageBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;
        public static GameObject debuffHitProjectile = null;
        public static GameObject buffTransferProjectile = null;

        public static void Init()
        {
            SetupProjectiles();
            SetupSkills();

            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float += DotController_InflictDot_GameObject_GameObject_DotIndex_float_float;
        }

        private static void DotController_InflictDot_GameObject_GameObject_DotIndex_float_float(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float orig, GameObject victimObject, GameObject attackerObject, DotController.DotIndex dotIndex, float duration, float damageMultiplier)
        {
            victimObject = victimObject.GetComponent<BTSendDebuffsToVictim>() ? victimObject.GetComponent<BTSendDebuffsToVictim>().victimBody.gameObject : victimObject;

            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier);
        }

        private static void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            if (!buffDef.isDebuff) //aka isBuff
            {
                self = self.gameObject.GetComponent<BTSendBuffsToAttacker>() ? self.gameObject.GetComponent<BTSendBuffsToAttacker>().attackerBody : self;
            } else
            {
                self = self.gameObject.GetComponent<BTSendDebuffsToVictim>() ? self.gameObject.GetComponent<BTSendDebuffsToVictim>().victimBody : self;
            }
            orig(self, buffDef);
        }

        private static void SetupProjectiles()
        {
            var rocketPrefab = Resources.Load<GameObject>("prefabs/projectiles/PaladinRocket");
            debuffHitProjectile = PrefabAPI.InstantiateClone(rocketPrefab, "DebuffHitProjectile");
            debuffHitProjectile.AddComponent<ApplyPlayerDebuffsOnHit>();


            buffTransferProjectile = PrefabAPI.InstantiateClone(rocketPrefab, "BuffTransferProjectile");
            buffTransferProjectile.AddComponent<GiveHitEnemyBuffTransfer>();
        }

        public class ApplyPlayerDebuffsOnHit : MonoBehaviour, IOnDamageInflictedServerReceiver
        {
            public ProjectileController projectileController = null;
            public GameObject owner = null;

            public void Awake()
            {
                projectileController = gameObject.GetComponent<ProjectileController>();
                if (projectileController)
                {
                    owner = projectileController.owner;
                }

            }

            public void OnDamageInflictedServer(DamageReport damageReport)
            {
                if (damageReport.attacker == owner)
                {
                    var ownerDebuffs = MainPlugin.ReturnBuffs(damageReport.attackerBody, true, false);
                    if (ownerDebuffs.Length > 0)
                    {
                        foreach (var buff in ownerDebuffs)
                        {
                            MainPlugin.AddBuffAndDot(buff, 5f, 1, damageReport.attackerBody);
                        }
                    }
                }
            }

        }

        public class GiveHitEnemyBuffTransfer : MonoBehaviour, IOnDamageDealtServerReceiver
        {
            public Type type;

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (damageReport.attackerBody)
                {
                    var comp = damageReport.victim.gameObject.GetComponent<BTSendBuffsToAttacker>();
                    if (!comp)
                    {
                        comp = damageReport.victim.gameObject.AddComponent<BTSendBuffsToAttacker>();
                    }
                    comp.stopwatch = 0f;

                    var comp2 = damageReport.attacker.GetComponent<BTSendDebuffsToVictim>();
                    if (!comp2)
                    {
                        comp2 = damageReport.attacker.AddComponent<BTSendDebuffsToVictim>();
                    }
                    comp2.stopwatch = 0f;
                }
            }
        }

        public class KillOnStopwatch : MonoBehaviour
        {
            public float duration = 16f;
            public float stopwatch = 0f;

            public void Update()
            {
                stopwatch += Time.deltaTime;
                if (stopwatch >= duration)
                {
                    DestroyImmediate(this);
                }
            }
        }


        public class BTSendBuffsToAttacker : KillOnStopwatch
        {
            public CharacterBody attackerBody;
        }

        public class BTSendDebuffsToVictim : KillOnStopwatch
        {
            public CharacterBody victimBody;
        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("MAGE_UTILITY_DEBUFFHIT_NAME", "Canceristor");
            LanguageAPI.Add("MAGE_UTILITY_DEBUFFHIT_DESCRIPTION", "<style=cHealth>Excises</style> a chunk out of yourself to launch a canister containing your debuffs for <style=cIsDamage>70% damage</style>. Deals <style=cIsDamage>+10% damage</style> per stack. <style=cDeath>Costs 10% of your health</style>.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Artificer.DebuffHit));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 10f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "MAGE_UTILITY_DEBUFFHIT_DESCRIPTION";
            mySkillDef.skillName = "MAGE_UTILITY_DEBUFFHIT_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            LoadoutAPI.AddSkillDef(mySkillDef);

            var skillLocator = myCharacter.GetComponent<SkillLocator>();

            var skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            LanguageAPI.Add("MAGE_SPECIAL_BUFFTRANSFER_NAME", "Tempered Shell Transfer");
            LanguageAPI.Add("MAGE_SPECIAL_BUFFTRANSFER_DESCRIPTION", "Attacks the closest enemy, targeting them. The targeted enemy receives your debuffs, and you receive their buffs.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Artificer.BuffTransfer));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 30f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "MAGE_SPECIAL_BUFFTRANSFER_DESCRIPTION";
            mySkillDef.skillName = "MAGE_SPECIAL_BUFFTRANSFER_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            LoadoutAPI.AddSkillDef(mySkillDef);

            skillLocator = myCharacter.GetComponent<SkillLocator>();

            skillFamily = skillLocator.special.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }
    }
}
