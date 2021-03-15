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
        public static void CacheDisplays()
        {
            foreach (var survivor in SurvivorCatalog.allSurvivorDefs)
            {
                var bodyPrefab = survivor.bodyPrefab;
                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                GameObject displayPrefab = survivorDef.displayPrefab;

                displayPrefab.transform.Find("");
            }
        }
    }
}
