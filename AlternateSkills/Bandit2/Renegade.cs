using RoR2;
using EntityStates;
using R2API;
using EntityStates.Bandit2;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace AlternateSkills.Bandit2
{
	public class Renegade : EntityStates.Bandit2.Weapon.FireSidearmSkullRevolver, IOnKilledOtherServerReceiver
	{
		public void OnKilledOtherServer(DamageReport damageReport)
		{
		}

		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			base.ModifyBullet(bulletAttack);
			bulletAttack.isCrit = true;
		}
	}
}
