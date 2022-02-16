using UnityEngine;

namespace Soulslike
{
    internal class MovingState : PlayerState
    {
        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            //get the movement input in worldspace.
            Vector3 worldDirection = machine.GetWorldSpaceInput();
            worldDirection.Normalize();

            //fetch current speed
            float speed = machine.CurrentMovementSpeed;
            //setup velocity
            Vector3 movement = worldDirection * (speed);
            //update animation
            machine.UpdateRelativeAnimatorSpeedsBasedOnWorldMovement(movement);
            //multiply with deltatime to get the movement step
            movement *= deltaTime;
            //set the y movement to -4 to fake gravity, so that the player character doesnt walk on air
            movement.y = -4;
            //move the charactercontroller
            machine.CharacterController.Move(movement);
            //align the transform with the movement direction
            machine.transform.forward = worldDirection;
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_move);
            machine.AllowRollCancel = true;
        }

        internal override void OnExit(PlayerMachine machine)
        {
        }
    }
}
