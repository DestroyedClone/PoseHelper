
using RoR2;
using R2API;
using UnityEngine;
using RoR2.Projectile;
using System.Collections.Generic;

namespace ROR1AltSkills.Acrid
{
    public class PoisonSplatController : MonoBehaviour
    {
        public DestroyOnTimer destroyOnTimer;
        public ProjectileController projectileController;
        public ProjectileDotZone projectileDotZone;
        public int currentStacks = 1;
        public static int maxStacks = 15;

        public void Start()
        {
            FindNearby();
        }

        private void AddStack()
        {
            if (currentStacks < maxStacks)
            {
                currentStacks++;
                projectileDotZone.resetFrequency /= 2f;
            }
            destroyOnTimer.age = 0;
        }

        private void FindNearby()
        {
            Vector3 vector = gameObject.transform.position;
            float diameterDistance = 9 * 9;

            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            List<ProjectileController> list = new List<ProjectileController>();

            int num3 = 0;
            int count = instancesList.Count;
            while (num3 < count)
            {
                ProjectileController enemyProjectileController = instancesList[num3];
                if (enemyProjectileController.owner == projectileController.owner && (projectileController.transform.position - vector).sqrMagnitude < diameterDistance
                    && projectileController.gameObject.GetComponent<PoisonSplatController>())
                {
                    list.Add(projectileController);
                }
                num3++;
            }

            bool result = count > 0;

            int i = 0;
            int count2 = list.Count;
            while (i < count2)
            {
                ProjectileController projectileController2 = list[i];
                if (projectileController2)
                {
                    PoisonSplatController poisonSplatController = projectileController2.GetComponent<PoisonSplatController>();
                    if (poisonSplatController)
                    {
                        poisonSplatController.AddStack();
                    }
                }
                i++;
            }

            if (result)
            {
                Destroy(gameObject);
            }
        }
    }
    public class PoisonFallController : MonoBehaviour
    {
        public float distanceToCheck = 10f;


    }
}
