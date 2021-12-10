using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PersonalizedPodPrefabs
{
    public abstract class PodBase
    {
        public abstract string BodyName { get; }
        public virtual string ConfigCategory
        {
            get
            {
                return "Pod: " + BodyName;
            }
        }
        public virtual string PodPrefabName
        {
            get
            {
                return BodyName + "PodPrefab";
            }
        }

        public virtual void AssignPodPrefab()
        {
            var survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(BodyCatalog.FindBodyPrefab(BodyName));
            survivorDef.bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab = CreatePod();
        }
        public virtual void Init(ConfigFile config)
        {
            AssignPodPrefab();
        }

        public virtual GameObject CreatePod()
        {
            PersonalizePodPlugin._logger.LogWarning($"PodBase found for {BodyName} but no modifications were done to it! Assigning Robocrate.");
            return PersonalizePodPlugin.roboCratePodPrefab;
        }
    }
}
