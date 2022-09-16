using System;
using RoR2;
using UnityEngine;
using EntityStates;
using System.Collections.Generic;
using RoR2.Skills;
using EntityStates.Engi.EngiMissilePainter;
using static EntityStates.Engi.EngiMissilePainter.Paint;

namespace AlternateSkills.Bandit2
{
	public class PrepPaint : BaseSkillState
	{
		[SerializeField]
		public float baseDuration;
		[SerializeField]
		public GameObject crosshairOverridePrefab;
		protected float duration;
		private Animator animator;
		private int bodySideWeaponLayerIndex;
		private GameObject originalCrosshairPrefab;
		//2
		public EntityState GetNextState()
		{
			return new FireRenegade();
		}
		//3
		public static GameObject stickyTargetIndicatorPrefab = Paint.stickyTargetIndicatorPrefab;
		public static float stackInterval = Paint.stackInterval;
		[SerializeField]
		public string enterSoundString;
		public static string exitSoundString = Paint.exitSoundString;
		public static string loopSoundString = Paint.loopSoundString;
		public static string lockOnSoundString = Paint.lockOnSoundString;
		public static string stopLoopSoundString = Paint.stopLoopSoundString;
		public static float maxAngle = Paint.maxAngle;
		public static float maxDistance = 99999f;
		private List<HurtBox> targetsList = new List<HurtBox>(); //bookmark

		private Dictionary<HurtBox, PrepPaint.IndicatorInfo> targetIndicators = new Dictionary<HurtBox, IndicatorInfo>();

		private Indicator stickyTargetIndicator;
		private bool releasedKeyOnce = false;
		private float stackStopwatch = 0f;
		private BullseyeSearch search;
		private bool queuedFiringState;
		private uint loopSoundID;
		private HealthComponent previousHighlightTargetHealthComponent;
		private HurtBox previousHighlightTargetHurtBox;

		public byte remainingShots = 3;

		private struct IndicatorInfo
		{
			public int refCount;
			public PrepPaint.EngiMissileIndicator indicator;
		}

		private class EngiMissileIndicator : Indicator
		{
			public override void UpdateVisualizer()
			{
				base.UpdateVisualizer();
				Transform transform = base.visualizerTransform.Find("DotOrigin");
				for (int i = transform.childCount - 1; i >= this.missileCount; i--)
				{
					EntityState.Destroy(transform.GetChild(i));
				}
				for (int j = transform.childCount; j < this.missileCount; j++)
				{
					UnityEngine.Object.Instantiate<GameObject>(base.visualizerPrefab.transform.Find("DotOrigin/DotTemplate").gameObject, transform);
				}
				if (transform.childCount > 0)
				{
					float num = 360f / (float)transform.childCount;
					float num2 = (float)(transform.childCount - 1) * 90f;
					for (int k = 0; k < transform.childCount; k++)
					{
						Transform child = transform.GetChild(k);
						child.gameObject.SetActive(true);
						child.localRotation = Quaternion.Euler(0f, 0f, num2 + (float)k * num);
					}
				}
			}
			public EngiMissileIndicator(GameObject owner, GameObject visualizerPrefab) : base(owner, visualizerPrefab)
			{
			}
			public int missileCount;
		}


