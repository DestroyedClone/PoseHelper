using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using EntityStates.BrotherMonster;

namespace MithrixMeme
{
    public class NoAnimDeathState : TrueDeathState
    {
		public override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
			EffectManager.SimpleMuzzleFlash(TrueDeathState.deathEffectPrefab, base.gameObject, "MuzzleCenter", false);
		}
	}
}
