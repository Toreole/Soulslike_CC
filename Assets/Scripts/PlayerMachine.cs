using System.Collections.Generic;
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
        [SerializeField]
        private CharacterController characterController;

        //ATTACK DATA
        [SerializeField]
        private AttackDefinition[] basicAttacks;
        private AttackDefinition currentAttack; //the attack currently being animated.

        //MOVEMENT
        [SerializeField]
        private float walkSpeed = 3;
        [SerializeField]
        private float runSpeed = 7;
        
        //ALL THE STATES
        private PlayerState activeState;
        //private bool ignoreStatePriority = false;

        //private IdleState idleState;
        //private MovingState movingState;
        //private AttackingState attackingState;
        //private RollingState rollingState;

        //PLAYER FLAGS
        private PlayerFlags flags;

        //EDITOR ONLY
#if UNITY_EDITOR
        /// <summary>
        /// WARNING. THIS IS ONLY TO BE USED INSIDE THE EDITOR.
        /// </summary>
        public bool showAttackHitbox;
        /// <summary>
        /// WARNING. THIS IS ONLY TO BE USED INSIDE THE EDITOR.
        /// </summary>
        public int selectedAttackIndex;
        /// <summary>
        /// WARNING. THIS IS ONLY TO BE USED INSIDE THE EDITOR.
        /// </summary>
        private bool runtimeShowAttackHitbox = false;
#endif

        //INPUT BUFFERS
        private Vector2 movementInput;
        private bool isSprinting;
        private bool isGrounded;
        //BufferedInputBools need to b serialized.
        [SerializeField]
        private BufferedInputBool attackInput;
        //rolling needs the input press, plus the last time it was pressed.
        [SerializeField]
        private BufferedInputBool rollInput;

        //Allow cancelling the current animation/state with a roll?
        private bool allowRollCancel = true;
        internal bool AllowRollCancel { get { return allowRollCancel; } set { allowRollCancel = value; } }
        /// <summary>
        /// Does the PlayerMachine have a currently valid roll INPUT.
        /// Determined by the button being pressed recently, and not consumed yet, while the time elapsed since the press is within the defined timeframe.
        /// </summary>
        internal bool HasValidRollInput 
        {
            get => rollInput.IsActiveAndValid;
            set
            {
                if (value)
                    rollInput.Set();
                else
                    rollInput.Unset();
            }
        }
        internal bool HasValidAttackInput
        {
            get => attackInput.IsActiveAndValid;
            set
            {
                if (value)
                    attackInput.Set();
                else
                    attackInput.Unset();
            }
        }

        internal Vector2 MovementInput => movementInput;
        internal bool IsSprinting => isSprinting;
        internal float CurrentMovementSpeed
        {
            get
            {
                return isSprinting ? runSpeed : walkSpeed;
            }
        }
        internal bool IsGrounded => isGrounded;

        internal CharacterController CharacterController => characterController;

        public AttackDefinition[] BasicAttacks => basicAttacks;
        internal AttackDefinition CurrentAttack { get => currentAttack; set => currentAttack = value; }
        internal Animator Animator => animator;

        //BUILTIN UNITY MESSAGES
        private void Start()
        {
            //InitializeAllStates();
            //default to idleState as activeState.
            SetActiveState(new IdleState());
            characterController.Move(Vector3.down);
        }

        private void Update()
        {
            CheckForStateTransition();
        }

        //Pass through to the activeState.
        private void OnAnimatorMove()
        {
            activeState.OnAnimatorMove(this, Time.deltaTime);
            //grounded check right after movement.
            isGrounded = characterController.collisionFlags.HasFlag(CollisionFlags.Below);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            //this is something to do later on.
        }

#if UNITY_EDITOR
        //Draw gizmos when this object is selected.
        private void OnDrawGizmosSelected()
        {
            //Draw Gizmos for the selected attack
            if (showAttackHitbox && selectedAttackIndex < BasicAttacks.Length)
            {
                Gizmos.color = Color.red;
                var attack = BasicAttacks[selectedAttackIndex];
                if (attack != null)
                {
                    for (int j = 0; j < attack.hitVolumes.Length; j++)
                    {
                        var volume = attack.hitVolumes[j];
                        if (volume != null)
                        {
                            volume.DrawGizmos(transform);
                        }
                    }
                }
                
            }
        }

        private void OnDrawGizmos()
        {
            if(Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward);
                if(runtimeShowAttackHitbox)
                {
                    if(currentAttack != null)
                    {
                        for (int i = 0; i < currentAttack.hitVolumes.Length; i++)
                        {
                            if (currentAttack.hitVolumes[i] != null)
                                currentAttack.hitVolumes[i].DrawGizmos(transform);
                        }
                    }
                    runtimeShowAttackHitbox = false;
                }
            }
        }
