using System;
using PlayerFlags = Soulslike.PlayerMachine.PlayerFlags;

namespace Soulslike
{
    internal class RollingState : PlayerState
    {

        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            //rolling gets its motion from the animation.
            machine.Animator.ApplyBuiltinRootMotion();
            //check for rollinput when rolling is allowed, this "skips" the transition to a different state, and lets it remain in the rolling state.
            if (machine.HasValidRollInput && machine.HasFlag(PlayerFlags.CanRoll))
            {
                //input for roll given, reset the roll animation.
                machine.Animator.SetTrigger(PlayerAnimationUtil.paramID_ReRoll);
                //consume roll input and unset the canroll flag
                machine.HasValidRollInput = false;
                machine.UnsetFlag(PlayerMachine.PlayerFlags.CanRoll);
                //re-adjust the rolling direction. this doesnt use machine.RotateTowards to make it snappier.
                machine.transform.forward = machine.GetWorldSpaceInput().normalized;
                //use stamina. 
                machine.UseRollStamina();
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
            //while rolling, dont idle, dont attack, dont roll again, dont regen stamina.
            machine.UnsetFlag(PlayerFlags.TriesToIdle | PlayerFlags.CanAttack | PlayerFlags.CanRoll | PlayerFlags.CanRegenStamina);
            //use stamina.
            machine.UseRollStamina();
        }

        internal override void OnExit(PlayerMachine machine)
        {

        }
    }
}
