using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulslike
{
    public class PlayerMachine : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private CameraController cameraController;

        //ALL THE STATES
        private PlayerState activeState;

        private IdleState idleState;

        //INPUT BUFFERS
        private Vector2 movementInput;
        private bool isSprinting;
        private bool isGrounded;

        internal Vector2 MovementInput => movementInput;
        internal bool IsSprinting => isSprinting;

        //BUILTIN UNITY MESSAGES
        private void Start()
        {
            InitializeAllStates();
            //default to idleState as activeState.
            SetActiveState(idleState);
        }

        private void Update()
        {
            CheckForStateTransition();
        }

        //Pass through to the activeState.
        private void OnAnimatorMove() => activeState.OnAnimatorMove();

        private void OnAnimatorIK(int layerIndex)
        {
            //this is something to do later on.
        }

        //PLAYERMACHINE FUNCTIONALITY
        private void CheckForStateTransition()
        {
            //this is entirely based on priority order, and the conditions that are met.
            //1. PlayerDeathState
            //2. PlayerFallState, PlayerLandState
            //3. PlayerRollState
            //4. PlayerAttackState
            //5. PlayerStrafeState //handles movement when locked onto a target.
            //6. PlayerMoveState //handles movement //might be able to merge with StrafeState.
            //7. PlayerIdleState //only happens when nothing is going on, is the default state.
        }

        /// <summary>
        /// This initializes all states that will be used.
        /// </summary>
        private void InitializeAllStates()
        {
            idleState = new IdleState(this);
        }

        internal void SetActiveState(PlayerState state)
        {
            activeState.OnExit();
            activeState = state;
            activeState.OnEnter();
        }

        /// <summary>
        /// Transforms the XY input for movement into a usable world-space direction for movement.
        /// This is not normalized.
        /// </summary>
        internal Vector3 GetWorldSpaceInput()
        {
            if (movementInput == Vector2.zero)
                return Vector3.zero;
            //the initial, raw input in worldspace, without accounting for camera rotation.
            Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);
            return cameraController.WorldToCameraXZ(direction);
        }

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
