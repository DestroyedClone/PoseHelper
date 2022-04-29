using RoR2;
using EntityStates;

namespace MyNameSpace
{
    public class ExampleStzate : BaseState
    {
        public float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Chat.AddMessage("IT'S ALIVE");
        }

        public override void OnExit()
        {
            
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= this.totalDuration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return base.GetMinimumInterruptPriority();
        }
    }
}
