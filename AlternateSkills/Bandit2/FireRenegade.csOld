using RoR2;

namespace AlternateSkills.Bandit2
{
	public class FireRenegade : EntityStates.Bandit2.Weapon.FireSidearmSkullRevolver, IOnKilledOtherServerReceiver
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
