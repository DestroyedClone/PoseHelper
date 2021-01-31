using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using static R2API.Utils.CommandHelper;
using R2API.Utils;
using RoR2;
using UnityEngine;
using EntityStates;
using UnityEngine.Networking;

namespace PoseHelper
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Console Command")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Empty Arg required")]
    public static class Commands
    {
        [ConCommand(commandName = "nocorpse", flags = ConVarFlags.ExecuteOnServer, helpText = "Nulls your deathstate, preventing ragdolls. Can not be reversed.")]
        private static void DeathStateClear(ConCommandArgs args)
        {
            var deathstate = args.senderBody.GetComponent<CharacterDeathBehavior>();
            if (deathstate) deathstate.deathState = new SerializableEntityStateType();
        }

        [ConCommand(commandName = "cloak", flags = ConVarFlags.ExecuteOnServer, helpText = "Toggles cloak.")]
        private static void GoDark(ConCommandArgs args)
        {
            var cb = args.senderBody;
            if (cb)
            {
                if (cb.HasBuff(BuffIndex.Cloak))
                    cb.RemoveBuff(BuffIndex.Cloak);
                else
                    cb.AddBuff(BuffIndex.Cloak);
            }
        }
        [ConCommand(commandName = "teleport", flags = ConVarFlags.ExecuteOnServer, helpText = "Teleport to specified coords. [x] [y] [z]")]
        private static void TeleportPos(ConCommandArgs args)
        {
            var cb = args.senderBody;
            var rbm = cb.GetComponent<RigidbodyMotor>();
            if (cb)
            {
                float[] array = { args.GetArgFloat(0), args.GetArgFloat(1), args.GetArgFloat(2) };
                var position = new Vector3(array[0], array[1], array[2]);
                if (cb.characterMotor)
                {
                    Debug.Log(string.Format("Teleported charactermotor to {0} {1} {2}", array[0], array[1], array[2]));
                    cb.characterMotor.Motor.SetPositionAndRotation(position, Quaternion.identity, true);
                }
                else if (rbm)
                {
                    Debug.Log(string.Format("Teleported rigidbody to {0} {1} {2}", array[0], array[1], array[2]));
                    rbm.rigid.position = position;
                }
            }
        }

        [ConCommand(commandName = "animator_speed", flags = ConVarFlags.ExecuteOnServer, helpText = "animator_speed [float]")]
        private static void AnimatorSpeed(ConCommandArgs args)
        {
            var cb = args.senderBody;

            if (cb)
            {
                var value = args.GetArgFloat(0);
                var animator = GetModelAnimator(cb);
                if (animator)
                {
                    if (!animator.enabled)
                    {
                        Debug.Log("Animator is not enabled.");
                    }
                    animator.speed = (float)value;
                }
            }
        }
        [ConCommand(commandName = "animator_toggle", flags = ConVarFlags.ExecuteOnServer, helpText = "animator_speed [float]")]
        private static void AnimatorToggle(ConCommandArgs args)
        {
            var cb = args.senderBody;

            if (cb)
            {
                var animator = GetModelAnimator(cb);
                if (animator)
                {
                    animator.enabled = true;
                    Debug.Log("Animator.enabled = "+ animator.enabled);
                }
            }
        }

        [ConCommand(commandName = "nextpose", flags = ConVarFlags.ExecuteOnServer, helpText = "finishpose [true/false]. true: kills the animator too.")]
        private static void FinishPose(ConCommandArgs args)
        {
            var cb = args.senderBody;
            if (cb)
            {
                var hc = cb.healthComponent;
                if (hc)
                {
                    if (args.TryGetArgBool(0) == true)
                    {
                        var animator = GetModelAnimator(cb);
                        if (animator)
                        {
                            animator.enabled = false;
                        }
                    } else
                    {
                        hc.Suicide();
                        args.senderMaster.Respawn(cb.footPosition, Quaternion.identity, false);
                    }
                }
            }
        }

        [ConCommand(commandName = "pp_doppel", flags = ConVarFlags.ExecuteOnServer, helpText = "Toggles doppelganger by spawning an umbra beetle.")]
        private static void PPDoppel(ConCommandArgs args) //borrowed code from debugtoolkit
        {
            var beetles = UnityEngine.Object.FindObjectsOfType<UmbraBeetle>();
            if (beetles.Length == 0)
            {
                GameObject body = BodyCatalog.FindBodyPrefab("BeetleBody");
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(body, args.sender.master.GetBody().transform.position, Quaternion.identity);
                gameObject.GetComponent<Inventory>().GiveItem(ItemIndex.InvadingDoppelganger);
                gameObject.AddComponent<UmbraBeetle>();
                NetworkServer.Spawn(gameObject);
                Debug.Log("Spawned umbra beetle");
            } else
            {
                UnityEngine.Object.DestroyImmediate(beetles[0].gameObject);
                Debug.Log("Destroyed umbra beetle");
            }
        }

        public static Animator GetModelAnimator(CharacterBody characterBody)
        {
            if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
            {
                return characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            }
            return null;
        }

        public class UmbraBeetle : MonoBehaviour
        {

        }

        public class DesCloneCommandComponent : MonoBehaviour
        {

        }
    }
}
