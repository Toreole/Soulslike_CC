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

        internal override void OnAnimatorMove()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnEnterGround()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnExit()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnLeaveGround()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnPlayerDeath()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnReceiveHit()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnRoll()
        {
            throw new System.NotImplementedException();
        }
    }
}