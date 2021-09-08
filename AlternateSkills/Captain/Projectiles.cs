using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using R2API;
using UnityEngine;
using RoR2.Projectile;

namespace AlternateSkills.Captain
{
    public static class Projectiles
    {
        public static GameObject nukeProjectile;
        public static GameObject irradiateProjectile;

        public static int nukeBlightStacks = 10;
        public static int nukeBlightDuration = 30;
        public static float nukeDamageCoefficient = 1000;

        public static void SetupProjectiles()
        {
            //projectileImpactExplosion.blastDamageCoefficient *= nukeDamageCoefficient;

            //var projectileStickOnImpact = nukeProjectile.AddComponent<ProjectileStickOnImpact>();
            //projectileStickOnImpact.ignoreCharacters = false;
            //projectileStickOnImpact.ignoreWorld = false;
            //projectileStickOnImpact.alignNormals = false;

            var syringeProjectile = Resources.Load<GameObject>("prefabs/projectiles/SyringeProjectile");
            irradiateProjectile = PrefabAPI.InstantiateClone(syringeProjectile, "CaptainScepterNukeIrradiate", true);
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<ProjectileSingleTargetImpact>());
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<SphereCollider>());
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<ProjectileSimple>());
            var nukeBehaviour = irradiateProjectile.AddComponent<NukeBehaviour>();
            nukeBehaviour.projectileController = irradiateProjectile.GetComponent<ProjectileController>();



            var baseProjectile = Resources.Load<GameObject>("prefabs/projectiles/CaptainAirstrikeAltProjectile");
            nukeProjectile = PrefabAPI.InstantiateClone(baseProjectile, "CaptainScepterNuke", true);

            var projectileImpactExplosion = nukeProjectile.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.blastRadius *= 100f;
            projectileImpactExplosion.fireChildren = true;
            projectileImpactExplosion.childrenCount = 1;
            projectileImpactExplosion.childrenProjectilePrefab = irradiateProjectile;


            ProjectileAPI.Add(irradiateProjectile);
            ProjectileAPI.Add(nukeProjectile);
        }

        public class NukeBehaviour : MonoBehaviour
        {
            public ProjectileController projectileController;
            bool hasIrradiated = false;
            public void Start()
            {
                if (!hasIrradiated)
                {
                    hasIrradiated = true;
                    Irradiate();
                    Destroy(gameObject);
                }
            }

            public void Irradiate()
            {
                Chat.AddMessage("Irradiating!");
                var blastAttack = new BlastAttack
                {
                    position = base.transform.position,
                    baseDamage = 0f,
                    baseForce = 0f,
                    radius = Mathf.Infinity,
                    attacker = (this.projectileController.owner ? this.projectileController.owner.gameObject : null),
                    inflictor = base.gameObject,
                    teamIndex = TeamIndex.None,
                    crit = false,
                    procChainMask = default,
                    procCoefficient = 0f,
                    bonusForce = Vector3.zero,
                    falloffModel = BlastAttack.FalloffModel.None,
                    damageColorIndex = DamageColorIndex.Poison,
                    damageType = DamageType.Stun1s,
                    attackerFiltering = AttackerFiltering.AlwaysHit,
                };
                blastAttack.AddModdedDamageType(DamageTypes.irradiateDamageType);
                blastAttack.Fire();
            }
        }

    }
}
