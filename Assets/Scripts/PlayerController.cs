using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulslike
{
    /// <summary>
    /// The PlayerController handles player input, and makes the character move.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private CharacterController characterController;
        [SerializeField]
        private float moveSpeed = 3f;
        [SerializeField]
        private float sprintSpeed = 5.5f;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private bool animatorUsesRootMotion = false;

        //The visual holder transform is rotated, so the character looks into the movement direction
        [SerializeField] 
        private Transform visualHolder;

        [Header("Camera Settings")]
        //the camera transform is moved in local space (z axis)
        [SerializeField]
        private Transform cameraTransform;
        [SerializeField] //the camera anchor is rotated.
        private Transform cameraAnchor;
        [SerializeField]
        private float cameraSpeed = 180f;
        [SerializeField]
        private float cameraDistance = 5f;
        [SerializeField]
        private bool invertY = true;
        [SerializeField]
        private float minimumYRotation = -75f;
        [SerializeField]
        private float maximumYRotation = 80f;
        [SerializeField, Tooltip("The radius of the sphere-cast towards the optimal camera position")]
        private float clearanceRadius = 0.2f;
        [SerializeField]
        private LayerMask cameraCollisionMask;

        //camera rotation:
        /// <summary> Rotation on local X axis </summary>
        private float pitch = 0f;
        /// <summary> Rotation on global y axis </summary>
        private float yaw = 0f;
        //the last distance the camera was using.
        private float currentCameraDistance = 5f;

        //deltaTime buffer to avoid getting it over and over again.
        private float deltaTime = 0f;

        //gravity stuff.
        private const float gravity = -14f;
        private float verticalVelocity = 0f;
        private bool isGrounded = false;
        private Vector3 lastGroundedMovement;

        //buffers for input.
        private Vector2 movementInput;
        private Vector2 cameraRotationinput;
        private bool isSprinting = false;

        //Standard MonoBehaviour Messages
        private void Update()
        {
            //cache the deltaTime.
            deltaTime = Time.deltaTime;
            HandleRotation();
            if (animatorUsesRootMotion)
            {
                UpdateAnimator();
            }
            else
            {
                HandleMovement();
            }

            HandleCameraOcclusion();
        }

        /// <summary>
        /// Uses the cameraRotationInput to rotate the character and the camera.
        /// </summary>
        private void HandleRotation()
        {
            //cache the rotation delta for the current timestep.
            float rotationDelta = deltaTime * cameraSpeed;
            //clamp pitch
            float pitchDelta = (invertY ? -1f : 1f) * rotationDelta * cameraRotationinput.y;
            pitch = Mathf.Clamp(pitch + pitchDelta, minimumYRotation, maximumYRotation);
            //change yaw
            yaw += rotationDelta * cameraRotationinput.x;
            //ew euler, but its simple and works.
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            //assign it.
            if(animatorUsesRootMotion)
                cameraAnchor.rotation = rotation;
            else
                cameraAnchor.localRotation = rotation;
        }

        /// <summary>
        /// uses the movementInput to move the character through the world.
        /// </summary>
        private void HandleMovement()
        {
            //convert the input from 2D to 3D.
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);

            if (isGrounded)
            {
                //the rotation on the y-axis.
                Quaternion rotation = Quaternion.Euler(0, yaw, 0);

                //multiply move by the speed and timestep.
                move *= (isSprinting ? sprintSpeed : moveSpeed);
                //apply the rotation, so the player moves relative to the cameras point of view.
                //this effectively transforms the vector from local space (of camera) to worldspace.
                move = rotation * move;
                
                if (movementInput != Vector2.zero) //check whether input is 0
                {
                    //get the normalized direction from the movement vector.
                    Vector3 moveDirection = move.normalized;
                    //apply it as the "rotation" of the visual representation of the character.
                    visualHolder.forward = moveDirection;
                    //inverse transform the move vector to the characters local space.
                    var relativeSpeed = visualHolder.InverseTransformVector(move);
                    //quickly send the relative speeds to the animator.
                    animator.SetFloat("relativeXSpeed", relativeSpeed.x);
                    animator.SetFloat("relativeZSpeed", relativeSpeed.z);
                }
                //set the last grounded movement.
                move *= deltaTime;
                lastGroundedMovement = move;
            }
            else //in the air
            {
                //override the move vector with the last speed we have saved.
                move = lastGroundedMovement;
            }
            animator.SetFloat("currentMoveSpeed", move.magnitude/deltaTime);
            //set the vertical velocity due to gravity.
            move.y = verticalVelocity * deltaTime;

            //move via the character controller.
            CollisionFlags collisionFlags = characterController.Move(move);
            //check for collision from below.
            if(collisionFlags.HasFlag(CollisionFlags.Below))
            {
                if(!isGrounded) //just landed on ground.
                {
                    isGrounded = true;
                    animator.SetBool("isGrounded", true);
                }
                verticalVelocity = -2f; //default of -2f vertical velocity as to allow going downhill
            }
            else
            {
                if(isGrounded) //just left ground.
                {
                    isGrounded = false;
                    animator.SetBool("isGrounded", false);
                    verticalVelocity = -0.5f; //reset the vertical velocity to something thats not just 0.
                }
                verticalVelocity += gravity * deltaTime;
            }
        }

        /// <summary>
        /// Updates relevant information for the animator, to play the correct animations.
        /// </summary>
        private void UpdateAnimator()
        {
            var actualSpeed = (isSprinting ? sprintSpeed : moveSpeed);
            //set up forward.
            var forward = cameraAnchor.forward;
            forward.y = 0;
            forward.Normalize();
            //right
            var right = cameraAnchor.right;
            Vector3 move = (movementInput.x * right + movementInput.y * forward) * actualSpeed;

            animator.SetFloat("currentMoveSpeed", actualSpeed);

            if (movementInput != Vector2.zero) //check whether input is 0
            {
                //get the normalized direction from the movement vector.
                Vector3 moveDirection = move.normalized;
                //apply it as the "rotation" of the visual representation of the character.
                visualHolder.forward = moveDirection;
                //inverse transform the move vector to the characters local space.
                var relativeSpeed = visualHolder.InverseTransformVector(move);
                //quickly send the relative speeds to the animator.
                animator.SetFloat("relativeXSpeed", relativeSpeed.x);
                animator.SetFloat("relativeZSpeed", relativeSpeed.z);
            }
            else
            {
                animator.SetFloat("currentMoveSpeed", 0);
            }
            //update grounded state.
            isGrounded = characterController.collisionFlags.HasFlag(CollisionFlags.Below);
            animator.SetBool("isGrounded", isGrounded);
                
        }

        /// <summary>
        /// Makes sure the camera doesnt clip through any solid colliders present.
        /// </summary>
        private void HandleCameraOcclusion()
        {
            //the ray starts at the anchor of the camera inside the player, and goes out the back.
            Ray ray = new Ray(cameraAnchor.position, -cameraAnchor.forward);
            float distance = cameraDistance;
            if(Physics.SphereCast(ray, clearanceRadius, out RaycastHit hit, cameraDistance, cameraCollisionMask))
            {
                distance = hit.distance;
            }
            //zoom back out slowly
            if(distance > currentCameraDistance)
            {
                currentCameraDistance = currentCameraDistance + distance * deltaTime; //ew
            }
            else //but clip in instantly
            {
                currentCameraDistance = distance;
            }
            cameraTransform.localPosition = new Vector3(0, 0, -currentCameraDistance);
        }

        //INPUT EVENTS
        /// <summary>
        /// Message sent by the Player Input script.
        /// </summary>
        public void OnControlsChanged(UnityEngine.InputSystem.PlayerInput playerInput)
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
        /// Receives the new input value for camera rotation, when input changes.
        /// </summary>
        public void OnCameraRotate(InputValue input)
        {
            cameraRotationinput = input.Get<Vector2>();
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