using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulslike
{
    internal abstract class PlayerState
    {
        protected PlayerMachine machine;
        public PlayerState(PlayerMachine machine)
        {
            this.machine = machine;
        }

        internal abstract void OnRoll();
        internal abstract void OnLeaveGround();
        internal abstract void OnEnterGround();
        internal abstract void OnReceiveHit();
        internal abstract void OnPlayerDeath();
        internal abstract void OnAnimatorMove();
        internal abstract void OnEnter();
        internal abstract void OnExit();
    }
}
