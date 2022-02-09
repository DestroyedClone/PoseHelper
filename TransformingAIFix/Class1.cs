using BepInEx;
using RoR2;
using RoR2.CharacterAI;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HereticAIFix
{
    [BepInPlugin("com.DestroyedClone.HeresyAIFix", "Heresy AI Fix", "1.0.0")]
    public class SurvivorsMod : BaseUnityPlugin
    {
        public static AISkillDriver[] hereticSkillDrivers;
        public static EntityStateMachine hereticESM;

        public void Start()
        {
            var hereticPrefab = Resources.Load<GameObject>("prefabs/charactermasters/hereticmonstermaster");
            hereticSkillDrivers = hereticPrefab.GetComponents<AISkillDriver>();
            hereticESM = hereticPrefab.GetComponent<EntityStateMachine>();
            On.RoR2.CharacterMaster.TransformBody += CharacterMaster_TransformBody;
            //On.RoR2.CharacterMaster.TransformBody += CharacterMaster_TransformBody1; //spawn
        }

        private void CharacterMaster_TransformBody1(On.RoR2.CharacterMaster.orig_TransformBody orig, CharacterMaster self, string bodyName)
        {
            if (bodyName == "HereticBody" && self.GetComponent<BaseAI>())
            {
                var newSummon = new MasterSummon()
                {
                    ignoreTeamMemberLimit = true,
                    inventoryToCopy = self.inventory,
                    masterPrefab = Resources.Load<GameObject>("prefabs/charactermasters/hereticmonstermaster"),
                    position = self.GetBody()?.footPosition ?? Vector3.zero,
                    useAmbientLevel = true,
                    teamIndexOverride = self.teamIndex
                };
                var newMaster = newSummon.Perform();

                return;
            }
            orig(self, bodyName);
            var body = self.GetBody();
            if (body)
            {
                TeleportHelper.TeleportBody(body, Vector3.zero);
                body.healthComponent?.Suicide();
            }
        }

        private void CharacterMaster_TransformBody(On.RoR2.CharacterMaster.orig_TransformBody orig, RoR2.CharacterMaster self, string bodyName)
        {
            if (bodyName == "HereticBody" && self.GetComponent<BaseAI>())
            {
                CopyFromHeretic(self);
            }
            orig(self, bodyName);
        }

        private static void CopyFromHeretic(CharacterMaster characterMaster)
        {
            foreach (var skillDriver in characterMaster.GetComponents<AISkillDriver>())
            {
                Destroy(skillDriver);
            }

            var characterAI = characterMaster.GetComponent<BaseAI>();
            List<AISkillDriver> newSkillDrivers = new List<AISkillDriver>();
            foreach (var skillDriver in hereticSkillDrivers)
            {
                var newDriver = characterMaster.gameObject.AddComponent<AISkillDriver>();
                newSkillDrivers.Add(newDriver);
                newDriver.activationRequiresAimConfirmation = skillDriver.activationRequiresAimConfirmation;
                newDriver.activationRequiresAimTargetLoS = skillDriver.activationRequiresAimTargetLoS;
                newDriver.activationRequiresTargetLoS = skillDriver.activationRequiresTargetLoS;
                newDriver.aimType = skillDriver.aimType;
                newDriver.buttonPressType = skillDriver.buttonPressType;
                newDriver.customName = skillDriver.customName;
                newDriver.driverUpdateTimerOverride = skillDriver.driverUpdateTimerOverride;
                newDriver.ignoreNodeGraph = skillDriver.ignoreNodeGraph;
                newDriver.maxDistance = skillDriver.maxDistance;
                newDriver.maxTargetHealthFraction = skillDriver.maxTargetHealthFraction;
                newDriver.maxUserHealthFraction = skillDriver.maxUserHealthFraction;
                newDriver.minDistance = skillDriver.minDistance;
                newDriver.minTargetHealthFraction = skillDriver.minTargetHealthFraction;
                newDriver.minUserHealthFraction = skillDriver.minUserHealthFraction;
                newDriver.moveInputScale = skillDriver.moveInputScale;
                newDriver.movementType = skillDriver.movementType;
                newDriver.moveTargetType = skillDriver.moveTargetType;
                //newDriver.name = skillDriver.name;
                newDriver.nextHighPriorityOverride = skillDriver.nextHighPriorityOverride;
                newDriver.noRepeat = skillDriver.noRepeat;
                newDriver.requiredSkill = skillDriver.requiredSkill;
                newDriver.requireEquipmentReady = skillDriver.requireEquipmentReady;
                newDriver.requireSkillReady = skillDriver.requireSkillReady;
                newDriver.resetCurrentEnemyOnNextDriverSelection = skillDriver.resetCurrentEnemyOnNextDriverSelection;
                newDriver.selectionRequiresAimTarget = skillDriver.selectionRequiresAimTarget;
                newDriver.selectionRequiresOnGround = skillDriver.selectionRequiresOnGround;
                newDriver.selectionRequiresTargetLoS = skillDriver.selectionRequiresTargetLoS;
                newDriver.shouldFireEquipment = skillDriver.shouldFireEquipment;
                newDriver.shouldSprint = skillDriver.shouldSprint;
                newDriver.shouldTapButton = skillDriver.shouldTapButton;
                newDriver.skillSlot = skillDriver.skillSlot;
            }
            var array = newSkillDrivers.ToArray();
            characterAI.skillDrivers = array;

            var esm = characterMaster.GetComponent<EntityStateMachine>();
            esm.customName = hereticESM.customName;
            esm.initialStateType = hereticESM.initialStateType;
            esm.mainStateType = hereticESM.mainStateType;
            esm.nextState = hereticESM.nextState;
        }

        public class ReplaceAIArray : MonoBehaviour
        {
            public BaseAI baseAI;
            public float age;

            public void Start()
            {
                if (!baseAI)
                {
                    baseAI = gameObject.GetComponent<BaseAI>();
                }
            }

            public void FixedUpdate()
            {
                //CopyFromHeretic(
            }
        }
    }
}