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
        [SerializeField]
        private LayerMask groundMask;
        [SerializeField]
        private float maxStamina = 100f;
        [SerializeField]
        private float staminaRegen = 30f;
        [SerializeField, Tooltip("The amount of stamina consumed by each roll.")]
        private float rollStaminaCost = 22f;
        [SerializeField, Tooltip("The Stamina consumed PER SECOND while sprinting.")]
        private float sprintStaminaCost = 8f;

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
        private bool isGrounded = true;
        //BufferedInputBools need to be serialized for timeFrame to be freely adjustable.
        [SerializeField]
        private BufferedInputBool attackInput;
        [SerializeField]
        private BufferedInputBool rollInput;

        //Stamina.
        private float stamina;
        private float zeroStaminaTime; //the last time, the player completely ran out of stamina.

        internal float Stamina
        {
            get => stamina;
            private set
            {
                stamina = Mathf.Clamp(value, 0, maxStamina);
                //upon hitting zero stamina, set the time, and "disable" the sprint input
                if (Mathf.Approximately(0, Stamina))
                {
                    zeroStaminaTime = Time.time;
                    isSprinting = false;
                }
            }
        }

        //internal bool AllowRollCancel { get { return HasFlag(PlayerFlags.CanRoll); } }
        /// <summary>
        /// Does the PlayerMachine have a currently valid roll INPUT.
        /// Determined by the button being pressed recently, and not consumed yet, while the time elapsed since the press is within the defined timeframe.
        /// </summary>
        internal bool HasValidRollInput 
        {
            get => rollInput.IsActiveAndValid && Stamina > 0;
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
            get => attackInput.IsActiveAndValid && Stamina > 0;
            set
            {
                if (value)
                    attackInput.Set();
                else
                    attackInput.Unset();
            }
        }

        internal Vector2 MovementInput => movementInput;
        internal bool IsSprinting => isSprinting && Stamina > 0;
        /// <summary>
        /// Returns the current target movement speed. if IsSprinting is true, this will return sprintSpeed, otherwise the regular moveSpeed.
        /// </summary>
        internal float CurrentMovementSpeed
        {
            get
            {
                return IsSprinting ? runSpeed : walkSpeed;
            }
        }
        internal bool IsGrounded => isGrounded;

        internal CharacterController CharacterController => characterController;

        public AttackDefinition[] BasicAttacks => basicAttacks;
        internal AttackDefinition CurrentAttack { get => currentAttack; set => currentAttack = value; }
        internal Animator Animator => animator;
        internal Transform LockedTarget { get; set; }

        //BUILTIN UNITY MESSAGES
        private void Start()
        {
            //InitializeAllStates();
            //default to idleState as activeState.
            SetActiveState(new IdleState());
            characterController.Move(Vector3.down);
            stamina = maxStamina;
            //add the OnTargetChanged event. No need to remove since PlayerMachine and CameraController are on the same object, and depend on each other.
            cameraController.OnTargetChanged += OnTargetChanged; 
        }

        private void Update()
        {
            CheckForStateTransition();
            RegenStamina();
        }

        //Pass through to the activeState.
        private void OnAnimatorMove()
        {
            activeState.OnAnimatorMove(this, Time.deltaTime);
            //grounded check right after movement.
            CheckForGround();
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
        /// <summary>
        /// Calls PlayerState.MoveNextState on the activeState. Checks for a change in states, then transitions them.
        /// </summary>
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

        /// <summary>
        /// Sets the activeState, calls OnExit and OnEnter appropriately.
        /// </summary>
        /// <param name="state">The new state to transition to.</param>
        private void SetActiveState(PlayerState state)
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
            float deltaAngle = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle), 720f * Time.deltaTime);
            Vector3 forward = Quaternion.AngleAxis(deltaAngle, Vector3.up) * transform.forward;
            transform.forward = forward;
        }

        /// <summary>
        /// A shorter way for the PlayerStates to set the animationID parameter of the animator.
        /// </summary>
        /// <param name="id">the id of the animation. see PlayerAnimationUtil.</param>
        internal void PlayAnimationID(int id)
        {
            animator.SetInteger(PlayerAnimationUtil.paramID_animationID, id);
        }

        /// <summary>
        /// Updates the relativeX and relativeZSpeed for the animator. Useed in the MovementBlendTree
        /// </summary>
        /// <param name="worldSpaceVelocity">the world-space velocity that is being used to move the player.</param>
        internal void UpdateRelativeAnimatorSpeedsBasedOnWorldMovement(Vector3 worldSpaceVelocity)
        {
            Vector3 relativeMovement = transform.InverseTransformVector(worldSpaceVelocity);
            animator.SetFloat("relativeXSpeed", relativeMovement.x);
            animator.SetFloat("relativeZSpeed", relativeMovement.z);
            //animator.SetFloat("currentMoveSpeed", relativeMovement.magnitude);
        }

        /// <summary>
        /// Regen the players stamina when the CanRegenStamina flag is set active.
        /// </summary>
        private void RegenStamina()
        {
            //only regen stamina if the flag is set, and at least half a second has gone by since the player ran out completely.
            if(this.HasFlag(PlayerFlags.CanRegenStamina) && Time.time - zeroStaminaTime > 0.5f)
            {
                Stamina += staminaRegen * Time.deltaTime;
            }
        }

        internal void UseSprintStamina() => Stamina -= sprintStaminaCost * Time.deltaTime;
        internal void UseRollStamina() => Stamina -= rollStaminaCost;

        /// <summary>
        /// Checks for ground below the player in order to update isGrounded.
        /// </summary>
        private void CheckForGround()
        {
            //bool lastGround = isGrounded;
            //first, check based on the characterController collision flags.
            isGrounded = characterController.collisionFlags.HasFlag(CollisionFlags.Below);
            //second, do a check for a surface below the player using a raycast.
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.1f, groundMask, QueryTriggerInteraction.Ignore))
            {
                //the slope of the ground has a maximum.
                isGrounded |= Vector3.Angle(hit.normal, Vector3.up) < 45;
            }
            //if (isGrounded && !lastGround)
            //    Debug.Log("landed");
            //else if (!isGrounded && lastGround)
            //    Debug.Log("left ground");
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

        /// <summary>
        /// Update the target selected by the cameraController.
        /// </summary>
        /// <param name="newTarget"></param>
        private void OnTargetChanged(Transform newTarget)
        {
            this.LockedTarget = newTarget;
            if (newTarget != null)
                this.SetFlag(PlayerFlags.IsLockedOnTarget);
            else
                this.UnsetFlag(PlayerFlags.IsLockedOnTarget);
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
        }
        /// <summary>
        /// Sets whether incoming hits should be ignored.
        /// </summary>
        //NOTE: THIS SHOULD BE MOVED OVER TO PLAYERENTITY
        public void SetIFrames(int value)
        {

        }

        //SOUNDS - THESE ARE TO BE MOVED TO A SEPERATE SCRIPT, AS TO NOT OVERLOAD THIS CLASS.
        public void FootR()
        {

        }
        public void FootL()
        {

        }

        public void Land()
        {

        }

        //hit detection for weapon attacks
        /// <summary>
        /// Does the overlap checks for the current attack. Only to be triggered via an animation event.
        /// </summary>
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

        /// <summary>
        /// Sets the rollInput buffered bool when the player is grounded.
        /// </summary>
        /// <param name="input"></param>
        public void OnRoll(InputValue input)
        {
            if (isGrounded)
            {
                rollInput.Set();
                //setting the animation trigger should be done in the RollingState.OnEnter
                //animator.SetTrigger("Roll");
            }
        }

        /// <summary>
        /// Sets the attackInput buffered bool.
        /// </summary>
        public void OnAttack()
        {
            attackInput.Set();
        }

        ///// <summary>
        ///// Upon receiving the LockTarget input, toggle between having a target and not having one.
        ///// </summary>
        //public void OnLockTarget()
        //{
        //    if (HasFlag(PlayerFlags.IsLockedOnTarget) is false)
        //    {
        //        if (cameraController.TryGetTarget(out Transform target))
        //        {
        //            LockedTarget = target;
        //            SetFlag(PlayerFlags.IsLockedOnTarget);
        //            Debug.Log("Locked Target.");
        //        }
        //        //LockedTarget = new GameObject("temporary 000 target").transform;
        //        //cameraController.LookTarget = LockedTarget;
        //    }
        //    else
        //    {
        //        UnsetFlag(PlayerFlags.IsLockedOnTarget);
        //        cameraController.LookTarget = null;
        //        Debug.Log("Removed Target.");
        //    }
        //}

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
            CanRegenStamina  = 1 << 6, //whether the machine should process the natural stamina regeneration.

        }
    }
}
