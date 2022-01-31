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

        //deltaTime buffer to avoid getting it over and over again.
        private float deltaTime = 0f;

        //buffers for input.
        private Vector2 movementInput;
        private Vector2 cameraRotationinput;

        //Standard MonoBehaviour Messages
        private void Update()
        {
            //cache the deltaTime.
            deltaTime = Time.deltaTime;
            HandleRotation();
            HandleMovement();

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
            cameraAnchor.localRotation = rotation;
        }

        /// <summary>
        /// uses the movementInput to move the character through the world.
        /// </summary>
        private void HandleMovement()
        {
            //convert the input from 2D to 3D.
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);

            //the rotation on the y-axis.
            Quaternion rotation = Quaternion.Euler(0, yaw, 0);
            //apply the rotation, so the player moves relative to the cameras point of view.
            move = rotation * move;

            if (movementInput != Vector2.zero) //check whether input is 0
            {
                //get the normalized direction from the movement vector.
                Vector3 moveDirection = move.normalized;
                //apply it as the "rotation" of the visual representation of the character.
                visualHolder.forward = moveDirection;
            }

            //multiply move by the speed and timestep.
            move *= (moveSpeed * deltaTime);

            //move via the character controller.
            characterController.Move(move);
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
            cameraTransform.localPosition = new Vector3(0, 0, -distance);
        }

        //INPUT EVENTS
        /// <summary>
        /// Message sent by the Player Input script.
        /// </summary>
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
        /// Receives the new input value for camera rotation, when input changes.
        /// </summary>
        public void OnCameraRotate(InputValue input)
        {
            cameraRotationinput = input.Get<Vector2>();
        }

    }
}