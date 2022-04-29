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
using R2API.Utils;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BossDropRewardDelay
{
    [BepInPlugin("com.DestroyedClone.BossDropRewardDelay", "Boss Drop Reward Delay", "1.1.1")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> cfgSpawnDelay;
        public static float spawnDelay;

        public void Awake()
        {
            cfgSpawnDelay = Config.Bind("", "Delay Between Drops", 0.3f, "The amount of time, in seconds, between each drop.");
            spawnDelay = cfgSpawnDelay.Value;

            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;

            R2API.Utils.CommandHelper.AddToConsoleWhenReady();
        }

        private void BossGroup_DropRewards(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
               x => x.MatchStloc(3),
               x => x.MatchLdloc(6),    //Might not need _s
               x => x.MatchLdloc(2)
           );
            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);    //self
            c.Emit(OpCodes.Ldloc, 1);    //PickupIndex
            c.Emit(OpCodes.Ldloc, 3);    //vector
            c.Emit(OpCodes.Ldloc, 4);    //rotation
            c.Emit(OpCodes.Ldloc, 6);    //scaledRewardCount
            c.EmitDelegate<Func<int, BossGroup, PickupIndex, Vector3, Quaternion, int, int>>((val, self, pickupIndex, vector, rotation, scaledRewardCount) =>
            {
                if (self && !self.GetComponent<DelayedBossRewardsSOTV>())
                {
                    var component = self.gameObject.AddComponent<DelayedBossRewardsSOTV>();
                    component.rng = self.rng;
                    component.scaledRewardCount = scaledRewardCount;
                    component.pickupIndex = pickupIndex;
                    component.bossDrops = self.bossDrops;
                    component.bossDropTables = self.bossDropTables;
                    component.bossDropChance = self.bossDropChance;
                    component.dropPosition = self.dropPosition;
                    component.vector = vector;
                    component.rotation = rotation;
                }
                return int.MaxValue;    //Prevent vanilla code from being run
            });
        }

        public class DelayedBossRewardsSOTV : MonoBehaviour
        {
            private int i = 0;

            public Xoroshiro128Plus rng;
            public int scaledRewardCount = 0;
            public PickupIndex pickupIndex;
            public List<PickupIndex> bossDrops;
            public List<PickupDropTable> bossDropTables;
            public float bossDropChance;
            public Transform dropPosition;
            public Vector3 vector;
            public Quaternion rotation;

            public float age = 0;
            public float delay = 0.3f;

            public void Awake()
            {
                delay = spawnDelay;
            }

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
                if (age < delay)
                {
                    return;
                }
                if (i < scaledRewardCount)
                {
                    PickupIndex pickupIndex2 = pickupIndex;
                    if ((bossDrops.Count > 0 || bossDropTables.Count > 0) && rng.nextNormalizedFloat <= bossDropChance)
                    {
                        if (bossDropTables.Count > 0)
                        {
                            pickupIndex2 = rng.NextElementUniform<PickupDropTable>(bossDropTables).GenerateDrop(rng);
                        }
                        else
                        {
                            pickupIndex2 = rng.NextElementUniform<PickupIndex>(bossDrops);
                        }
                    }
                    PickupDropletController.CreatePickupDroplet(pickupIndex2, dropPosition.position, vector);
                    i++;
                    vector = rotation * vector;
                    age = 0;
                } else
                {
                    enabled = false;
                }
            }
        }

        [ConCommand(commandName = "bossdrop_delay", flags = ConVarFlags.SenderMustBeServer, helpText = "bossdrop_delay {seconds}.")]
        private static void CCUpdateDelay(ConCommandArgs args)
        {
            if (args.Count > 0)
            {
                var newValue = args.GetArgFloat(0);
                if (newValue < 0)
                {
                    Debug.LogWarning("[BossDropRewardDelay] Can't set delay to less than 0!");
                }
                else
                {
                    var maxValueWarning = 5f;
                    if (newValue > maxValueWarning)
                    {
                        Debug.LogWarning($"[BossDropRewardDelay] Warning: reward delay set to larger than {maxValueWarning} seconds ({newValue}), rewards may take a long time to complete!");
                    }
                    spawnDelay = newValue;
                    foreach (var bossDropRewardDelayComponent in InstanceTracker.GetInstancesList<DelayedBossRewardsSOTV>())
                    {
                        if (bossDropRewardDelayComponent)
                        {
                            bossDropRewardDelayComponent.delay = spawnDelay;
                            bossDropRewardDelayComponent.age = 0;
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"[BossDropRewardDelay] {spawnDelay} seconds.");
            }
        }
    }
}