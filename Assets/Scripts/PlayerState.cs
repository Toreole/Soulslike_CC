using UnityEngine;

namespace Soulslike
{
    /// <summary>
    /// The basis for all PlayerStates (behaviours)
    /// </summary>
    internal abstract class PlayerState
    {
        //internal abstract void OnRoll();
        //internal abstract void OnLeaveGround();
        //internal abstract void OnEnterGround();
        //internal abstract void OnReceiveHit();
        //internal abstract void OnPlayerDeath();
        internal abstract void OnAnimatorMove(PlayerMachine machine, float deltaTime);

        internal virtual PlayerState MoveNextState(PlayerMachine machine)
        {
            //if(machine.PlayerIsDead) return PlayerDeadState;
            return this;
        }

        internal abstract void OnEnter(PlayerMachine machine);
        internal abstract void OnExit(PlayerMachine machine);
    }

    /// <summary>
    /// Provides static readonly properties for animationIDs, animator parameter IDs, and the like.
    /// </summary>
    internal static class PlayerAnimationUtil
    {
        //all the animation IDs.
        internal const int animationID_idle = 0;
        internal const int animationID_move = 20;
        internal const int animationID_attack = 70;
        internal const int animationID_roll = 80;
        internal const int animationID_fall = 90;
        internal const int animationID_land = 91;
        internal const int animationID_dead = 100;

        //animator parameters.
        /// <summary>
        /// integer parameter
        /// </summary>
        internal static readonly int paramID_animationID = Animator.StringToHash("animationID");
        /// <summary>
        /// integer parameter
        /// </summary>
        internal static readonly int paramID_attackIndex = Animator.StringToHash("attackIndex");
        /// <summary>
        /// float parameter
        /// </summary>
        internal static readonly int paramID_relativeX = Animator.StringToHash("relativeXSpeed");
        /// <summary>
        /// float parameter
        /// </summary>
        internal static readonly int paramID_relativeZ = Animator.StringToHash("relativeZSPeed");
        /// <summary>
        /// trigger parameter
        /// </summary>
        internal static readonly int paramID_ReRoll = Animator.StringToHash("ReRoll");
    }
}
