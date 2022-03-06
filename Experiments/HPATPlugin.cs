using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Collections;
using System.Linq;
using RoR2.CharacterAI;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Events;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HighPriorityAggroTest
{
    [BepInPlugin("com.DestroyedClone.aggrotest", "Aggro Test", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class HPATPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
            On.RoR2.CharacterBody.Awake += CharacterBody_Awake;
            On.RoR2.CharacterAI.BaseAI.OnBodyDamaged += BaseAI_OnBodyDamaged;
        }

        private void BaseAI_OnBodyDamaged(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDamaged orig, BaseAI self, DamageReport damageReport)
        {
            if (self.currentEnemy.gameObject && self.currentEnemy.gameObject.GetComponent<PriorityAggroTargetForEnemy>())
            {
                Debug.Log(self.body.GetDisplayName() + " retaliate ignored b/c their target is someone else.");
                return;
            }
            orig(self, damageReport);
        }

        private void CharacterBody_Awake(On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self)
        {
            orig(self);
            if (self.name.StartsWith("Beet"))
            {
                var com = self.gameObject.AddComponent<PriorityAggroTargetForEnemy>();
                com.ownerCharacterBody = PlayerCharacterMasterController.instances[0].body;
                com.characterBody = self;
            }
        }

        private HurtBox BaseAI_FindEnemyHurtBox(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            var gameObject = self.body.gameObject;
            if (gameObject)
            {
                self.enemySearch.viewer = self.body;
                self.enemySearch.teamMaskFilter = TeamMask.allButNeutral;
                self.enemySearch.teamMaskFilter.RemoveTeam(self.master.teamIndex);
                self.enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
                self.enemySearch.minDistanceFilter = 0;
                self.enemySearch.maxDistanceFilter = maxDistance; //maxDistance
                self.enemySearch.searchOrigin = self.bodyInputBank.aimOrigin;
                self.enemySearch.searchDirection = self.bodyInputBank.aimDirection;
                self.enemySearch.maxAngleFilter = 180f; // (full360Vision ? 180f : 90f)
                self.enemySearch.filterByLoS = filterByLoS;
                self.enemySearch.RefreshCandidates();
                self.enemySearch.FilterOutGameObject(gameObject);
                var list = self.enemySearch.GetResults().ToList();

                foreach (HurtBox hurtBox in list)
                {
                    if (hurtBox.GetComponent<PriorityAggroTargetForEnemy>())
                    {
                        return hurtBox; //Chooses the first non-charmed target
                    }
                }
                return list.FirstOrDefault(); //and falls back if it can't

            }
            return orig(self, maxDistance, full360Vision, filterByLoS);
        }

        public class PriorityAggroTargetForEnemy : NetworkBehaviour
        {
            public CharacterBody ownerCharacterBody;
            public CharacterBody characterBody;
            public float calculatedRadius = 30f;
            public bool retarget = true;
            public float interval = 5f;
            public float stopwatch = 0f;
            public bool ignoreDamage = true;

            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    stopwatch -= Time.fixedDeltaTime;
                    if (stopwatch <= 0f)
                    {
                        stopwatch = interval;
                        float radiusSqr = calculatedRadius * calculatedRadius;
                        Vector3 position = transform.position;
                        GenerateAggro(TeamComponent.GetTeamMembers(TeamIndex.Monster), radiusSqr, position);
                    }
                }
            }

            public void GenerateAggro(IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition)
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                foreach (TeamComponent teamComponent in recipients)
                {
                    if ((teamComponent.transform.position - currentPosition).sqrMagnitude <= radiusSqr)
                    {
                        BaseAI component = teamComponent.body.master.GetComponent<BaseAI>();
                        if (component)
                        {
                            RetargetEnemy(component);
                        }
                    }
                }
            }

            public void RetargetEnemy(BaseAI baseAI)
            {
                baseAI.targetRefreshTimer = 0.5f;
                if (characterBody && characterBody.healthComponent)
                {
                    baseAI.currentEnemy.gameObject = characterBody.healthComponent.gameObject;
                    baseAI.currentEnemy.bestHurtBox = characterBody.mainHurtBox;
                }
                if (baseAI.currentEnemy.gameObject)
                {
                    baseAI.enemyAttention = baseAI.enemyAttentionDuration;
                }
                baseAI.BeginSkillDriver(baseAI.EvaluateSkillDrivers());
            }
        }
    }
}
