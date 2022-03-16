using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using EntityStates;
using R2API;
using RoR2.Skills;
using RoR2.Projectile;


namespace EngiProjectileTeamSwapSkill.Engineer
{
	[RequireComponent(typeof(Collider))]
	public class ReflectProjectileZone : MonoBehaviour
    {
        public TeamFilter teamFilter;

		public float damageMultiplier = 0.25f;

		private void OnTriggerEnter(Collider other)
		{
			other.GetComponent<TeamFilter>();
			Rigidbody component = other.GetComponent<Rigidbody>();
			ProjectileController component2 = other.GetComponent<ProjectileController>();
			ProjectileDamage component3 = other.GetComponent<ProjectileDamage>();
			if (component && !ReflectedProjectileTracker.rigidBodies.Contains(component) && component2 && component3)
			{
				if (component2.teamFilter.teamIndex != teamFilter.teamIndex)
				{
					component2.teamFilter.teamIndex = teamFilter.teamIndex;
					if (component2.owner)
					{
						component.rotation = Util.QuaternionSafeLookRotation(component2.owner.transform.position - component.position);
					}
				}

				component2.procCoefficient = 0;
				var tracker = component2.gameObject.AddComponent<ReflectedProjectileTracker>();
				tracker.rigidBody = component;
				ProjectileManager.instance.FireProjectile(other.gameObject, other.transform.position, Util.QuaternionSafeLookRotation(component2.owner.transform.position - component.position),
					component2.owner, damageMultiplier, component3.force, component3.crit, component3.damageColorIndex, component2.owner);
				Destroy(other.gameObject);
			}
		}
	}

	public class ReflectedProjectileTracker : MonoBehaviour
    {
		public static List<Rigidbody> rigidBodies = new List<Rigidbody>();
		public Rigidbody rigidBody;

		public void Awake()
        {
			InstanceTracker.Add(this);
			rigidBodies.Add(rigidBody);
        }

		public void OnDestroy()
        {
			InstanceTracker.Remove(this);
			rigidBodies.Remove(rigidBody);
		}
    }
}
