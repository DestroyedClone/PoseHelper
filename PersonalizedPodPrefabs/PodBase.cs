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
        public abstract void Init(ConfigFile config);
    }
}
