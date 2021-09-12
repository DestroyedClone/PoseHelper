using System;
using BepInEx;
using R2API;
using UnityEngine;
using R2API.Utils;
using RoR2;
using EntityStates.Commando;
using System.Security;
using System.Security.Permissions;

using EntityStates.AI;
using static RoR2.RoR2Content;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace LobbySkillPreview
{
    [BepInPlugin("com.DestroyedClone.LobbySkillPreview", "Lobby Skill Preview", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class LobbySkillPreviewPlugin : BaseUnityPlugin
    {
        public static RuntimeAnimatorController lobbyController;
        public static RuntimeAnimatorController inGameController;

        public void Awake()
        {
            Debug.LogError("Warning, this mod is hardcoded, which means that new survivors have to be inputted manually.");

            CommandHelper.AddToConsoleWhenReady();

            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;

            //On.RoR2.UI.CharacterSelectController.SelectSurvivor += CharacterSelectController_SelectSurvivor;
            On.RoR2.UI.CharacterSelectController.Start += CharacterSelectController_Start;
        }

        private void CharacterSelectController_Start(On.RoR2.UI.CharacterSelectController.orig_Start orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);
            self.gameObject.AddComponent<SkillPreview>();
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            lobbyController = RoR2Content.Survivors.Commando.displayPrefab.GetComponentInChildren<Animator>().runtimeAnimatorController;
            inGameController = RoR2Content.Survivors.Commando.bodyPrefab.GetComponentInChildren<Animator>().runtimeAnimatorController;

            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private void CharacterSelectController_SelectSurvivor(On.RoR2.UI.CharacterSelectController.orig_SelectSurvivor orig, RoR2.UI.CharacterSelectController self, SurvivorIndex survivor)
        {
            orig(self, survivor);

            var skillPreview = UnityEngine.Object.FindObjectOfType<SkillPreview>();
            self.characterDisplayPads[0].displayInstance.transform.GetComponentInChildren<Animator>();
        }

        [ConCommand(commandName = "show_skill", flags = ConVarFlags.ExecuteOnServer, helpText = "path x y z")]
        public static void CMD_ShowSkill(ConCommandArgs args)
        {
            FindObjectOfType<SkillPreview>().PlayAnim(args.GetArgInt(0));
        }

        public class SkillPreview : MonoBehaviour
        {
            public Animator animator;
            public string skill1;
            bool canCount = true;

            float age;
            float duration;

            public void PlayAnim(int slot)
            {
                animator.runtimeAnimatorController = inGameController;
                age = 0;

                switch (slot)
                {
                    case 0:
                        duration = 1f;
                        PlayAnimation("Gesture Additive, Left", "FirePistol", "Left", duration);
                        PlayAnimation("Gesture Additive, Right", "FirePistol", "Right", duration);
                        break;
                    case 1:
                        duration = 2f;
                        PlayAnimation("Gesture, Additive", "FireFMJ", "FireFMJ.playbackRate", duration);
                        PlayAnimation("Gesture, Override", "FireFMJ", "FireFMJ.playbackRate", duration);
                        break;
                    case 2:
                        duration = SlideState.slideDuration;
                        PlayAnimation("Body", "SlideForward", "SlideForward.playbackRate", duration);
                        break;
                    case 3:
                        duration = 2f;
                        PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", duration);
                        PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", duration);
                        break;
                }
                Reset();
            }

            public void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration)
            {
                EntityStates.EntityState.PlayAnimationOnAnimator(animator, layerName, animationStateName, playbackRateParam, duration);
            }

            public void Update()
            {
                if (!animator)
                {
                    Debug.Log("Acquiring animator");
                    animator = FindObjectOfType<RoR2.UI.CharacterSelectController>().characterDisplayPads[0].displayInstance.transform.GetComponentInChildren<Animator>();
                    return;
                }

                if (canCount)
                {
                    age += Time.deltaTime;
                    if (age > duration)
                    {
                        animator.runtimeAnimatorController = lobbyController;
                        canCount = false;
                    }
                }
            }

            public void Reset()
            {
                canCount = true;
                age = 0;
            }
        }
    }
}
