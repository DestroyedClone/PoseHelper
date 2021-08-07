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
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;

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
		public GameObject DoppelgangerEffect = Resources.Load<GameObject>("prefabs/temporaryvisualeffects/DoppelgangerEffect");
		public static float evisMaxRange = EntityStates.Merc.Evis.maxRadius;

		public void Awake()
        {
			// Umbra
			ModifyDoppelGangerEffect();

			// Healthbar
            On.RoR2.UI.CombatHealthBarViewer.VictimIsValid += CombatHealthBarViewer_VictimIsValid;

			// Pinging
            On.RoR2.Util.HandleCharacterPhysicsCastResults += Util_HandleCharacterPhysicsCastResults;

			//Projectile Stuff
            On.RoR2.Projectile.MissileController.FindTarget += MissileController_FindTarget;

			// Character Specific
				// Huntress
            On.RoR2.HuntressTracker.SearchForTarget += HuntressTracker_SearchForTarget; //Aiming
                                                                                        // LightningOrb.PickNextTarget for glaive
                                                                                        // Merc
            On.EntityStates.Merc.Evis.SearchForTarget += Evis_SearchForTarget;

            // Shock thing
            On.RoR2.SetStateOnHurt.SetShock += SetStateOnHurt_SetShock;

		}

        private void SetStateOnHurt_SetShock(On.RoR2.SetStateOnHurt.orig_SetShock orig, SetStateOnHurt self, float duration)
        {
			orig(self, duration);
			// only continues if they can be shocked, so we can skip an if statement
			// ... i think
			var body = self.transform.parent.gameObject.GetComponent<CharacterBody>();
			body.RemoveBuff(RoR2Content.Buffs.Cloak);
			if (body.HasBuff(RoR2Content.Buffs.CloakSpeed)) //todo: add config for removing cloak speed if disrupted
			{
				body.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
			}

		}

        private HurtBox Evis_SearchForTarget(On.EntityStates.Merc.Evis.orig_SearchForTarget orig, EntityStates.Merc.Evis self)
        {
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.searchOrigin = base.transform.position;
			bullseyeSearch.searchDirection = UnityEngine.Random.onUnitSphere;
			bullseyeSearch.maxDistanceFilter = evisMaxRange;
			bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.GetTeam());
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
			bullseyeSearch.RefreshCandidates();
			bullseyeSearch.FilterOutGameObject(base.gameObject);
			return bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
		}

        private void ModifyDoppelGangerEffect()
        {
			if (!DoppelgangerEffect) return;

			var comp = DoppelgangerEffect.GetComponent<HideShadowIfCloaked>();
			if (!comp)
            {
				comp = DoppelgangerEffect.AddComponent<HideShadowIfCloaked>();
            }
			comp.particles = DoppelgangerEffect.transform.Find("Particles").gameObject;
		}

		private HurtBox FilterMethod(IEnumerable<HurtBox> listOfTargets)
        {
			HurtBox hurtBox = listOfTargets.FirstOrDefault<HurtBox>();

			int index = 0;
			while (hurtBox != null)
			{
				if ((bool)hurtBox.healthComponent?.body?.hasCloakBuff)
				{
					index++;
					hurtBox = listOfTargets.ElementAtOrDefault(index);
					continue;
				}
				break;
			}
			return hurtBox;
		}

        private void HuntressTracker_SearchForTarget(On.RoR2.HuntressTracker.orig_SearchForTarget orig, HuntressTracker self, Ray aimRay)
        {
			self.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.teamComponent.teamIndex);
			self.search.filterByLoS = true;
			self.search.searchOrigin = aimRay.origin;
			self.search.searchDirection = aimRay.direction;
			self.search.sortMode = BullseyeSearch.SortMode.Distance;
			self.search.maxDistanceFilter = self.maxTrackingDistance;
			self.search.maxAngleFilter = self.maxTrackingAngle;
			self.search.RefreshCandidates();
			self.search.FilterOutGameObject(self.gameObject);
			self.trackingTarget = FilterMethod(self.search.GetResults());
		}

        private Transform MissileController_FindTarget(On.RoR2.Projectile.MissileController.orig_FindTarget orig, RoR2.Projectile.MissileController self)
        {
			self.search.searchOrigin = self.transform.position;
			self.search.searchDirection = self.transform.forward;
			self.search.teamMaskFilter.RemoveTeam(self.teamFilter.teamIndex);
			self.search.RefreshCandidates();
			HurtBox hurtBox = FilterMethod(self.search.GetResults());

			if (hurtBox == null)
			{
				return null;
			}
			return hurtBox.transform;
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

		private class HideShadowIfCloaked : MonoBehaviour
        {
			public CharacterBody body;
			public GameObject particles;

			public void FixedUpdate()
            {
				if (body)
				{
					particles.SetActive(body.hasCloakBuff);
				}
            }
        }
	}
}
