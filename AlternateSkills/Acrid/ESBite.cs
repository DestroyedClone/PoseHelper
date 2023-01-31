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
using EntityStates.Croco;
using UnityEngine.AddressableAssets;

namespace AlternateSkills.Acrid
{
    public class ESBite : EntityStates.Croco.Bite
    {

        public void AcquireIfMissing()
        {
            //var asset = Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Croco/EntityStates.Croco.Bite.asset").WaitForCompletion();
            //credit: ideathhd
            var _ = new EntityStates.Croco.Bite();
            //var self = this as EntityStates.Croco.Bite;
            bloom = _.bloom;
            animationStateName = _.animationStateName;
            baseDuration = _.baseDuration;
            damageCoefficient = _.damageCoefficient;
            hitBoxGroupName = _.hitBoxGroupName;
            hitEffectPrefab = _.hitEffectPrefab;
            procCoefficient = _.procCoefficient;
            pushAwayForce = _.pushAwayForce;
            forceVector = _.forceVector;
            hitPauseDuration = _.hitPauseDuration;
            swingEffectPrefab = _.swingEffectPrefab;
            swingEffectMuzzleString = _.swingEffectMuzzleString;
            mecanimHitboxActiveParameter = _.mecanimHitboxActiveParameter;
            shorthopVelocityFromHit = _.shorthopVelocityFromHit;
            beginStateSoundString = _.beginStateSoundString;
            beginSwingSoundString = _.beginSwingSoundString;
            impactSound = _.impactSound;
            forceForwardVelocity = _.forceForwardVelocity;
            forwardVelocityCurve = _.forwardVelocityCurve;
            scaleHitPauseDurationAndVelocityWithAttackSpeed = _.scaleHitPauseDurationAndVelocityWithAttackSpeed;
            ignoreAttackSpeed = _.ignoreAttackSpeed;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            AcquireIfMissing();
            damageCoefficient = 2f;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
		{
			base.AuthorityModifyOverlapAttack(overlapAttack);
			overlapAttack.damageType = DamageType.Generic;
		}
    }

}