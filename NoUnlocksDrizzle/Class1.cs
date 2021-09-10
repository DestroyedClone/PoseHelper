using BepInEx;
using R2API;
using R2API.Utils;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace NoUnlocksDrizzle
{
    [BepInPlugin("com.DestroyedClone.NoUnlocksDrizzle", "No Unlocks on Drizzle", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(EffectAPI), nameof(PrefabAPI), nameof(NetworkingAPI))]
    public class Class1 : BaseUnityPlugin
    {


        public void Awake()
        {
            On.RoR2.Achievements.BaseAchievement.Grant += BaseAchievement_Grant;
            On.RoR2.Stats.StatManager.OnDamageDealt += StatManager_OnDamageDealt;
            On.RoR2.Stats.StatManager.OnCharacterDeath += StatManager_OnCharacterDeath;
            On.RoR2.Stats.StatManager.OnCharacterExecute += StatManager_OnCharacterExecute;
            On.RoR2.Stats.StatManager.OnCharacterHeal += StatManager_OnCharacterHeal;
            On.RoR2.Stats.StatManager.OnPlayerFirstCreatedServer += StatManager_OnPlayerFirstCreatedServer;
            On.RoR2.Stats.StatManager.OnServerGameOver += StatManager_OnServerGameOver;
            On.RoR2.Stats.StatManager.OnServerStageComplete += StatManager_OnServerStageComplete;
            On.RoR2.Stats.StatManager.OnServerStageBegin += StatManager_OnServerStageBegin;
            On.RoR2.Stats.StatManager.OnServerItemGiven += StatManager_OnServerItemGiven;
            On.RoR2.Stats.StatManager.ProcessEvents += StatManager_ProcessEvents;
            On.RoR2.Stats.StatManager.OnEquipmentActivated += StatManager_OnEquipmentActivated;
        }

        private void StatManager_OnEquipmentActivated(On.RoR2.Stats.StatManager.orig_OnEquipmentActivated orig, EquipmentSlot activator, EquipmentIndex equipmentIndex) { if (!Drizzle()) orig(activator, equipmentIndex); }

        private void StatManager_ProcessEvents(On.RoR2.Stats.StatManager.orig_ProcessEvents orig) { if (!Drizzle()) orig(); }

        private void StatManager_OnServerItemGiven(On.RoR2.Stats.StatManager.orig_OnServerItemGiven orig, Inventory inventory, ItemIndex itemIndex, int quantity) { if (!Drizzle()) orig(inventory, itemIndex, quantity); }

        private void StatManager_OnServerStageBegin(On.RoR2.Stats.StatManager.orig_OnServerStageBegin orig, Stage stage) { if (!Drizzle()) orig(stage); }

        private void StatManager_OnServerStageComplete(On.RoR2.Stats.StatManager.orig_OnServerStageComplete orig, Stage stage) { if (!Drizzle()) orig(stage); }

        private void StatManager_OnServerGameOver(On.RoR2.Stats.StatManager.orig_OnServerGameOver orig, Run run, GameEndingDef gameEndingDef) { if (!Drizzle()) orig(run, gameEndingDef); }

        private void StatManager_OnPlayerFirstCreatedServer(On.RoR2.Stats.StatManager.orig_OnPlayerFirstCreatedServer orig, Run run, PlayerCharacterMasterController playerCharacterMasterController) { if (!Drizzle()) orig(run, playerCharacterMasterController); }

        private void StatManager_OnCharacterHeal(On.RoR2.Stats.StatManager.orig_OnCharacterHeal orig, HealthComponent healthComponent, float amount) { if (!Drizzle()) orig(healthComponent, amount); }

        private void StatManager_OnCharacterExecute(On.RoR2.Stats.StatManager.orig_OnCharacterExecute orig, DamageReport damageReport, float executionHealthLost) { if (!Drizzle()) orig(damageReport, executionHealthLost); }

        private void StatManager_OnCharacterDeath(On.RoR2.Stats.StatManager.orig_OnCharacterDeath orig, DamageReport damageReport) { if (!Drizzle()) orig(damageReport); }

        private void StatManager_OnDamageDealt(On.RoR2.Stats.StatManager.orig_OnDamageDealt orig, DamageReport damageReport) {if (!Drizzle()) orig(damageReport);}

        private void BaseAchievement_Grant(On.RoR2.Achievements.BaseAchievement.orig_Grant orig, RoR2.Achievements.BaseAchievement self)
        {
            orig(self);
            if (Run.instance && Run.instance.selectedDifficulty == DifficultyIndex.Easy)
            {
                self.shouldGrant = true;
                self.owner.dirtyGrantsCount--;
            }
        }

        private bool Drizzle(){return Run.instance && Run.instance.selectedDifficulty == DifficultyIndex.Easy;}
    }
}
