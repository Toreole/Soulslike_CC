using UnityEngine;
using PlayerFlags = Soulslike.PlayerMachine.PlayerFlags;

namespace Soulslike
{
    /// <summary>
    /// Handles all the standard attacks (BasicAttacks array)
    /// </summary>
    internal class AttackingState : PlayerState
    {

        private int attackIndex = 0;

        internal override void OnAnimatorMove(PlayerMachine machine, float deltaTime)
        {
            //apply root motion
            machine.Animator.ApplyBuiltinRootMotion();
            //rotate towards stick direction
            if(machine.HasFlag(PlayerFlags.CanRotate))
                machine.RotateTowards(machine.GetWorldSpaceInput());
            //if the player can roll cancel, that means the attack animation is "done" and returning to idle, you can start a new attack
            if (machine.HasFlag(PlayerFlags.CanAttack) && machine.HasValidAttackInput)
            {
                machine.HasValidAttackInput = false; //unset the attack input.
                machine.UnsetFlag(PlayerFlags.TriesToIdle | PlayerFlags.CanAttack); //just like in OnEnter
                //try to increment the attackIndex.
                attackIndex++;
                attackIndex %= machine.BasicAttacks.Length;
                {
                    SetAttackByIndex(machine, attackIndex);
                    machine.UnsetFlag(PlayerFlags.CanRoll); //instead of machine.AllowRollCancel = false;
                    machine.SetFlag(PlayerFlags.CanRotate);
                }
            }

        }

        internal override PlayerState MoveNextState(PlayerMachine machine)
        {
            if (machine.HasValidRollInput && machine.HasFlag(PlayerFlags.CanRoll))
                return new RollingState();
            return base.MoveNextState(machine);
        }

        internal override void OnEnter(PlayerMachine machine)
        {
            //reset the attack index to 0.
            SetAttackByIndex(machine, 0);
            //setup everything the animator needs to animate the attack.
            machine.PlayAnimationID(PlayerAnimationUtil.animationID_attack);
            machine.SetFlag(PlayerFlags.CanRotate); //enable rotation at the start, is disabled by the animation later on.
            machine.UnsetFlag(PlayerFlags.TriesToIdle | PlayerFlags.CanAttack | PlayerFlags.CanRegenStamina); //disable attack input temporarily.
        }

        private void SetAttackByIndex(PlayerMachine machine, int index)
        {
            attackIndex = index;
            machine.Animator.SetInteger(PlayerAnimationUtil.paramID_attackIndex, index);
            machine.CurrentAttack = machine.BasicAttacks[index];
        }

        internal override void OnExit(PlayerMachine machine)
        {
            SetAttackByIndex(machine, 0);
        }

        //private float GetAttackAnimationLength()
        //{
        //    return (machine.BasicAttacks[attackIndex].associatedAnimation.length);
        //}
    }
}
