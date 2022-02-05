using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulslike
{
    internal abstract class PlayerState
    {
        //reference to the playermachine
        protected PlayerMachine machine;

        //priority of the state
        protected abstract int BasePriority { get; }
        public abstract int Priority { get; }

        public PlayerState(PlayerMachine machine)
        {
            this.machine = machine;
        }

        //internal abstract void OnRoll();
        //internal abstract void OnLeaveGround();
        //internal abstract void OnEnterGround();
        //internal abstract void OnReceiveHit();
        //internal abstract void OnPlayerDeath();
        internal abstract void OnAnimatorMove(float deltaTime);
        internal abstract void OnEnter();
        internal abstract void OnExit();
    }
}
