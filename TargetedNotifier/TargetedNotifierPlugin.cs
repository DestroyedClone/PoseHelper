using System;
using BepInEx;
using R2API.Utils;
using UnityEngine;
using RoR2;
using System.Collections.Generic;
using RoR2.CharacterAI;


namespace TargetedNotifier
{
    [BepInPlugin("com.DestroyedClone.TargetedNotifier", "Targeted Notifier", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class TargetedNotifierPlugin : BaseUnityPlugin
    {
        public static GameObject AlertedDisplay = Resources.Load<GameObject>("prefabs/effects/DamageRejected");
        public static List<BaseAI> NoticedEnemiesList = new List<BaseAI>();

        public void Awake()
        {
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
            On.RoR2.CharacterAI.BaseAI.OnDestroy += BaseAI_OnDestroy;
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
        }

        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);
            // show sprite of targeted enemy
            if (NoticedEnemiesList.Count == 0) return;
            foreach (var enemy in NoticedEnemiesList)
            {
                EffectManager.SimpleEffect(AlertedDisplay, enemy.body.corePosition + Vector3.up * 1f, Quaternion.identity, false);
            }
        }

        private void BaseAI_OnDestroy(On.RoR2.CharacterAI.BaseAI.orig_OnDestroy orig, BaseAI self)
        {
            NoticedEnemiesList.Remove(self);
            orig(self);
        }

        private HurtBox BaseAI_FindEnemyHurtBox(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, RoR2.CharacterAI.BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            var original = orig(self, maxDistance, full360Vision, filterByLoS);

            if (original != null)
            {
                if (original.healthComponent == LocalUserManager.readOnlyLocalUsersList[0].cachedBody.healthComponent)
                {
                    NoticedEnemiesList.Add(self);
                }
                else
                {
                    NoticedEnemiesList.Remove(self);
                }
            }

            return original;


        }
    }
}
