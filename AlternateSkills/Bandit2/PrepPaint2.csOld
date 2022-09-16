using System;
using RoR2;
using UnityEngine;
using EntityStates;
using System.Collections.Generic;
using RoR2.Skills;
using EntityStates.Engi.EngiMissilePainter;

namespace AlternateSkills.Bandit2
{
    public class PrepPaint : EntityStates.Bandit2.Weapon.BaseSidearmState
    {
		public static GameObject crosshairOverridePrefab;
		public static GameObject stickyTargetIndicatorPrefab;
		public static float stackInterval;
		public static string enterSoundString;
		public static string exitSoundString;
		public static string loopSoundString;
		public static string lockOnSoundString;
		public static string stopLoopSoundString;
		public static float maxAngle;
		public static float maxDistance;
		private List<HurtBox> targetsList;
		private Dictionary<HurtBox, IndicatorInfo> targetIndicators;
		private Indicator stickyTargetIndicator;
		private bool releasedKeyOnce;
		private float stackStopwatch;
		private GameObject previousCrosshairPrefab;
		private BullseyeSearch search;
		private bool queuedFiringState;
		private uint loopSoundID;
		private HealthComponent previousHighlightTargetHealthComponent;
		private HurtBox previousHighlightTargetHurtBox;
		private struct IndicatorInfo
		{
			public int refCount;
			public Paint.EngiMissileIndicator indicator;
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

			// Token: 0x04003D1D RID: 15645
			public int missileCount;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			if (base.isAuthority)
			{
				this.targetsList = new List<HurtBox>();
				this.targetIndicators = new Dictionary<HurtBox, PrepPaint.IndicatorInfo>();
				this.stickyTargetIndicator = new Indicator(base.gameObject, Paint.stickyTargetIndicatorPrefab);
				this.search = new BullseyeSearch();
			}
			base.PlayCrossfade("Gesture, Additive", "PrepHarpoons", 0.1f);
			Util.PlaySound(Paint.enterSoundString, base.gameObject);
			this.loopSoundID = Util.PlaySound(Paint.loopSoundString, base.gameObject);
			this.previousCrosshairPrefab = base.characterBody.crosshairPrefab;
			base.characterBody.crosshairPrefab = Paint.crosshairOverridePrefab;
		}

		public override void OnExit()
		{
			if (base.isAuthority && !this.outer.destroying && !this.queuedFiringState)
			{
				for (int i = 0; i < this.targetsList.Count; i++)
				{
					this.activatorSkillSlot.AddOneStock();
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
			base.characterBody.crosshairPrefab = this.previousCrosshairPrefab;
			base.PlayCrossfade("Gesture, Additive", "ExitHarpoons", 0.1f);
			Util.PlaySound(Paint.exitSoundString, base.gameObject);
			Util.PlaySound(Paint.stopLoopSoundString, base.gameObject);
			base.OnExit();
		}

		// Token: 0x060043DD RID: 17373 RVA: 0x001114D4 File Offset: 0x0010F6D4
		private void AddTargetAuthority(HurtBox hurtBox)
		{
			if (base.activatorSkillSlot.stock == 0)
			{
				return;
			}
			Util.PlaySound(Paint.lockOnSoundString, base.gameObject);
			this.targetsList.Add(hurtBox);
			Paint.IndicatorInfo indicatorInfo;
			if (!this.targetIndicators.TryGetValue(hurtBox, out indicatorInfo))
			{
				indicatorInfo = new Paint.IndicatorInfo
				{
					refCount = 0,
					indicator = new Paint.EngiMissileIndicator(base.gameObject, Resources.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"))
				};
				indicatorInfo.indicator.targetTransform = hurtBox.transform;
				indicatorInfo.indicator.active = true;
			}
			indicatorInfo.refCount++;
			indicatorInfo.indicator.missileCount = indicatorInfo.refCount;
			this.targetIndicators[hurtBox] = indicatorInfo;
			base.activatorSkillSlot.DeductStock(1);
		}

		// Token: 0x060043DE RID: 17374 RVA: 0x001115A0 File Offset: 0x0010F7A0
		private void RemoveTargetAtAuthority(int i)
		{
			HurtBox key = this.targetsList[i];
			this.targetsList.RemoveAt(i);
			Paint.IndicatorInfo indicatorInfo;
			if (this.targetIndicators.TryGetValue(key, out indicatorInfo))
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

		// Token: 0x060043DF RID: 17375 RVA: 0x00111624 File Offset: 0x0010F824
		private void CleanTargetsList()
		{
			for (int i = this.targetsList.Count - 1; i >= 0; i--)
			{
				HurtBox hurtBox = this.targetsList[i];
				if (!hurtBox.healthComponent || !hurtBox.healthComponent.alive)
				{
					this.RemoveTargetAtAuthority(i);
					base.activatorSkillSlot.AddOneStock();
				}
			}
			for (int j = this.targetsList.Count - 1; j >= base.activatorSkillSlot.maxStock; j--)
			{
				this.RemoveTargetAtAuthority(j);
			}
		}

		// Token: 0x060043E0 RID: 17376 RVA: 0x001116AB File Offset: 0x0010F8AB
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterBody.SetAimTimer(3f);
			if (base.isAuthority)
			{
				this.AuthorityFixedUpdate();
			}
		}

		// Token: 0x060043E1 RID: 17377 RVA: 0x001116D4 File Offset: 0x0010F8D4
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

		// Token: 0x060043E2 RID: 17378 RVA: 0x00111820 File Offset: 0x0010FA20
		private void AuthorityFixedUpdate()
		{
			this.CleanTargetsList();
			bool flag = false;
			HurtBox hurtBox;
			HealthComponent y;
			this.GetCurrentTargetInfo(out hurtBox, out y);
			if (hurtBox)
			{
				this.stackStopwatch += Time.fixedDeltaTime;
				if (base.inputBank.skill1.down && (this.previousHighlightTargetHealthComponent != y || this.stackStopwatch > Paint.stackInterval / this.attackSpeedStat || base.inputBank.skill1.justPressed))
				{
					this.stackStopwatch = 0f;
					this.AddTargetAuthority(hurtBox);
				}
			}
			if (base.inputBank.skill1.justReleased)
			{
				flag = true;
			}
			if (base.inputBank.skill2.justReleased)
			{
				this.outer.SetNextStateToMain();
				return;
			}
			if (base.inputBank.skill3.justReleased)
			{
				if (this.releasedKeyOnce)
				{
					flag = true;
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
			if (flag)
			{
				this.queuedFiringState = true;
				this.outer.SetNextState(new Fire
				{
					targetsList = this.targetsList,
					activatorSkillSlot = base.activatorSkillSlot
				});
			}
		}
	}
}
