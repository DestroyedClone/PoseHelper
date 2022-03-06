using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;
using System.Linq;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace InspectPlayer
{
    [BepInPlugin("com.DestroyedClone.InspectPlayer", "InspectPlayer", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.DifferentModVersionsAreOk)]
    public class Inspect : BaseUnityPlugin
    {
        // Check Key /\
        // Raytrace from origin cursor to where aiming /\
        // evaluate if its something inspectable /\
        // if it is:
        // show

        public static KeyCode keyToInspect = KeyCode.F;


        public void Start()
        {
			RoR2Content.Survivors.Commando.bodyPrefab.AddComponent<InspectTracker>();
		}

		[RequireComponent(typeof(TeamComponent))]
		[RequireComponent(typeof(CharacterBody))]
		[RequireComponent(typeof(InputBankTest))]
		public class InspectTracker : MonoBehaviour
		{
			public GameObject masterObject { get; set; }
			public CharacterMaster master { get; set; }

			// Token: 0x0600115E RID: 4446 RVA: 0x000491C9 File Offset: 0x000473C9
			private void Awake()
			{
				this.indicator = new Indicator(base.gameObject, Resources.Load<GameObject>("Prefabs/HuntressTrackingIndicator"));
				var playerNetworkUser = NetworkUser.readOnlyLocalPlayersList[0];
				if (!masterObject)
				{
					masterObject = playerNetworkUser.masterObject;
				}
				master = masterObject.GetComponent<CharacterMaster>();
			}

            private void Master_onBodyDestroyed(CharacterBody obj)
            {
				SpectateTarget(null);
            }

            // Token: 0x0600115F RID: 4447 RVA: 0x000491E6 File Offset: 0x000473E6
            private void Start()
			{
				this.characterBody = base.GetComponent<CharacterBody>();
				this.inputBank = base.GetComponent<InputBankTest>();
				this.teamComponent = base.GetComponent<TeamComponent>();
			}

			// Token: 0x06001160 RID: 4448 RVA: 0x0004920C File Offset: 0x0004740C
			public HurtBox GetTrackingTarget()
			{
				return this.trackingTarget;
			}

			// Token: 0x06001161 RID: 4449 RVA: 0x00049214 File Offset: 0x00047414
			private void OnEnable()
			{
				this.indicator.active = true;
				master.onBodyDestroyed += Master_onBodyDestroyed;
			}

			// Token: 0x06001162 RID: 4450 RVA: 0x00049222 File Offset: 0x00047422
			private void OnDisable()
			{
				this.indicator.active = false;
				master.onBodyDestroyed -= Master_onBodyDestroyed;
			}

			// Token: 0x06001163 RID: 4451 RVA: 0x00049230 File Offset: 0x00047430
			private void FixedUpdate()
			{
				this.trackerUpdateStopwatch += Time.fixedDeltaTime;
				if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
				{
					this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
					HurtBox hurtBox = this.trackingTarget;
					Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
					this.SearchForTarget(aimRay);
					this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
				}
			}

			private void Update()
			{
				if (characterBody.hasAuthority && characterBody.isPlayerControlled && characterBody.master
				   && !LocalUserManager.readOnlyLocalUsersList[0].isUIFocused)
				{
					var trackingTarget = GetTrackingTarget();
					if (Input.GetKeyDown(keyToInspect))
					{
						if (trackingTarget)
						{
							if (NetworkServer.active)
							{
								//DisplayInfoAboutTarget(trackingTarget);
								SpectateTarget(trackingTarget);
							}
							else
							{

							}
						}
					}
				}
			}

			public static CharacterBody originalBody;

			private void SpectateTarget(HurtBox hurtBox)
			{
				var playerNetworkUser = NetworkUser.readOnlyLocalPlayersList[0];
				var playerMasterObject = playerNetworkUser.masterObject;
				var playerCameraRigController = playerNetworkUser.cameraRigController;

				CharacterMaster enemyToSpectate = null;

				if (!hurtBox)
                {
					enemyToSpectate = masterObject.GetComponent<CharacterMaster>();
				} else
                {
					if (hurtBox.healthComponent.body?.master)
					{
						enemyToSpectate = hurtBox.healthComponent.body.master;
					}
				}

				if (enemyToSpectate != null)
				{
					playerNetworkUser.masterObject = enemyToSpectate.gameObject;
					//playerNetworkUser.masterObject = enemyToSpectate.GetBodyObject();
				} else
				{
					playerNetworkUser.masterObject = masterObject;
				}
			}

			private void DisplayInfoAboutTarget(HurtBox hurtBox)
            {
				var hc = hurtBox.healthComponent;
				var body = hc.body;
				var skillLocator = body.skillLocator;
				var message = $"{body.GetDisplayName()} HP: {hc.health} / {hc.fullHealth}";
				if (skillLocator.primary)
                {
					message += $"\n{skillLocator.primary.skillName}";
				}
				if (skillLocator.secondary)
				{
					message += $"\n{skillLocator.secondary.skillName}";
				}
				if (skillLocator.utility)
				{
					message += $"\n{skillLocator.utility.skillName}";
				}
				if (skillLocator.special)
				{
					message += $"\n{skillLocator.special.skillName}";
				}
				Chat.AddMessage(message);

			}

			// Token: 0x06001164 RID: 4452 RVA: 0x000492D0 File Offset: 0x000474D0
			private void SearchForTarget(Ray aimRay)
			{
				//this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
				this.search.filterByLoS = true;
				this.search.searchOrigin = aimRay.origin;
				this.search.searchDirection = aimRay.direction;
				this.search.sortMode = BullseyeSearch.SortMode.Distance;
				this.search.maxDistanceFilter = this.maxTrackingDistance;
				this.search.maxAngleFilter = this.maxTrackingAngle;
				this.search.RefreshCandidates();
				this.search.FilterOutGameObject(base.gameObject);
				this.trackingTarget = this.search.GetResults().FirstOrDefault<HurtBox>();
			}

			// Token: 0x04000FA1 RID: 4001
			public float maxTrackingDistance = 100f;

			// Token: 0x04000FA2 RID: 4002
			public float maxTrackingAngle = 20f;

			// Token: 0x04000FA3 RID: 4003
			public float trackerUpdateFrequency = 10f;

			// Token: 0x04000FA4 RID: 4004
			private HurtBox trackingTarget;

			// Token: 0x04000FA5 RID: 4005
			private CharacterBody characterBody;

			// Token: 0x04000FA6 RID: 4006
			private TeamComponent teamComponent;

			// Token: 0x04000FA7 RID: 4007
			private InputBankTest inputBank;

			// Token: 0x04000FA8 RID: 4008
			private float trackerUpdateStopwatch;

			// Token: 0x04000FA9 RID: 4009
			private Indicator indicator;

			// Token: 0x04000FAA RID: 4010
			private readonly BullseyeSearch search = new BullseyeSearch();
		}
	}
}
