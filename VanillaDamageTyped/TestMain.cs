using BepInEx;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using UnityEngine.UI;
using TMPro;
using R2API;
using RoR2.UI;
using UnityEngine.Networking;
using static R2API.DamageAPI;
using RoR2.Stats;
using EntityStates.Headstompers;
using System.Text;
using EntityStates.VoidInfestor;
using EntityStates.GlobalSkills.LunarNeedle;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace VanillaDamageTyped
{
    [BepInPlugin("com.DestroyedClone.VanillaDamageTyped", "Vanilla Damage Type", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class Class1 : BaseUnityPlugin
    {
        public static StringBuilder stringBuilder = new StringBuilder();

        public void Start()
        {
            On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            //On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact += ProjectileImpactExplosion_OnProjectileImpact;
            //On.RoR2.Projectile.ProjectileExplosion.DetonateServer += ProjectileExplosion_DetonateServer;
            RoR2Application.onLoad += () =>
            {
            };
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.EntityStates.Headstompers.BaseHeadstompersState.OnEnter += BaseHeadstompersState_OnEnter;
            On.EntityStates.Headstompers.HeadstompersFall.DoStompExplosionAuthority += HeadstompersFall_DoStompExplosionAuthority;
            On.EntityStates.VoidInfestor.Infest.OnEnter += Infest_OnEnter;
            On.EntityStates.GlobalSkills.LunarNeedle.ChargeLunarSecondary.PlayChargeAnimation += ChargeLunarSecondary_PlayChargeAnimation;
            On.EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary.ModifyProjectile += ThrowLunarSecondary_ModifyProjectile;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            var voidDef = DLC1Content.Elites.Void;
            stringBuilder.Clear();
            stringBuilder.Append(voidDef.color);
            stringBuilder.AppendLine(voidDef.damageBoostCoefficient.ToString());
            stringBuilder.AppendLine(voidDef.healthBoostCoefficient.ToString());
            Logger.LogMessage(stringBuilder.ToString());
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
        }

        private void ThrowLunarSecondary_ModifyProjectile(On.EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary.orig_ModifyProjectile orig, ThrowLunarSecondary self, ref RoR2.Projectile.FireProjectileInfo projectileInfo)
        {
            orig(self, ref projectileInfo);
            stringBuilder.Clear();
            stringBuilder.Append($"self.minSpeed: {self.minSpeed}");
            stringBuilder.Append($"self.maxSpeed: {self.maxSpeed}");
            Logger.LogMessage(stringBuilder.ToString());
        }

        private void ChargeLunarSecondary_PlayChargeAnimation(On.EntityStates.GlobalSkills.LunarNeedle.ChargeLunarSecondary.orig_PlayChargeAnimation orig, ChargeLunarSecondary self)
        {
            stringBuilder.Clear();
            stringBuilder.Append($"self.baseDuration: {self.baseDuration}");
            Logger.LogMessage(stringBuilder.ToString());
            orig(self);
        }

        private void Infest_OnEnter(On.EntityStates.VoidInfestor.Infest.orig_OnEnter orig, EntityStates.VoidInfestor.Infest self)
        {
            orig(self);
            stringBuilder.Clear();
            stringBuilder.AppendLine("Void Infestor Info");
            stringBuilder.AppendLine($"damageCoefficinet: {Infest.infestDamageCoefficient}");
            stringBuilder.AppendLine($"Infest.searchMaxAngle {Infest.searchMaxAngle}");
            stringBuilder.AppendLine($"Infest.searchMaxDistance {Infest.searchMaxDistance}");
            stringBuilder.AppendLine($"Infest.velocityInitialSpeed {Infest.velocityInitialSpeed}");
            stringBuilder.AppendLine($"Infest.velocityTurnRate {Infest.velocityTurnRate}");
            Logger.LogMessage(stringBuilder.ToString());
        }

        private void HeadstompersFall_DoStompExplosionAuthority(On.EntityStates.Headstompers.HeadstompersFall.orig_DoStompExplosionAuthority orig, EntityStates.Headstompers.HeadstompersFall self)
        {
            if (self.body)
            {
                Inventory inventory = self.body.inventory;
                if ((inventory ? inventory.GetItemCount(RoR2Content.Items.FallBoots) : 1) > 0)
                {
                    self.bodyMotor.velocity = Vector3.zero;
                    float num = Mathf.Max(0f, self.initialY - self.body.footPosition.y);
                    if (num > 0f)
                    {
                        stringBuilder.Clear();

                        foreach (var distance in new float[] { 1f, HeadstompersFall.maxDistance/2, HeadstompersFall.maxDistance })
                        {
                            float lerpFraction = Mathf.InverseLerp(0f, HeadstompersFall.maxDistance, distance);
                            float damageCoefficient = Mathf.Lerp(HeadstompersFall.minimumDamageCoefficient, HeadstompersFall.maximumDamageCoefficient, lerpFraction);
                            float radiusCoefficient = Mathf.Lerp(HeadstompersFall.minimumRadius, HeadstompersFall.maximumRadius, lerpFraction);
                            stringBuilder.Append($"Distance: {distance}");
                            stringBuilder.AppendLine($"Radius: {radiusCoefficient}");
                            stringBuilder.AppendLine($"Base Force: 200 * {radiusCoefficient} = {200 * radiusCoefficient}");
                            stringBuilder.AppendLine($"baseDamage: self.body.damage * {damageCoefficient}");
                        }
                        Logger.LogMessage(stringBuilder.ToString());
                        EffectData effectData = new EffectData();
                        effectData.origin = self.body.footPosition;
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/BootShockwave"), effectData, true);
                    }
                }
            }
            self.SetOnHitGroundProviderAuthority(null);
            self.outer.SetNextState(new HeadstompersCooldown());
        }

        private void BaseHeadstompersState_OnEnter(On.EntityStates.Headstompers.BaseHeadstompersState.orig_OnEnter orig, EntityStates.Headstompers.BaseHeadstompersState self)
        {
            stringBuilder.Clear();
            if ((self is EntityStates.Headstompers.HeadstompersCharge charge))
            {
                stringBuilder.AppendLine($"input maxChargeDuration: {EntityStates.Headstompers.HeadstompersCharge.maxChargeDuration}");
                stringBuilder.AppendLine($"minVelocityY: {EntityStates.Headstompers.HeadstompersCharge.minVelocityY}");
                stringBuilder.AppendLine($"accelerationY: {EntityStates.Headstompers.HeadstompersCharge.accelerationY}");
            }
            else if (self is EntityStates.Headstompers.HeadstompersIdle idle)
            {
                stringBuilder.AppendLine($"HeadstompersIdle.inputConfirmationDelay: {EntityStates.Headstompers.HeadstompersIdle.inputConfirmationDelay}");
            }
            else if (self is HeadstompersFall fall)
            {
                stringBuilder.AppendLine($"HeadstompersFall.initialFallSpeed: {EntityStates.Headstompers.HeadstompersFall.initialFallSpeed}");
                stringBuilder.AppendLine($"HeadstompersFall.maxFallSpeed: {EntityStates.Headstompers.HeadstompersFall.maxFallSpeed}");
                stringBuilder.AppendLine($"HeadstompersFall.accelerationY: {EntityStates.Headstompers.HeadstompersFall.accelerationY}");
                stringBuilder.AppendLine($"HeadstompersFall.maxDistance: {HeadstompersFall.maxDistance}");
                stringBuilder.AppendLine($"HeadstompersFall.minimumDamageCoefficient : {HeadstompersFall.minimumDamageCoefficient}");
                stringBuilder.AppendLine($"HeadstompersFall.maximumDamageCoefficient : {HeadstompersFall.maximumDamageCoefficient}");
                stringBuilder.AppendLine($"HeadstompersFall.minimumRadius : {HeadstompersFall.minimumRadius}");
                stringBuilder.AppendLine($"HeadstompersFall.maximumRadius : {HeadstompersFall.maximumRadius}");
            }
            Logger.LogMessage(stringBuilder.ToString());
            orig(self);
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.isPlayerControlled)
            {
                obj.gameObject.AddComponent<HelperComp>();
            }
        }

        private void ProjectileExplosion_DetonateServer(On.RoR2.Projectile.ProjectileExplosion.orig_DetonateServer orig, RoR2.Projectile.ProjectileExplosion self)
        {
            orig(self);
            Chat.AddMessage($"1. {self.gameObject.name} Explosion size: {self.blastRadius}");
        }

        private void ProjectileImpactExplosion_OnProjectileImpact(On.RoR2.Projectile.ProjectileImpactExplosion.orig_OnProjectileImpact orig, RoR2.Projectile.ProjectileImpactExplosion self, RoR2.Projectile.ProjectileImpactInfo impactInfo)
        {
            orig(self, impactInfo);
            Chat.AddMessage($"2. {self.gameObject.name} Explosion size: {self.blastRadius}");
        }

        private void FireworkLauncher_FireMissile(On.RoR2.FireworkLauncher.orig_FireMissile orig, FireworkLauncher self)
        {
            orig(self);
            Chat.AddMessage($"C:{self.crit}, launchInterval:{self.launchInterval}");
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);

        }

        private class HelperComp : MonoBehaviour
        {
            public RoR2.Stats.StatSheet statSheet = null;
            public ulong healing = 0;
            public ulong prevValue = 0;

            public void Start()
            {
                RoR2.Stats.StatSheet statSheet = NetworkUser.readOnlyInstancesList[0].masterPlayerStatsComponent.currentStats;
            }

            public void FixedUpdate()
            {
                if (statSheet == null)
                {
                    statSheet = NetworkUser.readOnlyInstancesList[0].masterPlayerStatsComponent.currentStats;
                } else
                {
                    healing = statSheet.GetStatValueULong(RoR2.Stats.StatDef.totalHealthHealed);
                    if (prevValue != healing)
                    {
                        Chat.AddMessage($"Healing amount: {prevValue-healing}");
                    }
                    prevValue = healing;
                }
            }
        }
    }
}
