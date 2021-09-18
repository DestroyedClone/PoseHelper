using EntityStates.JellyfishMonster;

namespace JellyfishShock
{
    public class JellyfishFixedDeathState : DeathState
    {
        public override void PlayDeathAnimation(float crossfadeDuration = 0.1F)
        {
            return;
        }
    }
}