using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace VersusPlayerBoss
{
    public abstract class CustomBoss
    {
        public virtual string Name { get; } = "Unnamed Boss";
        public virtual string Subtitle { get; } = "Unnamed Boss";
        public virtual int InitialBossCount { get; } = 1;
        //public virtual string Name { get; } = "Unnamed Boss";


        protected void CreateBossGroup()
        {
            BossGroup bossGroup = new BossGroup()
            {
                shouldDisplayHealthBarOnHud = true,

                bestObservedName = Name,
                bestObservedSubtitle = Subtitle,
                bossMemoryCount = InitialBossCount
            };
        }
    }
}
