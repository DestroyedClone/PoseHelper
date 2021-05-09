using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace AlternateSkills.Bandit2
{
    public class FireEclipse : EntityStates.Bandit2.Weapon.FireSidearmResetRevolver, IOnKilledOtherServerReceiver
    {
		public void OnKilledOtherServer(DamageReport damageReport)
		{
			if (NetworkServer.active)
			{
				new BlastAttack
				{
					attacker = base.gameObject,
					inflictor = base.gameObject,
					teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
					baseDamage = 0,
					baseForce = 0f,
					position = damageReport.victimBody.corePosition,
					radius = 7f,
					falloffModel = BlastAttack.FalloffModel.Linear,
					attackerFiltering = AttackerFiltering.NeverHit,
					damageType = DamageType.CrippleOnHit
				}.Fire();
			}
		}

		public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
        }
    }
}
