using BepInEx;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using UnityEngine.Networking;

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
        public void Awake()
        {
            On.RoR2.UI.CombatHealthBarViewer.VictimIsValid += CombatHealthBarViewer_VictimIsValid;
            On.RoR2.PingerController.GeneratePingInfo += PingerController_GeneratePingInfo;
        }

        private bool PingerController_GeneratePingInfo(On.RoR2.PingerController.orig_GeneratePingInfo orig, Ray aimRay, GameObject bodyObject, out PingerController.PingInfo result)
        {
			result = new PingerController.PingInfo
			{
				active = true,
				origin = Vector3.zero,
				normal = Vector3.zero,
				targetNetworkIdentity = null
			};
            aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, bodyObject, out float num);
            float maxDistance = 1000f + num;
			if (Util.CharacterRaycast(bodyObject, aimRay, out RaycastHit raycastHit, maxDistance, LayerIndex.entityPrecise.mask | LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
			{
				HurtBox component = raycastHit.collider.GetComponent<HurtBox>();
				if (component && component.healthComponent && !component.healthComponent.body.hasCloakBuff)
				{
					CharacterBody body = component.healthComponent.body;
					result.origin = body.corePosition;
					result.normal = Vector3.zero;
					result.targetNetworkIdentity = body.networkIdentity;
					return true;
				}
			}
			if (Util.CharacterRaycast(bodyObject, aimRay, out raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.defaultLayer.mask | LayerIndex.pickups.mask, QueryTriggerInteraction.Collide))
			{
				GameObject gameObject = raycastHit.collider.gameObject;
				NetworkIdentity networkIdentity = gameObject.GetComponentInParent<NetworkIdentity>();
				if (!networkIdentity)
				{
					Transform parent = gameObject.transform.parent;
					EntityLocator entityLocator = parent ? parent.GetComponentInChildren<EntityLocator>() : gameObject.GetComponent<EntityLocator>();
					if (entityLocator)
					{
						gameObject = entityLocator.entity;
						networkIdentity = gameObject.GetComponent<NetworkIdentity>();
					}
				}
				result.origin = raycastHit.point;
				result.normal = raycastHit.normal;
				result.targetNetworkIdentity = networkIdentity;
				return true;
			}
			return false;
		}

        private bool CombatHealthBarViewer_VictimIsValid(On.RoR2.UI.CombatHealthBarViewer.orig_VictimIsValid orig, RoR2.UI.CombatHealthBarViewer self, HealthComponent victim)
        {
            return victim && victim.alive && (self.victimToHealthBarInfo[victim].endTime > Time.time || victim == self.crosshairTarget) && !victim.body.hasCloakBuff;
        }

    }
}