		public virtual string exitAnimationStateName
		{
			get
			{
				return "BufferEmpty";
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			this.animator = base.GetModelAnimator();
			this.duration = this.baseDuration / this.attackSpeedStat;
			if (this.animator)
			{
				this.bodySideWeaponLayerIndex = this.animator.GetLayerIndex("Body, SideWeapon");
				this.animator.SetLayerWeight(this.bodySideWeaponLayerIndex, 1f);
				base.PlayAnimation("Gesture, Additive", "MainToSide", "MainToSide.playbackRate", this.duration);
			}
			if (this.crosshairOverridePrefab)
			{
				this.originalCrosshairPrefab = base.characterBody.crosshairPrefab;
				base.characterBody.crosshairPrefab = this.crosshairOverridePrefab;
			}
			base.characterBody.SetAimTimer(3f);
			//2
			Util.PlaySound(this.enterSoundString, base.gameObject);
			//3
			if (base.isAuthority)
			{
				this.targetsList = new List<HurtBox>();
				this.targetIndicators = new Dictionary<HurtBox, PrepPaint.IndicatorInfo>(); //Dictionary<HurtBox, Paint.IndicatorInfo>()
				this.stickyTargetIndicator = new Indicator(base.gameObject, Paint.stickyTargetIndicatorPrefab);
				this.search = new BullseyeSearch();
			}
			Util.PlaySound(Paint.enterSoundString, base.gameObject);
			this.loopSoundID = Util.PlaySound(Paint.loopSoundString, base.gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterBody.SetAimTimer(3f);
			if (base.isAuthority)
			{
				this.AuthorityFixedUpdate();
				if (base.characterBody.isSprinting)
				{
					this.outer.SetNextStateToMain();
				}
			}
			if (base.fixedAge > this.duration)
			{
				this.outer.SetNextState(this.GetNextState());
			}
		}

		public override void OnExit()
		{
			if (this.animator)
			{
				this.animator.SetLayerWeight(this.bodySideWeaponLayerIndex, 0f);
			}
			base.PlayAnimation("Gesture, Additive", this.exitAnimationStateName);
			if (this.crosshairOverridePrefab)
			{
				base.characterBody.crosshairPrefab = this.originalCrosshairPrefab;
			}
			Transform transform = base.FindModelChild("SpinningPistolFX");
			if (transform)
			{
				transform.gameObject.SetActive(false);
			}
			//3
			if (base.isAuthority && !this.outer.destroying && !this.queuedFiringState)
			{
				for (int i = 0; i < this.targetsList.Count; i++)
				{
					base.activatorSkillSlot.AddOneStock();
				}
			}
			if (this.targetIndicators != null)
			{
				foreach (KeyValuePair<HurtBox, PrepPaint.IndicatorInfo> keyValuePair in this.targetIndicators)
				{
					keyValuePair.Value.indicator.active = false;
				}
			}
			if (this.stickyTargetIndicator != null)
			{
				this.stickyTargetIndicator.active = false;
			}
			Util.PlaySound(Paint.exitSoundString, base.gameObject);
			Util.PlaySound(Paint.stopLoopSoundString, base.gameObject);

			base.OnExit();
		}

		private void AddTargetAuthority(HurtBox hurtBox)
		{
			if (remainingShots == 0)
			{
				return;
			}
			Util.PlaySound(Paint.lockOnSoundString, base.gameObject);
			this.targetsList.Add(hurtBox);
            if (!this.targetIndicators.TryGetValue(hurtBox, out IndicatorInfo indicatorInfo))
            {
                indicatorInfo = new PrepPaint.IndicatorInfo
                {
                    refCount = 0,
                    indicator = new PrepPaint.EngiMissileIndicator(base.gameObject, Resources.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"))
                };
                indicatorInfo.indicator.targetTransform = hurtBox.transform;
                indicatorInfo.indicator.active = true;
            }
            indicatorInfo.refCount++;
			indicatorInfo.indicator.missileCount = indicatorInfo.refCount;
			this.targetIndicators[hurtBox] = indicatorInfo;
			remainingShots--;
		}

		private void RemoveTargetAtAuthority(int i)
		{
			HurtBox key = this.targetsList[i];
			this.targetsList.RemoveAt(i);
            if (this.targetIndicators.TryGetValue(key, out IndicatorInfo indicatorInfo))
            {
                indicatorInfo.refCount--;
                indicatorInfo.indicator.missileCount = indicatorInfo.refCount;
                this.targetIndicators[key] = indicatorInfo;
                if (indicatorInfo.refCount == 0)
                {
                    indicatorInfo.indicator.active = false;
                    this.targetIndicators.Remove(key);
                }
            }
        }

		private void CleanTargetsList()
		{
			for (int i = this.targetsList.Count - 1; i >= 0; i--)
			{
				HurtBox hurtBox = this.targetsList[i];
				if (!hurtBox.healthComponent || !hurtBox.healthComponent.alive)
				{
					this.RemoveTargetAtAuthority(i);
					remainingShots++;
				}
			}
			for (int j = this.targetsList.Count - 1; j >= base.activatorSkillSlot.maxStock; j--)
			{
				this.RemoveTargetAtAuthority(j);
			}
		}

		private void GetCurrentTargetInfo(out HurtBox currentTargetHurtBox, out HealthComponent currentTargetHealthComponent)
		{
			Ray aimRay = base.GetAimRay();
			this.search.filterByDistinctEntity = true;
			this.search.filterByLoS = true;
			this.search.minDistanceFilter = 0f;
			this.search.maxDistanceFilter = Paint.maxDistance;
			this.search.minAngleFilter = 0f;
			this.search.maxAngleFilter = Paint.maxAngle;
			this.search.viewer = base.characterBody;
			this.search.searchOrigin = aimRay.origin;
			this.search.searchDirection = aimRay.direction;
			this.search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
			this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(base.GetTeam());
			this.search.RefreshCandidates();
			this.search.FilterOutGameObject(base.gameObject);
			foreach (HurtBox hurtBox in this.search.GetResults())
			{
				if (hurtBox.healthComponent && hurtBox.healthComponent.alive)
				{
					currentTargetHurtBox = hurtBox;
					currentTargetHealthComponent = hurtBox.healthComponent;
					return;
				}
			}
			currentTargetHurtBox = null;
			currentTargetHealthComponent = null;
		}

		private void AuthorityFixedUpdate()
		{
			this.CleanTargetsList();
			bool allowFire = false;
            this.GetCurrentTargetInfo(out HurtBox hurtBox, out HealthComponent y);
            if (hurtBox)
			{
				this.stackStopwatch += Time.fixedDeltaTime;
				if (base.inputBank.skill4.down && (this.previousHighlightTargetHealthComponent != y || this.stackStopwatch > Paint.stackInterval / this.attackSpeedStat || base.inputBank.skill4.justPressed))
				{
					this.stackStopwatch = 0f;
					this.AddTargetAuthority(hurtBox);
				}
			}
			if (base.inputBank.skill4.justReleased)
			{
				if (this.releasedKeyOnce) //remove this if statement if it doesnt work
				{
					allowFire = true;
				}
				this.releasedKeyOnce = true;
			}
			if (hurtBox != this.previousHighlightTargetHurtBox)
			{
				this.previousHighlightTargetHurtBox = hurtBox;
				this.previousHighlightTargetHealthComponent = y;
				this.stickyTargetIndicator.targetTransform = ((hurtBox && base.activatorSkillSlot.stock != 0) ? hurtBox.transform : null);
				this.stackStopwatch = 0f;
			}
			this.stickyTargetIndicator.active = this.stickyTargetIndicator.targetTransform;
			if (allowFire)
			{
				this.queuedFiringState = true;
				this.outer.SetNextState(new FirePaint
				{
					targetsList = this.targetsList,
					activatorSkillSlot = base.activatorSkillSlot
				});
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
