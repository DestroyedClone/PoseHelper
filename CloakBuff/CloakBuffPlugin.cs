using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Events;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace CloakBuff
{
    [BepInPlugin("com.DestroyedClone.CloakBuff", "CloakBuff", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class CloakBuffPlugin : BaseUnityPlugin
	{
		public static ConfigEntry<bool> DisableProximityMine { get; set; }

		public void Awake()
        {
            On.RoR2.UI.CombatHealthBarViewer.VictimIsValid += CombatHealthBarViewer_VictimIsValid;
            On.RoR2.Util.HandleCharacterPhysicsCastResults += Util_HandleCharacterPhysicsCastResults;

            //Projectile Stuff
            On.RoR2.Projectile.MineProximityDetonator.OnTriggerEnter += MineProximityDetonator_OnTriggerEnter;
		}

        private void MineProximityDetonator_OnTriggerEnter(On.RoR2.Projectile.MineProximityDetonator.orig_OnTriggerEnter orig, RoR2.Projectile.MineProximityDetonator self, Collider collider)
        {
			if (NetworkServer.active)
			{
				if (collider)
				{
					HurtBox component = collider.GetComponent<HurtBox>();
					if (component)
					{
						HealthComponent healthComponent = component.healthComponent;
						if (healthComponent)
						{
							TeamComponent teamComponent = healthComponent.GetComponent<TeamComponent>();
							if (teamComponent && teamComponent.teamIndex == self.myTeamFilter.teamIndex)
							{
								return;
							}
							// FUTURE IL
							if (healthComponent.body.hasCloakBuff)
                            {
								return;
                            }
							//
							UnityEvent unityEvent = self.triggerEvents;
							if (unityEvent == null)
							{
								return;
							}
							unityEvent.Invoke();
						}
					}
				}
				return;
			}
		}

        private bool Util_HandleCharacterPhysicsCastResults(On.RoR2.Util.orig_HandleCharacterPhysicsCastResults orig, GameObject bodyObject, Ray ray, RaycastHit[] hits, out RaycastHit hitInfo)
		{
			int num = -1;
			float num2 = float.PositiveInfinity;
			for (int i = 0; i < hits.Length; i++)
			{
				float distance = hits[i].distance;
				if (distance < num2)
				{
					HurtBox component = hits[i].collider.GetComponent<HurtBox>();
					if (component)
					{
						HealthComponent healthComponent = component.healthComponent;
						if (healthComponent)
						{
							if (healthComponent.gameObject == bodyObject)
								goto IL_82;
                            else if (healthComponent.body.hasCloakBuff) // This is where you would put IL if you were smart (not me)
                            {
								continue;
                            }
						}
					}
					if (distance == 0f)
					{
						hitInfo = hits[i];
						hitInfo.point = ray.origin;
						return true;
					}
					num = i;
					num2 = distance;
				}
			IL_82:;
			}
			if (num == -1)
			{
				hitInfo = default;
				return false;
			}
			hitInfo = hits[num];
			return true;
		}

		private bool CombatHealthBarViewer_VictimIsValid(On.RoR2.UI.CombatHealthBarViewer.orig_VictimIsValid orig, RoR2.UI.CombatHealthBarViewer self, HealthComponent victim)
        {
            return victim && victim.alive && (self.victimToHealthBarInfo[victim].endTime > Time.time || victim == self.crosshairTarget) && !victim.body.hasCloakBuff;
        }


	}
}
