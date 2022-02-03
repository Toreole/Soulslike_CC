using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulslike
{
    public class PlayerMachine : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        //ALL THE STATES
        private PlayerState activeState;

        //INPUT BUFFERS
        private Vector2 movementInput;
        private bool isSprinting;
        private bool isGrounded;

        //BUILTIN UNITY MESSAGES
        private void Start()
        {
            
        }

        private void Update()
        {
            
        }

        private void OnAnimatorMove()
        {
            
        }

        private void OnAnimatorIK(int layerIndex)
        {
            
        }

        //PlayerMachine functionality:


        //ANIMATION EVENTS
        public void SetIFrame(bool b)
        {

        }

        //sounds
        public void FootR()
        {

        }
        public void FootL()
        {

        }

        //INPUT MESSAGES FROM PLAYER INPUT
        public void OnControlsChanged(PlayerInput playerInput)
        {
            Debug.Log($"Controls Changed to: {playerInput.currentControlScheme}");
        }

        /// <summary>
        /// Receives the new input value for movement, when input changes.
        /// </summary>
        public void OnMovement(InputValue input)
        {
            movementInput = input.Get<Vector2>();
        }

        /// <summary>
        /// Receives the new input value for sprinting, when input changes.
        /// </summary>
        public void OnSprint(InputValue input)
        {
            isSprinting = input.Get<float>() > 0;
        }

        public void OnRoll(InputValue input)
        {
            Debug.Log("Roll");
            if (isGrounded)
            {
                animator.SetTrigger("Roll");
                Debug.Log("Roll2");
            }
        }

        public void OnAttack()
        {
            animator.SetTrigger("Attack");
        }

        public void OnLockTarget()
        {

        }
    }
}
