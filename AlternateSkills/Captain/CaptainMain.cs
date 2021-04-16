using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace AlternateSkills.Captain
{
    public class CaptainMain
    {
        public void Init()
        {
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.AddBuff += CharacterBody_AddBuff;
        }

        private void CharacterBody_AddBuff(On.RoR2.CharacterBody.orig_AddBuff orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            // a simple stat hook, adds armor after stats are recalculated
            if (self)
            {
                if (self.HasBuff(Buffs.acceleratedBuff))
                {
                    self.moveSpeed *= 9f;
                }
            }
        }
    }
}
