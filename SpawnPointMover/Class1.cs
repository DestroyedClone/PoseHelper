using BepInEx;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace SpawnPointMover
{
    [BepInPlugin("com.DestroyedClone.SpawnPointMover", "SpawnPointMover", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static Vector3 newPosition;
        public static List<SpawnPoint> SpawnPoints => SpawnPoint.instancesList;

        public void Start()
        {
            On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1_OnEnter;
        }

        private void Phase1_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
        {
            orig(self);
            if (!SceneInfo.instance)
            {
                return;
            }
            ChildLocator component = SceneInfo.instance.GetComponent<ChildLocator>();
            if (!component)
            {
                return;
            }
            if (MoonMissionController.instance)
            {
                Transform transform = component.FindChild("PlayerSpawnOrigin");
                transform.position = new Vector3(165.1803f, 497.2362f, 105.2121f);
                foreach (var spawnPoint in new List<SpawnPoint>(SpawnPoint.instancesList))
                {
                    Destroy(spawnPoint.gameObject);
                }
                MoonMissionController.instance.GeneratePlayerSpawnPointsServer();
            }
        }
    }
}