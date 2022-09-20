using EntityStates;
using static AlternateSkills.Merc.MercenaryMain;
using RoR2;

namespace AlternateSkills.Merc
{
	public class ESActiveEcho : BaseSkillState
    {
        // The last attack's type and position are stored on comp
        // 1. Enter skill
        // 2. Get info from comp
        // 3. Activate
        // 4. Leave
        public DCMercEchoComponent echoComponent;

        public override void OnEnter()
        {
            base.OnEnter();
            echoComponent = characterBody.GetComponent<DCMercEchoComponent>();
            if (echoComponent)
            {   
                Chat.AddMessage("Attempting to slash");
                echoComponent.ConsumeSlash();
            }
            OnExit();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return base.GetMinimumInterruptPriority();
        }

    }
}