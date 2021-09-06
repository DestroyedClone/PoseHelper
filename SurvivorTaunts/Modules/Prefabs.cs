using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using R2API;

namespace SurvivorTaunts.Modules
{
    public static class Prefabs
    {
        public static List<GameObject> displayPrefabs = new List<GameObject>();
        public static List<RuntimeAnimatorController> runtimeAnimatorControllers = new List<RuntimeAnimatorController>();

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.SurvivorCatalog))]
        public static void CacheDisplays()
        {
            foreach (var survivor in SurvivorCatalog.allSurvivorDefs)
            {
                var bodyPrefab = survivor.bodyPrefab;
                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                if (survivorDef != null) continue;
                GameObject displayPrefab = survivorDef.displayPrefab;
                displayPrefabs.Add(displayPrefab);

                for (int i = 0; i < displayPrefab.transform.childCount; i++)
                {
                    var child = displayPrefab.transform.GetChild(i);
                    if (child.gameObject.name.Substring(0,3).ToLower() == "mdl")
                    {
                        runtimeAnimatorControllers.Add(child.GetComponent<Animator>().runtimeAnimatorController);
                    }
                }
            }
        }

        public static void GetAnimator()
        {

        }
    }
}
