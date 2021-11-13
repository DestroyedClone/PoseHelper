using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static CrowbarProposal.Main;
using System.Collections.Generic;

namespace CrowbarProposal.Items
{
    public class CrowbarMod : ItemBase<CrowbarMod>
    {
        public override string ItemName => "Fragile Crowbar";

        public override string ItemLangTokenName => "FRAGILE_CROWBAR";

        public override string ItemPickupDesc => "The first damage you deal to an enemy is increased.";

        public override string ItemFullDescription => "The first hit you've done against an enemy deals <style=cIsDamage>+35%</style> <style=cStack>(+35% per stack)</style> damage. This damage is DOUBLED to enemies above <style=cIsDamage>90% health</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => RoR2Content.Items.Crowbar.pickupModelPrefab;

        public override Sprite ItemIcon => RoR2Content.Items.Crowbar.pickupIconSprite;

        public static float damagePerStack = 0.35f;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            // Damage Doubler Based on Healthy
            //On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            // Original suggested
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage1;
        }

        private void HealthComponent_TakeDamage1(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var crowbarTracker = self.GetComponent<FragileCrowbarTracker>();
            if (!crowbarTracker || !crowbarTracker.attackerObjects.Contains(damageInfo.attacker))
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var master = attackerBody.master;
                    if (master)
                    {
                        var stacks = GetCount(master);
                        if (stacks > 0)
                        {
                            damageInfo.damage *= 1f + 0.75f * (float)stacks;
                            EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.crowbarImpactEffectPrefab, damageInfo.position, -damageInfo.force, true);

                            if (!crowbarTracker)
                            {
                                crowbarTracker = self.gameObject.AddComponent<FragileCrowbarTracker>();
                            }
                            crowbarTracker.attackerObjects.Add(damageInfo.attacker);
                        }
                    }
                }
            }

            orig(self, damageInfo);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var crowbarTracker = self.GetComponent<FragileCrowbarTracker>();
            if (!crowbarTracker || crowbarTracker.attackerObjects.Contains(damageInfo.attacker))
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    var master = attackerBody.master;
                    if (master)
                    {
                        var stacks = GetCount(master);
                        if (stacks > 0)
                        {
                            var damageModifier = 1f + stacks * damagePerStack;

                            if (self.combinedHealth >= self.fullCombinedHealth * 0.9f)
                            {
                                damageModifier *= 2f;
                                EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.crowbarImpactEffectPrefab, damageInfo.position, -damageInfo.force, true);
                            }
                            damageInfo.damage *= damageModifier;
                            if (!crowbarTracker)
                            {
                                crowbarTracker = self.gameObject.AddComponent<FragileCrowbarTracker>();
                            }
                            crowbarTracker.attackerObjects.Add(damageInfo.attacker);
                        }
                    }
                }
            }

            orig(self, damageInfo);
        }

        public class FragileCrowbarTracker : MonoBehaviour
        {
            // Keeps track of all masters that have damaged this being.
            public List<GameObject> attackerObjects = new List<GameObject>();
        }
    }
}
