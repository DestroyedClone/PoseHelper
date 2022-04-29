using RoR2;
using EntityStates;

namespace MyNameSpace
{
    public class ExampleState : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //Code Here
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //Code Here
        }

        public override void OnExit()
        {
            //Code Here
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return base.GetMinimumInterruptPriority();
        }
    }
}
