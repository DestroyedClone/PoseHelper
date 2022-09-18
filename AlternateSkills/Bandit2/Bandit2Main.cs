using RoR2;
using BepInEx.Configuration;

namespace AlternateSkills.Bandit2
{
    public class Bandit2 : SurvivorMain
    {
        public override string CharacterName => "Bandit2";
        public string TokenPrefix = "DCALTSKILLS_BANDIT2";
        public float damageRange = 0.5f;

        public override void Init(ConfigFile config)
        {
            base.Init(config);
            SetupBandit2Body();
        }

        public void SetupBandit2Body()
        {
            var bodyPrefab = RoR2Content.Survivors.Bandit2.bodyPrefab;
            //bodyPrefab.GetComponent<BackstabManager>()?.enabled = false;
            var skillLoctor = bodyPrefab.GetComponent<SkillLocator>();
            skillLoctor.passiveSkill.skillNameToken = TokenPrefix+"_PASSIVE_NAME";
            skillLoctor.passiveSkill.skillDescriptionToken = TokenPrefix+"_PASSIVE_DESC";
        }

        public override void SetupPassive()
        {
            base.SetupPassive();
            On.RoR2.HealthComponent.TakeDamage += RandomizeBanditDamage;
        }

        public void RandomizeBanditDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker?.GetComponent<CharacterBody>()?.bodyIndex == BodyIndex)
            {
                damageInfo.damage *= + (UnityEngine.Random.Range(-damageRange, damageRange));
            }
            orig(self, damageInfo);
        }
    }
}
