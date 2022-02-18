using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerFlags = Soulslike.PlayerMachine.PlayerFlags;

namespace Soulslike
{
    internal class IdleState : PlayerState
    {

        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            //Idle does not move.
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            machine.UpdateRelativeAnimatorSpeedsBasedOnWorldMovement(new Vector3(0,0,0));
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_idle);
            machine.AllowRollCancel = true;
            machine.UnsetFlag(PlayerFlags.TriesToIdle);
            //the player can do everything in idle.
            machine.SetFlag(PlayerFlags.CanRoll | PlayerFlags.CanAttack | PlayerFlags.CanRotate);
        }

        internal override PlayerState MoveNextState(PlayerMachine machine)
        {
            if (machine.HasValidAttackInput)
                return new AttackingState();
            if(machine.HasValidRollInput)
                return new RollingState();
            if (machine.MovementInput != Vector2.zero)
                return new MovingState();
            return base.MoveNextState(machine);
        }

        internal override void OnExit(PlayerMachine machine)
        {
            //quit doing nothing LOL
        }
    }
}