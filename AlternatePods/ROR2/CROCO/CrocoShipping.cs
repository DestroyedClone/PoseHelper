using R2API;
using RoR2;
using UnityEngine;

namespace AlternatePods
{
    public class CrocoShipping
    {
        public string TokenPrefix = "PODMOD";
        public string BodyName = "Croco";
        public string PodName = "Shipping";

        public string PodPrefab = $"{TokenPrefix}_{BodyName.ToUpper()}_{PodName.ToUpper()}";
        public string NameToken = PodPrefab+"_NAME";
        public string DescToken = PodPrefab+"_DESC";
        public GameObject PodPrefab;
        public GenericSkill genericSkill;

        public void SetupPod()
        {
            PodPrefab = PrefabAPI.InstantiateClone(Assets.genericPodPrefab, PodPrefab);
        }

        public void SetupSkill()
        {
            genericSkill = bodyPrefab.AddComponent<GenericSkill>();
        }
    }
}