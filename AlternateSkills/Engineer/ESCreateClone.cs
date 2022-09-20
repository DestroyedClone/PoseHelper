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
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using HG;
using RoR2.Audio;
using RoR2.Items;
using RoR2.Navigation;
using RoR2.Networking;
using RoR2.Projectile;
using Unity;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace AlternateSkills.Engi
{
	public class ESCreateClone : BaseSkillState
    {
        [SerializeField]
		public float baseDuration = 0.5f;
		private float duration;

		private HurtBox initialOrbTarget;

		private AllyTracker allyTracker;

        public override void OnEnter()
        {
            base.OnEnter();
			this.allyTracker = base.GetComponent<AllyTracker>();
            if (this.allyTracker && base.isAuthority)
			{
				this.initialOrbTarget = this.allyTracker.GetTrackingTarget();
			}
            this.duration = this.baseDuration / this.attackSpeedStat;
			base.characterBody.SetAimTimer(this.duration + 2f);
			base.PlayAnimation("Gesture, Additive", "PrepWall", "PrepWall.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            SummonAllyClone();
        }

        public void SummonAllyClone()
        {
            if (!NetworkServer.active) return;
            var targetBody = initialOrbTarget.healthComponent.body;
            if (targetBody && !targetBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
            {
                var masterIndex = MasterCatalog.FindAiMasterIndexForBody(targetBody.bodyIndex);
                if ((int)masterIndex > -1)
                {
                    MasterSummon master = new MasterSummon();
                    master.ignoreTeamMemberLimit = true;
                    master.inventoryToCopy = targetBody.inventory;
                    master.loadout = targetBody.master.loadout;
                    master.masterPrefab = MasterCatalog.GetMasterPrefab(masterIndex);
                    master.position = base.transform.position + base.transform.forward * 5f;
                    master.summonerBodyObject = base.gameObject;
                    master.teamIndexOverride = teamComponent.teamIndex;
                    master.useAmbientLevel = true;
                    var result = master.Perform();
                    if (result)
                    {
                        var cb = result.bodyInstanceObject.GetComponent<CharacterBody>();
                        if (!cb.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                        {
                            cb.bodyFlags &= CharacterBody.BodyFlags.Mechanical;
                        }
                        var dio = RoR2Content.Items.ExtraLife;
                        var voidDio = DLC1Content.Items.ExtraLifeVoid;
                        cb.inventory.RemoveItem(dio, cb.inventory.GetItemCount(dio));
                        cb.inventory.RemoveItem(voidDio, cb.inventory.GetItemCount(voidDio));
                        Deployable deployable = result.gameObject.AddComponent<Deployable>();
						deployable.onUndeploy = new UnityEvent();
						deployable.onUndeploy.AddListener(new UnityAction(characterBody.master.TrueKill));
						characterBody.master.AddDeployable(deployable, EngiMain.DeployableSlot_MechanicalClone);
                    } else {
                        activatorSkillSlot.stock++;
                    }
                }
            }
        }

        public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge > this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}
    }
}