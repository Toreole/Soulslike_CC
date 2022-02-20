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
            //rolling gets its motion from the animation.
            machine.Animator.ApplyBuiltinRootMotion();
            //check for rollinput when rolling is allowed, this "skips" the transition to a different state, and lets it remain in the rolling state.
            if (machine.HasValidRollInput && machine.HasFlag(PlayerMachine.PlayerFlags.CanRoll))
            {
                //input for roll given, reset the roll animation.
                machine.Animator.SetTrigger(PlayerAnimationUtil.paramID_ReRoll);
                //consume roll input and unset the canroll flag
                machine.HasValidRollInput = false;
                machine.UnsetFlag(PlayerMachine.PlayerFlags.CanRoll);
                //re-adjust the rolling direction. this doesnt use machine.RotateTowards to make it snappier.
                machine.transform.forward = machine.GetWorldSpaceInput().normalized;
            }
        }

        internal override PlayerState MoveNextState(PlayerMachine machine)
        {
            if (machine.HasFlag(PlayerMachine.PlayerFlags.CanAttack) && machine.HasValidAttackInput)
                return new AttackingState();
            return base.MoveNextState(machine);
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            machine.transform.forward = machine.GetWorldSpaceInput().normalized;
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_roll);
            machine.UnsetFlag(PlayerMachine.PlayerFlags.TriesToIdle | PlayerMachine.PlayerFlags.CanAttack | PlayerMachine.PlayerFlags.CanRoll);
        }

        internal override void OnExit(PlayerMachine machine)
        {

        }
    }
}
