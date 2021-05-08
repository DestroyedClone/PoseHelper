using RoR2;
using EntityStates;
using R2API;
using EntityStates.Bandit2;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace AlternateSkills.Bandit2
{
    public class Eclipse : EntityStates.Bandit2.Weapon.FireSidearmResetRevolver, IOnKilledOtherServerReceiver
    {
        public void OnKilledOtherServer(DamageReport damageReport)
        {
            var blastDmg = damageReport.damageDealt - damageReport.combinedHealthBeforeDamage;
			Debug.Log("Resulting damage: "+ blastDmg);
			if (NetworkServer.active)
			{
				new BlastAttack
				{
					attacker = base.gameObject,
					inflictor = base.gameObject,
					teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
					baseDamage = blastDmg,
					baseForce = 0f,
					position = damageReport.victimBody.corePosition,
					radius = 7f,
					falloffModel = BlastAttack.FalloffModel.Linear,
					attackerFiltering = AttackerFiltering.NeverHit,
					damageType = DamageType.ResetCooldownsOnKill
				}.Fire();
			}
		}
    }
}
