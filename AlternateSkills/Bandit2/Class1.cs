using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using EntityStates.Bandit2.Weapon;
using EntityStates;

namespace AlternateSkills.Bandit2
{
	public class FireSidearmResetRevolverScepter : FireSidearmResetRevolver
	{
		// Token: 0x06004753 RID: 18259 RVA: 0x0012125E File Offset: 0x0011F45E
		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			base.ModifyBullet(bulletAttack);
			bulletAttack.damageType |= DamageType.ResetCooldownsOnKill;
			bulletAttack.bulletCount = 2U;
		}
	}
}
