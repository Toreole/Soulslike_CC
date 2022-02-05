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
        [SerializeField, Tooltip("How much time can pass at max between an input for a roll, and the roll itself."), Range(0.01f, 0.25f)]
        private float rollInputTimeFrame = 0.05f;

        //ALL THE STATES
        private PlayerState activeState;

        private IdleState idleState;
        private MovingState movingState;
        private AttackingState attackingState;

        //EDITOR ONLY
#if UNITY_EDITOR
        public bool showAttackHitbox;
        public int selectedAttackIndex;
        private bool runtimeShowAttackHitbox = false;
#endif

        //INPUT BUFFERS
        private Vector2 movementInput;
        private bool isSprinting;
        private bool isGrounded;
        private bool shouldAttack;
        //rolling needs the input press, plus the last time it was pressed.
        private bool rollInput;
        private float rollInputTime;

        //Allow cancelling the current animation/state with a roll?
        private bool allowRollCancel = true;

        /// <summary>
        /// Does the PlayerMachine have a currently valid roll INPUT.
        /// Determined by the button being pressed recently, and not consumed yet, while the time elapsed since the press is within the defined timeframe.
        /// </summary>
        internal bool HasValidRollInput => rollInput && (Time.time < rollInputTime + rollInputTimeFrame);
        internal Vector2 MovementInput => movementInput;
        internal bool IsSprinting => isSprinting;
        internal float CurrentMovementSpeed
        {
            get
            {
                return isSprinting ? runSpeed : walkSpeed;
            }
        }

        internal CharacterController CharacterController => characterController;

        public AttackDefinition[] BasicAttacks => basicAttacks;
        internal Animator Animator => animator;

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
        private void OnAnimatorMove()
        {
            activeState.OnAnimatorMove(Time.deltaTime);
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
            //this is entirely based on priority order, and the conditions that are met.
            //1. PlayerDeathState //priority 100
            //2. PlayerFallState, PlayerLandState //priority 90

            //3. PlayerRollState //priority 80
            //4. PlayerAttackState //priority 70
            if(shouldAttack && activeState.Priority < 70)
            {
                if(activeState != attackingState)
                {
                    SetActiveState(attackingState);
                    return;
                }
                else
                {
                    //INCREASE THE ATTACK INDEX; ASSIGN NEW ATTACK; INCREASE ANIMATOR ATTACK PROPERTY
                }
                shouldAttack = false; //consume the attack input.
            }

            //6. PlayerMoveState //handles movement //might be able to merge with StrafeState. //priority 20
            //7. PlayerIdleState //only happens when nothing is going on, is the default state. //priority 0
            if(movementInput != Vector2.zero && activeState.Priority < 20)
            {
                if (activeState != movingState)
                {
                    SetActiveState(movingState);
                    return;
                }
            }
            else if(activeState.Priority == 0)//if nothing else happens, go back to idle.
            {
                if (activeState != idleState)
                    SetActiveState(idleState);
            }
        }

        /// <summary>
        /// This initializes all states that will be used.
        /// </summary>
        private void InitializeAllStates()
        {
            idleState = new IdleState(this);
            movingState = new MovingState(this);
            attackingState = new AttackingState(this);
        }

        internal void SetActiveState(PlayerState state)
        {
            activeState?.OnExit();
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
           
            //align the worldspace direction with view.
            return cameraController.WorldToCameraXZ(direction);
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

        //ANIMATION EVENTS

        public void SetRollEnabled(int value)
        {
            allowRollCancel = value == 1;
        }
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
            currentAttack = basicAttacks[0]; //REMOVE THIS
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
                rollInput = input.isPressed;
                rollInputTime = Time.time;
                //setting the animation trigger should be done in the RollingState.OnEnter
                //animator.SetTrigger("Roll");
            }
        }

        public void OnAttack()
        {
            shouldAttack = true;
        }

        public void OnLockTarget()
        {

        }
    }
}
