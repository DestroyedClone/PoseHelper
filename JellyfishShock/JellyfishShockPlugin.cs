using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace JellyfishShock
{
    [BepInPlugin("com.DestroyedClone.OriginalJellyfish", "Original Jellyfish", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI))]
    public class JellyfishShockPlugin : BaseUnityPlugin
    {
        public static GameObject myCharacter = Resources.Load<GameObject>("prefabs/characterbodies/JellyfishBody");
        public static BodyIndex bodyIndex = myCharacter.GetComponent<CharacterBody>().bodyIndex;

        public static ConfigEntry<float> JellyfishBaseDamage;
        public static ConfigEntry<float> JellyfishLevelDamage;
        public static ConfigEntry<float> JellyfishDischargeDamageCoefficient;
        public static ConfigEntry<bool> JellyfishLoreChange;

        public void Awake()
        {
            Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
        }

        private void Language_onCurrentLanguageChanged()
        {
            SetupConfig();
            SetupLanguage();
            SetupBody();
            SetupSkills();
            Language.onCurrentLanguageChanged -= Language_onCurrentLanguageChanged;
        }

        public void SetupConfig()
        {
            var bodyBaseDamage = "Base Damage";
            var bodyLevelDamage = "Level Damage";
            var skillNameDamageCoefficient = "Damage Coefficient";
            var loreOverride = "Lore Override";
            var loreOverrideDesc = "If true, replaces the lore with the one from \"Risk of Rain 1\"";

            var localizeKey = "Current Config Language:";
            var localizeDesc = "If you want this config to become localized:" +
                "\n1. In-game, select your language to either: English, Spanish, Japanese, or Russian." +
                "\n2. Close the game, and delete your config file for this mod." +
                "\n3. Start the game.";

            var currentLanguage = Language.currentLanguageName;

            switch (currentLanguage)
            {
                case "es-419":
                    localizeKey = "Idioma de configuración actual:";
                    localizeDesc = "Si quieres que esta configuración se localice:" +
                        "\n1. En el juego, seleccione su idioma a cualquiera: Inglés, español, japonés o ruso" +
                        "\n2. Cierra el juego, y borra tu archivo de configuración para este mod" +
                        "\n3. Inicie el juego";
                    bodyBaseDamage = "Daño de base";
                    bodyLevelDamage = "Daño por nivel";
                    skillNameDamageCoefficient = "Coeficiente de daños";
                    loreOverride = "Anulación de Lore";
                    loreOverrideDesc = "Si es cierto, sustituye el lore por el de \"Risk of Rain 1\"";
                    break;
                case "ja":
                    localizeKey = "現在の設定言語。";
                    localizeDesc = "このコンフィグを翻訳させたい場合には" +
                        "\n1. ゲーム内では、言語を次のいずれかに選択します。英語」「スペイン語」「日本語」「ロシア語」のいずれかを選択します。" +
                        "\n2. ゲームを終了して、このMODの設定ファイルを削除してください。" +
                        "\n3. ゲームを開始する。";
                    bodyBaseDamage = "ベースダメージ";
                    bodyLevelDamage = "レベルダメージ";
                    skillNameDamageCoefficient = "ダメージ係数";
                    loreOverride = "伝承の上書き";
                    loreOverrideDesc = "本当ならば、「Risk of Rain 1」に登場する伝承者と入れ替わる。";
                    break;
                case "RU":
                    localizeKey = "Текущий язык конфигурации:";
                    localizeDesc = "Если вы хотите, чтобы этот конфиг стал локализованным:" +
                        "\n1. В игре выберите язык: Английский, Испанский, Японский или Русский." +
                        "\n2. Закройте игру и удалите ваш файл конфигурации для этого мода." +
                        "\n3. Запустите игру.";
                    bodyBaseDamage = "Базовое повреждение";
                    bodyLevelDamage = "Уровневое повреждение";
                    skillNameDamageCoefficient = "Коэффициент повреждения";
                    loreOverride = "Переопределение знаний";
                    loreOverrideDesc = "Если верно, заменяет историю на историю из \"Risk of Rain 1\".";
                    break;
            }

            Config.Bind("0", localizeKey, $"{Language.currentLanguage.selfName}", localizeDesc);
            JellyfishBaseDamage = Config.Bind(string.Empty, bodyBaseDamage, 10f, string.Empty);
            JellyfishLevelDamage = Config.Bind(string.Empty, bodyLevelDamage, 1.5f, string.Empty);
            JellyfishDischargeDamageCoefficient = Config.Bind(string.Empty, skillNameDamageCoefficient, 1f, string.Empty);
            JellyfishLoreChange = Config.Bind(string.Empty, loreOverride, true, loreOverrideDesc);
        }

        private static void SetupBody()
        {
            myCharacter.GetComponent<SetStateOnHurt>().canBeHitStunned = false;
            myCharacter.GetComponent<SetStateOnHurt>().canBeStunned = false;
            myCharacter.GetComponent<SphereCollider>().enabled = false;
            myCharacter.GetComponent<Rigidbody>().mass = 999999f;

            var characterBody = myCharacter.GetComponent<CharacterBody>();
            characterBody.baseDamage = JellyfishBaseDamage.Value;
            characterBody.levelDamage = JellyfishLevelDamage.Value;

            myCharacter.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(typeof(JellyfishFixedDeathState));
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

                // Skill
                var nameString = "DESTROYEDCLONE_JELLYFISHSHOCK_NAME";
                var descString = "DESTROYEDCLONE_JELLYFISHSHOCK_DESCRIPTION";
                var damage = JellyShockSkill.novaDamageCoefficient * 100;
                LanguageAPI.Add(nameString, "Discharge", "en");
                LanguageAPI.Add(descString, $"Shocks nearby enemies for <style=cIsDamage>{damage}% damage</style>.", "en");
                LanguageAPI.Add(nameString, "Descarga", "en");
                LanguageAPI.Add(descString, $"Golpea a los enemigos cercanos con <style=cIsDamage>{damage}% de daño</style>.", "es");
                LanguageAPI.Add(nameString, "放電", "ja");
                LanguageAPI.Add(descString, $"近くの敵にショックを与える<style=cIsDamage>{damage}％ダメージ</style>.", "ja");
                LanguageAPI.Add(nameString, "Разряд", "RU");
                LanguageAPI.Add(descString, $"Сотрясает ближайших врагов на <style=cIsDamage>{damage}% урона</style>.", "RU");
            }
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
            mySkillDef.icon = Resources.Load<Sprite>("textures/bufficons/texBuffLunarShellIcon");
            mySkillDef.skillDescriptionToken = "DESTROYEDCLONE_JELLYFISHSHOCK_DESCRIPTION";
            mySkillDef.skillName = "DESTROYEDCLONE_JELLYFISHSHOCK_NAME";
            mySkillDef.skillNameToken = mySkillDef.skillName;

            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.dontAllowPastMaxStocks = false;
            mySkillDef.forceSprintDuringState = false;
            mySkillDef.keywordTokens = new string[] { };
            mySkillDef.resetCooldownTimerOnUse = false;

            LoadoutAPI.AddSkillDef(mySkillDef);

            skillFamily.variants[(int)skillFamily.defaultVariantIndex].skillDef = mySkillDef;
        }
    }
}