using BepInEx;
using R2API.Utils;
using R2API;
using RoR2;
using BepInEx.Configuration;
using UnityEngine;
using R2API.Networking;
using System.Security;
using System.Security.Permissions;
using static RoR2.Chat;
using EntityStates;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace MithrixMeme
{
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(
   nameof(LanguageAPI),
   nameof(ResourcesAPI),
   nameof(NetworkingAPI),
   nameof(EffectAPI)
   )]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MithrixMemePlugin : BaseUnityPlugin
    {
        public const string ModVer = "2.0.0";
        public const string ModName = "Honorable Mithrix";
        public const string ModGuid = "com.DestroyedClone.HonorableMithrix";
        public static ConfigEntry<string> HonoredItemString { get; set; }
        public static ItemIndex HonoredItem;

        public void Awake()
        {
            LanguageAPI.Add("MMP_MITHRIX_DIALOGUE_STYLE", "<color=#c6d5ff><size=120%> Mithrix: {0}</color></size>");
            HonoredItemString = Config.Bind("", "Honored Item", "Squid", "Name of item that mithrix bows down to, must follow ItemIndex names.");

            On.RoR2.ItemStealController.RpcOnStealFinishClient += ItemStealController_RpcOnStealFinishClient;
            On.RoR2.PickupCatalog.Init += PickupCatalog_Init;
        }

        private void PickupCatalog_Init(On.RoR2.PickupCatalog.orig_Init orig)
        {
            orig();
            HonoredItem = ItemCatalog.FindItemIndex(HonoredItemString.Value);
        }

        private void ItemStealController_RpcOnStealFinishClient(On.RoR2.ItemStealController.orig_RpcOnStealFinishClient orig, ItemStealController self)
        {
            orig(self);
            bool activate = false;

            foreach (var invInfo in self.stolenInventoryInfos)
            {
                if (invInfo.stolenItemStacks[(int)HonoredItem] > 0)
                {
                    activate = true;
                    break;
                }
            }

            if (activate)
            {
                var brother = GameObject.Find("BrotherHurtBody(Clone)");
                if (brother)
                {
                    var component = brother.AddComponent<MithrixKneel>();
                    component.characterBody = brother.GetComponent<CharacterBody>();
                    component.question = Language.GetString(ItemCatalog.GetItemDef(HonoredItem).nameToken);

                }
            }
        }

        public static void MithrixSay(string text)
        {
            Chat.SendBroadcastChat(new SimpleChatMessage
            {
                baseToken = "<color=#c6d5ff><size=120%>Mithrix: {0}</color></size>",
                paramTokens = new[] { text }
            });
        }

        public class MithrixKneel : MonoBehaviour
        {
            public CharacterBody characterBody;
            public string question;
            float stopwatch = 0f;
            bool firstLine = false;
            bool secondLine = false;
            bool thirdLine = false;

            public void Awake()
            {
                gameObject.GetComponent<SpeechBubbleController>().enabled = false;
                gameObject.GetComponent<CharacterDirection>().enabled = false;
                gameObject.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(typeof(NoAnimDeathState));
                var setState = gameObject.GetComponent<SetStateOnHurt>();
                setState.canBeFrozen = false;
                setState.canBeHitStunned = false;
                setState.canBeStunned = false;

                var genericSkills = gameObject.GetComponents<GenericSkill>();
                foreach (var generic in genericSkills)
                {
                    generic.enabled = false;
                }


                var collider = gameObject.GetComponent<CapsuleCollider>();
                collider.center = Vector3.down * 2f;
                collider.height = 1f;
            }


            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch < 3f)
                {
                    if (question == "" || question == null) question = "A gamer";
                    characterBody.moveSpeed = 0f;
                    characterBody.attackSpeed = 0f;

                    if (!firstLine)
                    {
                        MithrixSay(question + "?");
                        firstLine = true;
                    }
                }
                else if (stopwatch < 5f)
                {
                    if (!secondLine)
                    {
                        MithrixSay("I...");
                        secondLine = true;
                    }
                }
                else if (stopwatch < 6f)
                {
                    if (!thirdLine)
                    {
                        MithrixSay("I kneel.");
                        PlayDeathAnimation();
                        thirdLine = true;
                    }
                }
            }
            void PlayDeathAnimation()
            {
                EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
                if (entityStateMachine == null)
                {
                    return;
                }
                //entityStateMachine.SetState(EntityState.Instantiate(EntityStates.BrotherMonster.TrueDeathState));
                //characterBody.SetBodyStateToPreferredInitialState
                //entityStateMachine.SetInterruptState(EntityState.Instantiate(new SerializableEntityStateType(typeof(EntityStates.BrotherMonster.TrueDeathState))), InterruptPriority.Death);
                entityStateMachine.SetInterruptState(new KneelState(), InterruptPriority.Death);
                //base.PlayAnimation("FullBody Override", "TrueDeath");
                characterBody.moveSpeed = 0f;
                characterBody.characterDirection.moveVector = characterBody.characterDirection.forward;
            }
        }
    }
}
