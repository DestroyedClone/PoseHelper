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

        public static void Init()
        {
            SetupProjectiles();
            SetupSkills();
        }
        private static void SetupProjectiles()
        {
            var rocketPrefab = Resources.Load<GameObject>("prefabs/projectiles/PaladinRocket");
            debuffHitProjectile = PrefabAPI.InstantiateClone(rocketPrefab, "DebuffHitProjectile");
            debuffHitProjectile.AddComponent<ApplyPlayerDebuffsOnHit>();
        }
        public static BuffDef[] ReturnBuffs(CharacterBody characterBody, bool returnDebuffs, bool returnBuffs)
        {
            List<BuffDef> buffDefs = new List<BuffDef>();
            BuffIndex buffIndex = (BuffIndex)0;
            BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
            while (buffIndex < buffCount)
            {
                BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                if (characterBody.HasBuff(buffDef))
                {
                    if ((buffDef.isDebuff && returnDebuffs) || (!buffDef.isDebuff && returnBuffs))
                    {
                        buffDefs.Add(buffDef);
                    }
                }
                buffIndex++;
            }
            return buffDefs.ToArray();
        }

        //KomradeSpectre Aetherium AccursedPotion
        public static void AddBuffAndDot(BuffDef buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            DotController.DotIndex index = (DotController.DotIndex)Array.FindIndex(DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != DotController.DotIndex.None)
                {
                    DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    body.AddTimedBuffAuthority(buff.buffIndex, duration);
                }
            }
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
                    var ownerDebuffs = ReturnBuffs(damageReport.attackerBody, true, false);
                    if (ownerDebuffs.Length > 0)
                    {
                        foreach (var buff in ownerDebuffs)
                        {
                            AddBuffAndDot(buff, 5f, 1, damageReport.attackerBody);
                        }
                    }
                }
            }

        }

        private static void SetupSkills()
        {
            LanguageAPI.Add("MAGE_UTILITY_DEBUFFHIT_NAME", "Canceristor");
            LanguageAPI.Add("MAGE_UTILITY_DEBUFFHIT_DESCRIPTION", "<style=cHealth>Excises</style> a chunk out of yourself to launch a canister containing your debuffs for <style=cIsDamage>70% damage</style>. Deals <style=cIsDamage>+10% damage</style> per stack. <style=cDeath>Costs 10% of your health</style>.");

            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Acrid.KickOff));
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

            var skillFamily = skillLocator.secondary.skillFamily;

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
