using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BossDropRewardDelay
{
    [BepInPlugin(Guid, FormattedModName, Version)]
    [BepInDependency("com.rune580.riskofoptions")]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModName = "BossDropRewardDelay",
        FormattedModName = "Boss Drop Reward Delay",
        Author = "DestroyedClone",
        Guid = "com." + Author + "." + ModName,
        Version = "1.2.0";

        public static ConfigEntry<float> cfgSpawnDelay;
        public static float SpawnDelay => cfgSpawnDelay.Value;

        public void Awake()
        {
            cfgSpawnDelay = Config.Bind("General", "Delay Between Drops", 0.3f, "The amount of time, in seconds, between each drop.");

            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;

            ModSettingsManager.AddOption(new SliderOption(cfgSpawnDelay, new SliderConfig()
            {
                min = 0.2f,
                max = 4,
                formatString = "{0:0.0}s",
            }), Guid, FormattedModName);
        }

        private void BossGroup_DropRewards(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(6),
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(8)
            );
            c.Index += 3;
            c.Emit(OpCodes.Ldarg_0);    //self
            c.Emit(OpCodes.Ldloc_1);    //PickupIndex
            c.Emit(OpCodes.Ldloc, 3);    //vector
            c.Emit(OpCodes.Ldloc, 4);    //rotation
            c.Emit(OpCodes.Ldloc, 2);    //scaledRewardCount
            c.EmitDelegate<Func<int, BossGroup, PickupIndex, Vector3, Quaternion, int, int>>((val, self, pickupIndex, vector, rotation, scaledRewardCount) =>
            {
                if (self && !self.GetComponent<DelayedBossRewards>())
                {
                    var component = self.gameObject.AddComponent<DelayedBossRewards>();
                    component.rng = self.rng;
                    component.num = scaledRewardCount;
                    component.pickupIndex = pickupIndex;
                    component.bossDrops = self.bossDrops;
                    component.bossDropChance = self.bossDropChance;
                    component.dropPosition = self.dropPosition;
                    component.vector = vector;
                    component.rotation = rotation;
                }
                return int.MaxValue;    //Prevent vanilla code from being run
            });
        }

        public class DelayedBossRewards : MonoBehaviour
        {
            // Carry-overs
            public Xoroshiro128Plus rng;

            public List<PickupIndex> bossDrops;
            public float bossDropChance;
            public Transform dropPosition;

            private int i = 0;
            public PickupIndex pickupIndex;
            public int num = 1;
            public Quaternion rotation;
            public Vector3 vector;

            public float age = 0;

            public void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            public void OnDisable()
            {
                InstanceTracker.Remove(this);
            }

            public void FixedUpdate()
            {
                // Stopwatch Check
                age += Time.fixedDeltaTime;

                // allows config to be changed while the items are still dropping
                if (age < Plugin.SpawnDelay)
                {
                    return;
                }
                // Drop Count Check
                if (i < num)
                {
                    PickupIndex pickupIndex2 = pickupIndex;
                    if (bossDrops.Count > 0 && rng.nextNormalizedFloat <= bossDropChance)
                    {
                        pickupIndex2 = rng.NextElementUniform<PickupIndex>(bossDrops);
                    }
                    PickupDropletController.CreatePickupDroplet(pickupIndex2, dropPosition.position, vector);
                    i++;
                    vector = rotation * vector;
                    age = 0;
                }
                else
                {
                    enabled = false;
                }
            }
        }
    }
}