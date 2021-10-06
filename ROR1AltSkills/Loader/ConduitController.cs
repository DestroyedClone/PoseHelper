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
        public float damage = 0.8f;
        public float duration = 9f;
        public float tickFrequency = 1 / 60;
        private float age;

        public GameObject conduitA;
        public GameObject conduitB;

        private bool hasDealtDamage = false;

        private float damageStopwatch = 0f;
        public float damageIntervalLocal;

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
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (!boxCollider) boxCollider = gameObject.AddComponent<BoxCollider>();
            var distanceBetweenConduits = Vector3.Distance(conduitA.transform.position, conduitB.transform.position) / 2f;
            var direction = (conduitA.transform.position - conduitB.transform.position).normalized;
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

        }
    }
}
