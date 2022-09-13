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

namespace EngiProjectileTeamSwapSkill.Engineer
{
    public class CommandoMain : SurvivorMain
    {
        public override string CharacterName => "Engi";

        public static GameObject bubbleShieldPrefab = Resources.Load<GameObject>("prefabs/projectiles/engibubbleshield");
        public static GameObject timeBubbleWardPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/timebubbleward");
        public static GameObject reflectShieldPrefab;

        public static SkillDef reflectShieldSkillDef;
        public static SkillDef stickyShieldSkillDef;
        public static SkillDef trickShieldSkillDef;

        public override void SetupLanguage()
        {
            LanguageAPI.Add("KEYWORD_REFLECTSHIELD", "[ Reflect ]" +
                "\nReflected projectiles deal 75% less damage and are unable to proc items.");
            LanguageAPI.Add("KEYWORD_STICKYSHIELD_ATTACH", "[ Sticky ]" +
                "\nThis shield is attached to allies that walk over it.");
            LanguageAPI.Add("KEYWORD_STICKYSHIELD_REDUCE", "[ Reduce Damage ]" +
                "\nThis shield reduces damage of incoming attacks by 25%.");
            LanguageAPI.Add("KEYWORD_TRICKSHIELD", "[ Team Swap ]" +
                "\nProjectiles that pass through can deal damage to any team.");
        }

        public override void SetupAssets()
        {
            reflectShieldPrefab = PrefabAPI.InstantiateClone(bubbleShieldPrefab, "EngiReflectShield", true);
            //reflectShieldPrefab.layer = timeBubbleWardPrefab.layer;
            var collider2 = reflectShieldPrefab.transform.Find("Collision");
            UnityEngine.Object.Destroy(collider2);

            var comp = reflectShieldPrefab.gameObject.AddComponent<ReflectProjectileZone>();
            comp.teamFilter = reflectShieldPrefab.GetComponent<ProjectileController>().teamFilter;
            /*
            var bubbleEffect = UnityEngine.Object.Instantiate(timeBubbleWardPrefab, reflectShieldPrefab.transform);
            var collider = bubbleEffect.transform.Find("Visuals+Collider");
            bubbleEffect.transform.GetComponentInChildren<BuffWard>().enabled = false;
            var slowComp = collider.GetComponent<SlowDownProjectiles>();
            var newComp = slowComp.gameObject.AddComponent<ReflectProjectileZone>();
            newComp.teamFilter = reflectShieldPrefab.GetComponent<ProjectileController>().teamFilter;
            UnityEngine.Object.Destroy(slowComp);

            ProjectileAPI.Add(reflectShieldPrefab);*/
        }


        public override void SetupUtility()
        {
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_NAME", "Reflect Shield");
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION", "Place a <style=cIsUtility>reflective shield</style> that <style=cIsDamage>reflects</style> attacks back at their attacker.");

            var oldDef = Resources.Load<SkillDef>("skilldefs/engibody/engibodyplacebubbleshield");
            reflectShieldSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            reflectShieldSkillDef.activationState = oldDef.activationState;
            reflectShieldSkillDef.activationStateMachineName = oldDef.activationStateMachineName;
            reflectShieldSkillDef.baseMaxStock = 1;
            reflectShieldSkillDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            reflectShieldSkillDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            reflectShieldSkillDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            reflectShieldSkillDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            reflectShieldSkillDef.interruptPriority = oldDef.interruptPriority;
            reflectShieldSkillDef.isCombatSkill = oldDef.isCombatSkill;
            reflectShieldSkillDef.mustKeyPress = oldDef.mustKeyPress;
            reflectShieldSkillDef.rechargeStock = 1;
            reflectShieldSkillDef.requiredStock = 1;
            reflectShieldSkillDef.stockToConsume = 1;
            reflectShieldSkillDef.icon = oldDef.icon;
            reflectShieldSkillDef.skillDescriptionToken = "DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION";
            reflectShieldSkillDef.skillName = "DC_COMMANDO_UTILITY_TACTICALDIVE_NAME";
            reflectShieldSkillDef.skillNameToken = reflectShieldSkillDef.skillName;
            reflectShieldSkillDef.keywordTokens = new string[]
            {
                "KEYWORD_AGILE",
                "KEYWORD_REFLECTSHIELD"
            };

            LoadoutAPI.AddSkillDef(reflectShieldSkillDef);

            var skillFamily = SurvivorSkillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = reflectShieldSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(reflectShieldSkillDef.skillNameToken, false, null)
            };

            /*
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_NAME", "Reflect Shield");
            LanguageAPI.Add("DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION", "Place a <style=cIsUtility>reflective shield</style> that <style=cIsDamage>reflects</style> attacks back at their attacker.");

            reflectShieldSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            reflectShieldSkillDef.activationState = oldDef.activationState;
            reflectShieldSkillDef.activationStateMachineName = oldDef.activationStateMachineName;
            reflectShieldSkillDef.baseMaxStock = 1;
            reflectShieldSkillDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            reflectShieldSkillDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            reflectShieldSkillDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            reflectShieldSkillDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            reflectShieldSkillDef.interruptPriority = oldDef.interruptPriority;
            reflectShieldSkillDef.isCombatSkill = oldDef.isCombatSkill;
            reflectShieldSkillDef.mustKeyPress = oldDef.mustKeyPress;
            reflectShieldSkillDef.rechargeStock = 1;
            reflectShieldSkillDef.requiredStock = 1;
            reflectShieldSkillDef.stockToConsume = 1;
            reflectShieldSkillDef.icon = oldDef.icon;
            reflectShieldSkillDef.skillDescriptionToken = "DC_COMMANDO_UTILITY_TACTICALDIVE_DESCRIPTION";
            reflectShieldSkillDef.skillName = "DC_COMMANDO_UTILITY_TACTICALDIVE_NAME";
            reflectShieldSkillDef.skillNameToken = reflectShieldSkillDef.skillName;
            reflectShieldSkillDef.keywordTokens = new string[]
            {
                "KEYWORD_AGILE",
                "KEYWORD_REFLECTSHIELD"
            };

            LoadoutAPI.AddSkillDef(reflectShieldSkillDef);*/
        }

        public override void Hooks()
        {
            base.Hooks();

            On.EntityStates.Engi.EngiWeapon.FireMines.OnEnter += FireMines_OnEnter;
        }

        private bool UtilityIsSkillDef(EntityStateMachine esm, SkillDef skillDef)
        {
            return esm.commonComponents.characterBody?.skillLocator?.utility?.skillDef
                && esm.commonComponents.characterBody.skillLocator.utility.skillDef == skillDef;
        }

        private void FireMines_OnEnter(On.EntityStates.Engi.EngiWeapon.FireMines.orig_OnEnter orig, EntityStates.Engi.EngiWeapon.FireMines self)
        {
            bool isReflectShield = UtilityIsSkillDef(self.outer, reflectShieldSkillDef);
            var cachedProjectile = self.projectilePrefab;
            if (isReflectShield)
            {
                self.projectilePrefab = reflectShieldPrefab;
            }
            orig(self);
            self.projectilePrefab = cachedProjectile;
        }
    }
}
