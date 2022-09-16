using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AlternatePods
{
    public abstract class PodCharBase
    {
        public abstract string BodyName { get; }
        public abstract List<Object> Pods {get;}
        public virtual string ConfigCategory
        {
            get
            {
                return "Pod: " + BodyName;
            }
        }
        public virtual GenericSkill passiveSlot;

        public virtual void Init(ConfigFile config)
        {
            SetupConfig(config);
            CreatePassiveSlot();
        }

        public virtual void SetupConfig(ConfigFile config)
        {
        }

        public virtual void CreatePassiveSlot()
        {
            if (!BodyPrefab)
                BodyPrefab = LegacyResourcesAPI.Load<GameObject>($"prefabs/CharacterBodies/{CharacterName}Body");
        }
    }
}