#endif

        //PLAYERMACHINE FUNCTIONALITY
        private void CheckForStateTransition() 
        {
            var nextState = activeState.MoveNextState(this);
            if (nextState != activeState) //detect change in nextState
                SetActiveState(nextState);
        }

        ///// <summary>
        ///// This initializes all states that will be used.
        ///// </summary>
        //private void InitializeAllStates()
        //{
        //    idleState = new IdleState();
        //    movingState = new MovingState();
        //    attackingState = new AttackingState();
        //    rollingState = new RollingState();
        //}

        internal void SetActiveState(PlayerState state)
        {
            activeState?.OnExit(this);
            activeState = state;
            activeState.OnEnter(this);
        }

        /// <summary>
        /// Transforms the XY input for movement into a usable world-space direction for movement.
        /// This is not normalized.
        /// </summary>
        internal Vector3 GetWorldSpaceInput()
        {
            if (movementInput == Vector2.zero)
                return transform.forward; //default to the relative forward in case there is no input.
            //the initial, raw input in worldspace, without accounting for camera rotation.
            Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);
           
            //align the worldspace direction with view.
            return cameraController.WorldToCameraXZ(direction);
        }

        /// <summary>
        /// Rotates the PlayerMachine transform towards the given Vector (based on transform.forward !!)
        /// </summary>
        internal void RotateTowards(Vector3 forwardDirection)
        {
            float angle = Vector3.SignedAngle(transform.forward, forwardDirection, Vector3.up);
            float deltaAngle = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle), 540f * Time.deltaTime);
            Vector3 forward = Quaternion.AngleAxis(deltaAngle, Vector3.up) * transform.forward;
            transform.forward = forward;
        }

        internal void PlayAnimationID(int id)
        {
            animator.SetInteger("animationID", id);
        }

        /// <summary>
        /// Updates the relativeX and relativeZSpeed for the animator. Useed in the MovementBlendTree
        /// </summary>
        /// <param name="worldSpaceVelocity"></param>
        internal void UpdateRelativeAnimatorSpeedsBasedOnWorldMovement(Vector3 worldSpaceVelocity)
        {
            Vector3 relativeMovement = transform.InverseTransformVector(worldSpaceVelocity);
            animator.SetFloat("relativeXSpeed", relativeMovement.x);
            animator.SetFloat("relativeZSpeed", relativeMovement.z);
            //animator.SetFloat("currentMoveSpeed", relativeMovement.magnitude);
        }

        //Methods for flags
        internal void SetFlag(PlayerFlags flag)
        {
            flags |= flag;
        }
        internal void UnsetFlag(PlayerFlags flag)
        {
            flags &= ~flag;
        }
        internal void ToggleFlag(PlayerFlags flag)
        {
            flags ^= flag;
        }
        internal bool HasFlag(PlayerFlags flag)
        {
            return (flags & flag) != 0;
        }
        internal void SetFlagByName(string flagName)
        {
            if (string.IsNullOrWhiteSpace(flagName))
                return;
            SetFlag((PlayerFlags)System.Enum.Parse(typeof(PlayerFlags), flagName));
        }
        internal void UnsetFlagByName(string flagName)
        {
            if (string.IsNullOrWhiteSpace(flagName))
                return;
            UnsetFlag((PlayerFlags)System.Enum.Parse(typeof(PlayerFlags), flagName));
        }

        //ANIMATION EVENTS
        /// <summary>
        /// Once an attack animation is complete, this event will be called. enables rolling and attacking (for combos).
        /// This happens before the TriesToIdle flag is set.
        /// </summary>
        public void OnAttackComplete()
        {
            SetFlag(PlayerFlags.CanRoll | PlayerFlags.CanAttack);
        }

        /// <summary>
        /// Enables and Disables the ability to cancel out of animations/states with a roll.
        /// </summary>
        public void SetRollEnabled(int value)
        {
            if (value is 1)
                SetFlag(PlayerFlags.CanRoll);
            else 
                UnsetFlag(PlayerFlags.CanRoll);
            allowRollCancel = value == 1;
            Debug.Log($"RollCancel: {allowRollCancel}");
        }
        /// <summary>
        /// Sets whether incoming hits should be ignored.
        /// </summary>
        public void SetIFrames(int value)
        {

        }

        //sounds
        public void FootR()
        {

        }
        public void FootL()
        {

        }

        //hit detection for weapon attacks
        public void Hit()
        {
            //currentAttack = basicAttacks[0]; //REMOVE THIS
            if(currentAttack != null)
            {
                List<Collider> colliders = new List<Collider>(10);
                Collider[] hits = new Collider[10];

                for(int i = 0; i < currentAttack.hitVolumes.Length; i++)
                {
                    int count = currentAttack.hitVolumes[i].Overlap(hits, transform, int.MaxValue); //TODO LayerMask
#if UNITY_EDITOR //while in the editor, draw a gizmo that visualizes the hitbox, yep.
                    runtimeShowAttackHitbox = true;
#endif
                    for (int j = 0; j < count; j++)
                        if(colliders.Contains(hits[j]) is false)
                            colliders.Add(hits[j]);
                }
                //TODO: try to damage the hit entities.
                for(int i = 0; i < colliders.Count; i++)
                {
                    var damageable = colliders[i].GetComponent<IDamageable>();
                    if(damageable != null)
                    {
                        damageable.Damage(1);
                    }
                }

            }
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
            if (isGrounded)
            {
                rollInput.Set();
                //setting the animation trigger should be done in the RollingState.OnEnter
                //animator.SetTrigger("Roll");
            }
        }

        public void OnAttack()
        {
            attackInput.Set();
        }

        public void OnLockTarget()
        {

        }

        //player flags.
        [System.Flags]
        internal enum PlayerFlags
        {
            NONE = 0,
            CanRoll          = 1 << 0, //whether the player can cancel the current state by performing a roll
            CanMove          = 1 << 1, //whether the player is allowed to move (walk) based on input
            CanAttack        = 1 << 2, //whether an attack can be started on this frame
            IsLockedOnTarget = 1 << 3, //changes movement and camera behaviour
            TriesToIdle      = 1 << 4, //whether the machine/animator tries to go back to idle (current animation is done, and can be overridden)
            CanRotate        = 1 << 5, //whether the machine is allowed to rotate the player. 
            //IsInvincible     = 1 << 6, //whether incoming hits should be ignored. -- not necessary for the PlayerMachine, this is handled by PlayerEntity

        }
    }
}
