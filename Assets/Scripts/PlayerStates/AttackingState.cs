using UnityEngine;

namespace Soulslike
{
    internal class AttackingState : PlayerState
    {
        public AttackingState(PlayerMachine machine) : base(machine)
        {
        }

        private float enterTime = 0;
        private float TimeSinceEnter => Time.time - enterTime;

        public override int Priority => (AnimationIsDone && TimeSinceEnter > 0.2f)? 0 : BasePriority;

        protected override int BasePriority => 70;

        internal override void OnAnimatorMove(float deltaTime)
        {
            machine.Animator.ApplyBuiltinRootMotion();
        }

        internal override void OnEnter()
        {
            Debug.Log("AttackingState");
            enterTime = Time.time;
        }

        internal override void OnExit()
        {

        }
    }
}
