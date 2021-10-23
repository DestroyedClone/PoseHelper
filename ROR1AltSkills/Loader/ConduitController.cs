using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ROR1AltSkills.Loader;
using RoR2;
using UnityEngine.Networking;

namespace ROR1AltSkills.Loader
{
    public class ConduitController : MonoBehaviour
    {
        public CharacterBody owner;

        public float damage = 0.8f;
        public float duration = 9f;
        public float tickFrequency = 1 / 60;
        private float age;

        public GameObject conduitA;
        public GameObject conduitB;

        private bool hasDealtDamage = false;

        private float damageStopwatch = 0f;
        public float damageIntervalLocal;

        BoxCollider boxCollider;

        public void Start()
        {
            if (!conduitA || !conduitB)
            {
                enabled = false;
                return;
            }
            ModifyBoxCollider();
        }

        public void ModifyBoxCollider()
        {
            boxCollider = gameObject.GetComponent<BoxCollider>();
            if (!boxCollider) boxCollider = gameObject.AddComponent<BoxCollider>();
            var distanceBetweenConduits = Vector3.Distance(conduitA.transform.position, conduitB.transform.position) / 2f;
            var direction = (conduitA.transform.position - conduitB.transform.position).normalized;
            boxCollider.center = (conduitA.transform.position + conduitB.transform.position) / 2f;
        }

        public void FixedUpdate()
        {
            age += Time.fixedDeltaTime;
            if (NetworkServer.active)
            {
                damageStopwatch += Time.fixedDeltaTime;
                if (damageStopwatch > damageIntervalLocal)
                {
                    damageStopwatch -= damageIntervalLocal;
                    TickDamage();
                }
            }

            if (age >= duration)
            {
                Destroy(this);
            }
        }

        public void TickDamage()
        {
            RaycastHit[] array = Physics.BoxCastAll(boxCollider.center, boxCollider.size / 2, Vector3.forward, Quaternion.identity, 5f, RoR2.LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);

            foreach (var hit in array)
            {
                if (hit.collider)
                {
                    var hc = hit.collider.GetComponent<HealthComponent>();
                    if (hc)
                    {
                        if (FriendlyFireManager.ShouldSplashHitProceed(hc, owner.teamComponent.teamIndex))
                        {
                            var damageInfo = new DamageInfo()
                            {
                                attacker = owner.gameObject,
                                inflictor = gameObject,
                                crit = owner.RollCrit(),
                                damage = damage,
                                damageColorIndex = DamageColorIndex.Item,
                                damageType = DamageType.SlowOnHit,
                                force = Vector3.zero,
                                position = hc.body ? hc.body.corePosition : hc.transform.position,
                                procChainMask = default,
                                procCoefficient = 0f,
                            };
                            hc.TakeDamage(damageInfo);
                        }
                    }
                }
            }
        }
    }
}
