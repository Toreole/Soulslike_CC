using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulslike
{
    internal class RollingState : PlayerState
    {
        public RollingState(PlayerMachine machine) : base(machine)
        {
        }

        public override int Priority => BasePriority;

        protected override int BasePriority => 80;

        internal override void OnAnimatorMove(float deltaTime)
        {
            machine.Animator.ApplyBuiltinRootMotion();
            if (machine.HasValidRollInput && machine.AllowRollCancel)
            {
                //input for roll given, reset the roll animation.
                machine.Animator.SetTrigger("ReRoll");
                machine.HasValidRollInput = false;
                machine.AllowRollCancel = false;
                machine.transform.forward = machine.GetWorldSpaceInput().normalized;
            }
        }

        internal override void OnEnter()
        {
            machine.AllowRollCancel = false;
            machine.transform.forward = machine.GetWorldSpaceInput().normalized;
            machine.PlayAnimationID(BasePriority);
        }

        internal override void OnExit()
        {

        }
    }
}
