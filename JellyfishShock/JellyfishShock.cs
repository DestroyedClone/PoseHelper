using EntityStates;
using EntityStates.JellyfishMonster;
using RoR2;
using UnityEngine;

namespace JellyfishShock
{
    public class JellyShockSkill : BaseState
    {
        public JellyShockSkill()
        {
            novaDamageCoefficient = JellyfishShockPlugin.JellyfishDischargeDamageCoefficient.Value;
            chargingEffectPrefab = JellyNova.chargingEffectPrefab;
            novaEffectPrefab = JellyNova.novaEffectPrefab;
            chargingSoundString = ShockState.enterSoundString;
            novaSoundString = JellyNova.novaSoundString;
            novaRadius = JellyNova.novaRadius * 0.8f;
            novaForce = 0;
        }

        public static float baseDuration = 0.25f;
        public static GameObject chargingEffectPrefab;
        public static GameObject novaEffectPrefab;
        public static string chargingSoundString;
        public static string novaSoundString;
        public static float novaDamageCoefficient = 1f;
        public static float novaRadius;
        public static float novaForce;
        private bool hasExploded;
        private float duration;
        private float stopwatch;
        private GameObject chargeEffect;
        private PrintController printController;
        private uint soundID;

        public override void OnEnter()
        {
            base.OnEnter();
            this.stopwatch = 0f;
            this.duration = JellyShockSkill.baseDuration / this.attackSpeedStat;
            Transform modelTransform = base.GetModelTransform();
            base.PlayCrossfade("Body", "Nova", "Nova.playbackRate", this.duration, 0.1f);
            this.soundID = Util.PlaySound(JellyShockSkill.chargingSoundString, base.gameObject);
            if (JellyShockSkill.chargingEffectPrefab)
            {
                this.chargeEffect = UnityEngine.Object.Instantiate<GameObject>(JellyShockSkill.chargingEffectPrefab, base.transform.position, base.transform.rotation);
                this.chargeEffect.transform.parent = base.transform;
                this.chargeEffect.transform.localScale = new Vector3(JellyShockSkill.novaRadius, JellyShockSkill.novaRadius, JellyShockSkill.novaRadius);
                this.chargeEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = this.duration;
            }
            if (modelTransform)
            {
                this.printController = modelTransform.GetComponent<PrintController>();
                if (this.printController)
                {
                    this.printController.enabled = true;
                    this.printController.printTime = this.duration;
                }
            }
        }

        // Token: 0x0600409B RID: 16539 RVA: 0x001007D0 File Offset: 0x000FE9D0
        public override void OnExit()
        {
            base.OnExit();
            AkSoundEngine.StopPlayingID(this.soundID);
            if (this.chargeEffect)
            {
                EntityState.Destroy(this.chargeEffect);
            }
            if (this.printController)
            {
                this.printController.enabled = false;
            }
        }

        // Token: 0x0600409C RID: 16540 RVA: 0x0010081F File Offset: 0x000FEA1F
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            if (this.stopwatch >= this.duration && base.isAuthority && !this.hasExploded)
            {
                this.Detonate();
                this.outer.SetNextStateToMain();
                return;
            }
        }

        // Token: 0x0600409D RID: 16541 RVA: 0x00100860 File Offset: 0x000FEA60
        private void Detonate()
        {
            this.hasExploded = true;
            Util.PlaySound(JellyShockSkill.novaSoundString, base.gameObject);
            if (this.chargeEffect)
            {
                EntityState.Destroy(this.chargeEffect);
            }
            if (JellyShockSkill.novaEffectPrefab)
            {
                EffectManager.SpawnEffect(JellyShockSkill.novaEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    scale = JellyShockSkill.novaRadius
                }, true);
            }
            new BlastAttack
            {
                attacker = base.gameObject,
                inflictor = base.gameObject,
                teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                baseDamage = this.damageStat * JellyShockSkill.novaDamageCoefficient,
                baseForce = 0f,
                bonusForce = Vector3.zero,
                position = base.transform.position,
                radius = JellyShockSkill.novaRadius,
                procCoefficient = 1f,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                crit = JellyfishShockPlugin.JellyfishDischargeCanCrit.Value && Util.CheckRoll(outer.commonComponents.characterBody.crit, outer.commonComponents.characterBody.master.luck),
                damageColorIndex = DamageColorIndex.Item,
                damageType = JellyfishShockPlugin.JellyfishDischargeCanShock.Value ? DamageType.Shock5s : DamageType.Generic,
                procChainMask = default
            }.Fire();
        }

        // Token: 0x0600409E RID: 16542 RVA: 0x0006E9B6 File Offset: 0x0006CBB6
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}