using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }

        internal override void OnExit(PlayerMachine machine)
        {
            //quit doing nothing LOL
        }
    }
}