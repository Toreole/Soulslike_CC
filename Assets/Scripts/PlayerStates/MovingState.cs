﻿using UnityEngine;
using PlayerFlags = Soulslike.PlayerMachine.PlayerFlags;

namespace Soulslike
{
    internal class MovingState : PlayerState
    {
        private float currentSpeed = 0f;

        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            //get the movement input in worldspace.
            Vector3 worldDirection = machine.GetWorldSpaceInput();
            worldDirection.Normalize();

            //align the transform with the movement direction
            machine.RotateTowards(worldDirection);

            //update currentSpeed
            currentSpeed = Mathf.MoveTowards(currentSpeed, machine.CurrentMovementSpeed, 40f * deltaTime); //just use absurd acceleration, transition should be near instant but noticable.
            //the relative allowed speed based on the dot of worldDirection and transform.forward:
            float speedMultiplier = Vector3.Dot(worldDirection, machine.transform.forward);
            //setup velocity in the direction of the transforms forward (after the rotation)
            Vector3 movement = machine.transform.forward * (currentSpeed * speedMultiplier);
            //update animation
            machine.UpdateRelativeAnimatorSpeedsBasedOnWorldMovement(movement);
            //set the y movement to -4 to fake sticking to the ground, so that the player character doesnt walk on air
            movement.y = -4;
            //multiply with deltatime to get the movement step
            movement *= deltaTime;
            //move the charactercontroller
            machine.CharacterController.Move(movement);
        }

        internal override PlayerState MoveNextState(PlayerMachine machine)
        {
            if (machine.IsGrounded == false)
                return new FallingState();
            if (machine.HasValidRollInput && machine.HasFlag(PlayerFlags.CanRoll)) //technically these flag checks are unnecessary, but good to keep them in anyway i think.
                return new RollingState();
            if (machine.HasValidAttackInput && machine.HasFlag(PlayerFlags.CanAttack))
                return new AttackingState();
            if (machine.MovementInput == Vector2.zero)
                return new IdleState();
            return this;
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_move);
            machine.UnsetFlag(PlayerFlags.TriesToIdle);
            machine.SetFlag(PlayerFlags.CanRoll | PlayerFlags.CanAttack | PlayerFlags.CanRotate);
        }

        internal override void OnExit(PlayerMachine machine)
        {
        }
    }
}
