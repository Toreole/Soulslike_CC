using UnityEngine;

namespace Soulslike
{
    /// <summary>
    /// Handles all the standard attacks (BasicAttacks array)
    /// </summary>
    internal class AttackingState : PlayerState
    {

        private int attackIndex = 0;
        private float enterTime = 0;
        private float TimeSinceEnter => Time.time - enterTime;

        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            //apply root motion
            machine.Animator.ApplyBuiltinRootMotion();
            //rotate towards stick direction
            machine.RotateTowards(machine.GetWorldSpaceInput());
            //if the player can roll cancel, that means the attack animation is "done" and returning to idle, you can start a new attack
            if (machine.AllowRollCancel && machine.HasValidAttackInput)
            {
                machine.HasValidAttackInput = false; //unset the attack input.
                //try to increment the attackIndex.
                attackIndex++;
                attackIndex %= machine.BasicAttacks.Length;
                {
                    SetAttackByIndex(machine, attackIndex);
                    machine.AllowRollCancel = false;
                    enterTime = Time.time;
                }
            }

        }

        internal override void OnEnter(PlayerMachine machine)
        {
            //set the enter time
            enterTime = Time.time;
            //reset the attack index to 0.
            SetAttackByIndex(machine, 0);
            //setup everything the animator needs to animate the attack.
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_attack);
        }

        private void SetAttackByIndex(PlayerMachine machine, int index)
        {
            attackIndex = index;
            machine.Animator.SetInteger(PlayerAnimationUtil.paramID_attackIndex, index);
            machine.CurrentAttack = machine.BasicAttacks[index];
        }

        internal override void OnExit(PlayerMachine machine)
        {
            enterTime = float.MaxValue; //magic
            SetAttackByIndex(machine, 0);
        }

        //private float GetAttackAnimationLength()
        //{
        //    return (machine.BasicAttacks[attackIndex].associatedAnimation.length);
        //}
    }
}
