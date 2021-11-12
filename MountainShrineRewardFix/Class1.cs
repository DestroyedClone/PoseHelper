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

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BossDropRewardDelay
{
    [BepInPlugin("com.DestroyedClone.BossDropRewardDelay", "Boss Drop Reward Delay", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> cfgSpawnDelay;

        public void Awake()
        {
            cfgSpawnDelay = Config.Bind("", "Delay Between Drops", 0.3f, "The amount of time, in seconds, between each drop.");

            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
        }

        private void BossGroup_DropRewards(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                 x => x.MatchStloc(5),
                 x => x.MatchLdcI4(0),
                x => x.MatchStloc(6)
            );
            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);    //self
            c.Emit(OpCodes.Ldloc_2);    //PickupIndex
            c.Emit(OpCodes.Ldloc, 4);    //vector
            c.Emit(OpCodes.Ldloc, 5);    //rotation
            c.Emit(OpCodes.Ldloc, 3);    //scaledRewardCount
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

            public List<PickupIndex> rewards = new List<PickupIndex>();

            public float age = 0;
            public float delay = 0.3f;

            public void Awake()
            {
                delay = cfgSpawnDelay.Value;
            }

            public void FixedUpdate()
            {
                // Stopwatch Check
                age += Time.fixedDeltaTime;
                if (age < delay)
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