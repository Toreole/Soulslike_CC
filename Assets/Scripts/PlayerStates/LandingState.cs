using UnityEngine;
using PlayerFlags = Soulslike.PlayerMachine.PlayerFlags;

namespace Soulslike
{
    internal class LandingState : PlayerState
    {
        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            //landing doesnt really move at all.
        }

        internal override PlayerState MoveNextState(PlayerMachine machine)
        {
            if (machine.HasValidRollInput && machine.HasFlag(PlayerFlags.CanRoll))
                return new RollingState();
            return base.MoveNextState(machine);
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_land);
            //when landing, the player cant move, attack, or rotate, but can immediately roll.
            machine.UnsetFlag(PlayerFlags.CanMove | PlayerFlags.CanAttack | PlayerFlags.CanRotate);
            machine.SetFlag(PlayerFlags.CanRoll);
            //TODO: fall damage.
        }

        internal override void OnExit(PlayerMachine machine)
        {

        }
    }
}
