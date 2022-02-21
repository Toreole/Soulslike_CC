using UnityEngine;
using PlayerFlags = Soulslike.PlayerMachine.PlayerFlags;

namespace Soulslike
{
    internal class FallingState : PlayerState
    {
        float yVel = -1.5f;
        Vector3 xzVel = Vector3.zero;

        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            Vector3 velocity = xzVel;
            velocity.y = yVel;
            //gravity
            yVel += -10f * deltaTime;
            //decelerate on xz
            xzVel = Vector3.MoveTowards(xzVel, Vector3.zero, deltaTime * 6f);
            //apply the movement
            machine.CharacterController.Move(velocity * deltaTime);
        }

        internal override PlayerState MoveNextState(PlayerMachine machine)
        {
            if (machine.IsGrounded)
                return new LandingState();
            // if (machine.HasValidAttackInput)
            //     return new PlungeAttackState();
            return this;
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_fall);
            //unset nearly every flag.
            machine.UnsetFlag(PlayerFlags.CanRoll | PlayerFlags.CanRotate | PlayerFlags.CanMove);
            //set the attack flag to enable plunging attacks later on - this is important for soulslikes.
            machine.SetFlag(PlayerFlags.CanAttack);
            xzVel = machine.GetWorldSpaceInput() * machine.CurrentMovementSpeed;
        }

        internal override void OnExit(PlayerMachine machine)
        {
            //TODO: fall damage should be calculated in here.
        }
    }
}
