using UnityEngine;

namespace Soulslike
{
    /// <summary>
    /// Handles all the standard attacks (BasicAttacks array)
    /// </summary>
    internal class AttackingState : PlayerState
    {
        public AttackingState(PlayerMachine machine) : base(machine)
        {
        }

        private int attackIndex = 0;
        private float enterTime = 0;
        private float TimeSinceEnter => Time.time - enterTime;

        public override int Priority => (GetAttackAnimationLength() <= TimeSinceEnter)? 0 : BasePriority;

        protected override int BasePriority => 70;

        internal override void OnAnimatorMove(float deltaTime)
        {
            machine.Animator.ApplyBuiltinRootMotion();
        }

        internal override void OnEnter()
        {
            Debug.Log("AttackingState");
            //set the enter time
            enterTime = Time.time;
            //reset the attack index to 0.
            attackIndex = 0;
            //setup everything the animator needs to animate the attack.
            machine.PlayAnimationID(BasePriority);
        }

        internal override void OnExit()
        {
            enterTime = float.MaxValue; //magic
        }

        private float GetAttackAnimationLength()
        {
            return (machine.BasicAttacks[attackIndex].associatedAnimation.averageDuration);
        }
    }
}
