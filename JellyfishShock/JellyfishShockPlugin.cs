using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace JellyfishShock
{
    [BepInPlugin("com.DestroyedClone.OriginalJellyfish", "Original Jellyfish", "1.0.2")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI), nameof(ContentAddition))]
    [BepInDependency("com.ThinkInvisible.TILER2", BepInDependency.DependencyFlags.SoftDependency)]
    public class JellyfishShockPlugin : BaseUnityPlugin
    {
        public static GameObject myCharacter;
        public static BodyIndex jellyBodyIndex;

        public static GameObject shockEffect;

        public static ConfigEntry<float> JellyfishBaseDamage;
        public static ConfigEntry<float> JellyfishLevelDamage;
        public static ConfigEntry<float> JellyfishDischargeDamageCoefficient;
        public static ConfigEntry<bool> JellyfishDischargeCanCrit;
        public static ConfigEntry<bool> JellyfishDischargeCanShock;
        public static ConfigEntry<bool> JellyfishLoreChange;

        public static ConfigEntry<bool> JellyfishHitstun;
        public static ConfigEntry<bool> JellyfishStun;
        public static ConfigEntry<bool> JellyfishCollision;
        public static ConfigEntry<bool> JellyfishKnockback;


        internal static BepInEx.Logging.ManualLogSource _logger;

        public static int incremeter = 0;

        public static T Load<T>(string assetPath)
        {
            var loadedAsset = Addressables.LoadAssetAsync<T>(assetPath).WaitForCompletion();
            return loadedAsset;
        }

        public void Start()
        {
            _logger = Logger;
            myCharacter = Load<GameObject>("RoR2/Base/Jellyfish/JellyfishBody.prefab");
            jellyBodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;
            SetupConfig();
            CreateShockEffect();
            SetupLanguage();
            SetupBody();
            SetupSkills();

            //R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        //[ConCommand(commandName = "spawn_effect", flags = ConVarFlags.ExecuteOnServer, helpText = "")]
        public static void ChangeLight(ConCommandArgs args)
        {
            var effect = Resources.Load<GameObject>(args.GetArgString(0));
            if (effect)
            {
                EffectManager.SpawnEffect(effect, new EffectData
                {
                    origin = args.senderBody.corePosition,
                    scale = JellyShockSkill.novaRadius
                }, true);
            }
        }

        public static void CreateShockEffect()
        {
            shockEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Lightning/LightningStrikeImpact.prefab"), "OriginalJellyshockEffect", false); ;
            //var ukuEffect = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningOrbEffect");
            shockEffect.transform.localScale *= 0.25f;
            Destroy(shockEffect.transform.Find("LightningRibbon").gameObject);
            shockEffect.transform.Find("Flash").transform.localScale *= 0.5f;
            //Destroy(shockEffect.GetComponent<EffectComponent>());
            //shockEffect.AddComponent<EffectComponent>();
            //shockEffect.GetComponent<EffectComponent>().soundName = ukuEffect.GetComponent<EffectComponent>().soundName;

            if (R2API.ContentAddition.AddEffect(shockEffect))
            {
                _logger.LogMessage("Added shockEffect to ContentPack");
            }
            else
            {
                _logger.LogWarning("shockEffect failed to be added to ContentPack.");
            }
        }

        public void SetupConfig()
        {
            var bodyBaseDamage = "Base Damage";
            var bodyLevelDamage = "Level Damage";
            var skillNameDamageCoefficient = "Damage Coefficient";
            var loreOverride = "Lore Override";
            var loreOverrideDesc = "If true, replaces the lore with the one from \"Risk of Rain 1\"";

            var allowHitstun = "Allow hitstun";
            var allowHitstunDesc = "Sufficiently strong hits can temporarily stun this enemy.";
            var allowStun = "Allow stuns";
            var allowStunDesc = "Allow attacks like Commando's Suppressive Fire or items like Stun Grenade to stun this enemy.";
            var allowCollision = "Enable Collision";
            var allowCollisionDesc = "Prevents the jellyfish from passing through walls.";
            var allowKnockback = "Allow Knockback";

            //var currentLanguageIsNull = Language.currentLanguage == null;

            //var currentLanguage = currentLanguageIsNull ? "en" : Language.currentLanguageName;
            //var realName = currentLanguageIsNull ? "English" : Language.currentLanguage.selfName;

            //var commandoSpecialLocalize = Language.GetString("COMMANDO_SPECIAL_NAME", currentLanguage);
            // stunGrenadeLocalize = Language.GetString("ITEM_STUNCHANCEONHIT_NAME", currentLanguage);

            var bodyCategory = "Jellyfish";
            var skillCategory = "Discharge Skill";

            JellyfishBaseDamage = Config.Bind(bodyCategory, bodyBaseDamage, 10f, string.Empty);
            JellyfishLevelDamage = Config.Bind(bodyCategory, bodyLevelDamage, 2f, string.Empty);
            JellyfishLoreChange = Config.Bind(bodyCategory, loreOverride, true, loreOverrideDesc);
            JellyfishHitstun = Config.Bind(bodyCategory, allowHitstun, false, allowHitstunDesc);
            JellyfishStun = Config.Bind(bodyCategory, allowStun, false, allowStunDesc);
            JellyfishCollision = Config.Bind(bodyCategory, allowCollision, false, allowCollisionDesc);
            JellyfishKnockback = Config.Bind(bodyCategory, allowKnockback, false, string.Empty);

            JellyfishDischargeDamageCoefficient = Config.Bind(skillCategory, skillNameDamageCoefficient, 1f, string.Empty);
            JellyfishDischargeCanShock = Config.Bind(skillCategory, "Shocks", false, "If true, then the skill applies Captain's shock damage type. Note that this won't affect Players.");
            JellyfishDischargeCanCrit = Config.Bind(skillCategory, "Can Crit", true, "If true, then the skill can roll to crit.");
        }

        private static void SetupBody()
        {
            myCharacter.GetComponent<SetStateOnHurt>().canBeHitStunned = JellyfishHitstun.Value;
            myCharacter.GetComponent<SetStateOnHurt>().canBeStunned = JellyfishStun.Value;
            myCharacter.GetComponent<SphereCollider>().enabled = JellyfishCollision.Value;
            if (!JellyfishKnockback.Value) myCharacter.GetComponent<Rigidbody>().mass = 99999999;

            var characterBody = myCharacter.GetComponent<CharacterBody>();
            characterBody.baseDamage = JellyfishBaseDamage.Value;
            characterBody.levelDamage = JellyfishLevelDamage.Value;

            //myCharacter.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(typeof(JellyfishFixedDeathState));
        }

        private static void SetupLanguage()
        {
            if (JellyfishLoreChange.Value)
            {
                // Lore Entry
                var lorestring = "JELLYFISH_BODY_LORE";
                var lore_en = "Field Notes:  An airborne creature, capable of flight using a combination of gases in its clear hull. Like the Jellyfish on earth, they also use pulsation to aid in locomotion; however, rather than a series of tentacles they have two 'branches' made of many tentacles wrapped around themselves." +
                    "\n\nAlso like the Jellyfish, they have quite the sting, capable of penetrating my weather shielding. The same gases used for flight are used to create a very powerful electrostatic charge." +
                    "\n\nWhen they are not busy hunting me, the Jellyfish have been seen sunbathing and absorbing the strange fumes from the ground.";
                var lore_es = "Notas de campo:  Una criatura aérea, capaz de volar utilizando una combinación de gases en su casco transparente. Al igual que las medusas de la Tierra, también utilizan la pulsación para ayudarse en la locomoción; sin embargo, en lugar de una serie de tentáculos, tienen dos \"ramas\" formadas por muchos tentáculos envueltos alrededor de sí mismos." +
                    "\n\nTambién, al igual que las medusas, tienen un aguijón bastante potente, capaz de penetrar mi escudo meteorológico. Los mismos gases que utilizan para volar sirven para crear una carga electrostática muy potente." +
                    "\n\nCuando no están ocupadas cazándome, se ha visto a las medusas tomando el sol y absorbiendo los extraños gases del suelo. Traducción realizada con la versión gratuita del traductor";
                var lore_jp = "フィールドノート。 透明な船体の中にあるガスの組み合わせで飛行することができる空中生物。地球上のクラゲと同様に脈動を利用して移動するが、一連の触手ではなく、多くの触手を巻き付けた2本の「枝」を持つ。" +
                    "\n\nまた、クラゲのようにかなりの刺があり、私の遮蔽物を貫通することができる。飛行に使われるのと同じガスを使って、非常に強力な静電気を発生させます。" +
                    "\n\nクラゲは私を狩るのに忙しくないときは、日光浴をしたり、地面から奇妙なガスを吸収したりしているのが目撃されている。";
                var lore_ru = "Полевые заметки:  Воздушное существо, способное летать с помощью комбинации газов в своем прозрачном корпусе. Как и земные медузы, они также используют пульсацию для передвижения; однако вместо ряда щупалец у них есть два \"отростка\", состоящих из множества щупалец, обернутых вокруг себя." +
                    "\n\nКак и у медуз, у них довольно сильное жало, способное пробить мою защиту от непогоды. Те же газы, которые используются для полета, используются для создания очень мощного электростатического заряда." +
                    "\n\n\nКогда они не заняты охотой на меня, Медуз видели загорающими и поглощающими странные испарения с земли.";

                LanguageAPI.Add(lorestring, lore_en, "en");
                LanguageAPI.Add(lorestring, lore_es, "es-419");
                LanguageAPI.Add(lorestring, lore_jp, "ja");
                LanguageAPI.Add(lorestring, lore_ru, "RU");
            }

            // Skill
            var nameString = "DESTROYEDCLONE_JELLYFISHSHOCK_NAME";
            var descString = "DESTROYEDCLONE_JELLYFISHSHOCK_DESCRIPTION";
            var damage = JellyShockSkill.novaDamageCoefficient * 100;
            LanguageAPI.Add(nameString, "Discharge", "en");
            LanguageAPI.Add(descString, $"Shocks nearby enemies for <style=cIsDamage>{damage}% damage</style>.", "en");
            LanguageAPI.Add(nameString, "Descarga", "es-419");
            LanguageAPI.Add(descString, $"Golpea a los enemigos cercanos con <style=cIsDamage>{damage}% de daño</style>.", "es");
            LanguageAPI.Add(nameString, "放電", "ja");
            LanguageAPI.Add(descString, $"近くの敵にショックを与える<style=cIsDamage>{damage}％ダメージ</style>.", "ja");
            LanguageAPI.Add(nameString, "Разряд", "RU");
            LanguageAPI.Add(descString, $"Сотрясает ближайших врагов на <style=cIsDamage>{damage}% урона</style>.", "RU");
        }

        private static void SetupSkills()
        {
            var skillLocator = myCharacter.GetComponent<SkillLocator>();
            var skillFamily = skillLocator.secondary.skillFamily;
            var defaultSkillDef = skillFamily.variants[(int)skillFamily.defaultVariantIndex].skillDef;
            var mySkillDef = ScriptableObject.CreateInstance<SkillDef>();

            mySkillDef.activationState = new SerializableEntityStateType(typeof(JellyShockSkill));
            mySkillDef.activationStateMachineName = defaultSkillDef.activationStateMachineName;
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 1f;
            mySkillDef.beginSkillCooldownOnSkillEnd = defaultSkillDef.beginSkillCooldownOnSkillEnd;
            mySkillDef.canceledFromSprinting = defaultSkillDef.canceledFromSprinting;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = defaultSkillDef.interruptPriority;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            //mySkillDef.icon = Resources.Load<Sprite>("RoR2/Base/ShockNearby/texBuffTeslaIcon");
            mySkillDef.skillDescriptionToken = "DESTROYEDCLONE_JELLYFISHSHOCK_DESCRIPTION";
            mySkillDef.skillName = "DESTROYEDCLONE_JELLYFISHSHOCK_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            (mySkillDef as ScriptableObject).name = "JellyfishBodySecondaryDischarge";

            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.dontAllowPastMaxStocks = false;
            mySkillDef.forceSprintDuringState = false;
            mySkillDef.keywordTokens = new string[] { };
            mySkillDef.resetCooldownTimerOnUse = false;

            if (ContentAddition.AddSkillDef(mySkillDef))
            {
                _logger.LogMessage("Added Discharge SkillDef to ContentPack");
            }
            else
            {
                _logger.LogError("Failed to add Discharge SkillDef to ContentPack");
            }

            skillFamily.variants[(int)skillFamily.defaultVariantIndex].skillDef = mySkillDef;
        }
    }
}