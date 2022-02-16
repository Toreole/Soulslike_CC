using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulslike
{
    internal class RollingState : PlayerState
    {

        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            machine.Animator.ApplyBuiltinRootMotion();
            if (machine.HasValidRollInput && machine.AllowRollCancel)
            {
                //input for roll given, reset the roll animation.
                machine.Animator.SetTrigger(PlayerAnimationUtil.paramID_ReRoll);
                machine.HasValidRollInput = false;
                machine.AllowRollCancel = false;
                machine.transform.forward = machine.GetWorldSpaceInput().normalized;
            }
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            machine.AllowRollCancel = false;
            machine.transform.forward = machine.GetWorldSpaceInput().normalized;
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_roll);
        }

        internal override void OnExit(PlayerMachine machine)
        {

        }
    }
}
