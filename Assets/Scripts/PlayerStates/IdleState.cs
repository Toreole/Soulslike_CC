using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulslike
{
    internal class IdleState : PlayerState
    {
        public IdleState(PlayerMachine machine) : base(machine)
        {
        }

        public override int Priority => BasePriority;

        protected override int BasePriority => 0;

        internal override void OnAnimatorMove(float deltaTime)
        {
            //Idle does not move.
        }

        internal override void OnEnter()
        {
            machine.UpdateRelativeAnimatorSpeedsBasedOnWorldMovement(new Vector3(0,0,0));
            Debug.Log("IdleState");
            machine.PlayAnimationID(BasePriority);
        }

        internal override void OnExit()
        {
            //quit doing nothing LOL
        }
    }
}