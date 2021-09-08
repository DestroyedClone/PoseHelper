using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using System.Security;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;


[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AlternateSkills
{
    [BepInPlugin("com.DestroyedClone.AlternateSkills", "Alternate Skills", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI), nameof(ProjectileAPI), nameof(DamageAPI))]

    public class MainPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            //Buffs.RegisterBuffs();
            //Acrid.AcridMain.Init();
            //Artificer.ArtificerMain.Init();
            //Bandit2.Bandit2Main.Init();
            Captain.CaptainMain.Init();
            //Commando.CommandoMain.Init();
            //Mercenary.MercenaryMain.Init();
            //Treebot.TreebotMain.Init();
        }

        public static BuffDef[] ReturnBuffs(CharacterBody characterBody, bool returnDebuffs, bool returnBuffs)
        {
            List<BuffDef> buffDefs = new List<BuffDef>();
            BuffIndex buffIndex = (BuffIndex)0;
            BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
            while (buffIndex < buffCount)
            {
                BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                if (characterBody.HasBuff(buffDef))
                {
                    if ((buffDef.isDebuff && returnDebuffs) || (!buffDef.isDebuff && returnBuffs))
                    {
                        buffDefs.Add(buffDef);
                    }
                }
                buffIndex++;
            }
            return buffDefs.ToArray();
        }


        // Aetherium: https://github.com/KomradeSpectre/AetheriumMod/blob/6f35f9d8c57f4b7fa14375f620518e7c904c8287/Aetherium/Items/AccursedPotion.cs#L344-L358
        public static void AddBuffAndDot(BuffDef buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != RoR2.DotController.DotIndex.None)
                {
                    RoR2.DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    body.AddTimedBuffAuthority(buff.buffIndex, duration);
                }
            }
        }
    }
}
