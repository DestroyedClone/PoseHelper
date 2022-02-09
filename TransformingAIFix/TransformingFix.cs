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
    [BepInPlugin("com.DestroyedClone.TransformingAIFix", "Transforming AI Fix", "1.0.0")]
    public class TransformingAIFixPlugin : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.CharacterMaster.TransformBody += CharacterMaster_TransformBody;
        }

        private void CharacterMaster_TransformBody(On.RoR2.CharacterMaster.orig_TransformBody orig, RoR2.CharacterMaster self, string bodyName)
        {
            var baseAI = self.GetComponent<BaseAI>();
            if (baseAI)
            {
                //Chat.AddMessage($"baseAI found");
                var masterPrefab = MasterCatalog.FindMasterPrefab(bodyName);
                if (masterPrefab)
                {
                    //Chat.AddMessage($"1");
                    ReplaceSkillDrivers(self, baseAI, masterPrefab);
                }
                if (!masterPrefab)
                {
                    var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
                    if (bodyPrefab)
                    {
                        var masterIndex = MasterCatalog.FindAiMasterIndexForBody(bodyPrefab.GetComponent<CharacterBody>().bodyIndex);
                        masterPrefab = MasterCatalog.GetMasterPrefab(masterIndex);
                        if (masterPrefab)
                        {
                            //Chat.AddMessage($"2");
                            ReplaceSkillDrivers(self, baseAI, masterPrefab);
                        }
                    }
                }
            }
            orig(self, bodyName);
        }

        private static void ReplaceSkillDrivers(CharacterMaster characterMaster, BaseAI baseAI, GameObject newCharacterMasterPrefab)
        {
            //Chat.AddMessage($"{characterMaster.name} has transformed into {newCharacterMasterPrefab.name}");
            foreach (var skillDriver in characterMaster.GetComponents<AISkillDriver>())
            {
                Destroy(skillDriver);
            }

            var listOfPrefabDrivers = newCharacterMasterPrefab.GetComponents<AISkillDriver>();

            List<AISkillDriver> newSkillDrivers = new List<AISkillDriver>();
            foreach (var skillDriver in listOfPrefabDrivers)
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
            baseAI.skillDrivers = array;

            var esm = characterMaster.GetComponent<EntityStateMachine>();
            var customESM = newCharacterMasterPrefab.GetComponent<EntityStateMachine>();
            esm.customName = customESM.customName;
            esm.initialStateType = customESM.initialStateType;
            esm.mainStateType = customESM.mainStateType;
            esm.nextState = customESM.nextState;
        }
    }
}