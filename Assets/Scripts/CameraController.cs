using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulslike
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        Transform anchor;
        [SerializeField]
        Transform myTransform;

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

        //INPUT BUFFER
        private Vector2 cameraRotationInput;

        //RUNTIME VARIABLES
        private float yaw; //Y-axis
        private float pitch; //x-axis.
        private float deltaTime;
        private float currentCameraDistance;

        //INPUT MESSAGES
        /// <summary>
        /// Receives the new input value for camera rotation, when input changes.
        /// </summary>
        public void OnCameraRotate(InputValue input)
        {
            cameraRotationInput = input.Get<Vector2>();
        }

        //Unity Messages
        private void Start()
        {
            
        }

        //Update the camera based on the input.
        private void LateUpdate()
        {
            deltaTime = Time.deltaTime;
            HandleRotation();
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
            float pitchDelta = (invertY ? -1f : 1f) * rotationDelta * cameraRotationInput.y;
            pitch = Mathf.Clamp(pitch + pitchDelta, minimumYRotation, maximumYRotation);
            //change yaw
            yaw += rotationDelta * cameraRotationInput.x;
            //ew euler, but its simple and works.
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            //assign it.
            anchor.rotation = rotation;
        }

        /// <summary>
        /// Makes sure the camera doesnt clip through any solid colliders present.
        /// </summary>
        private void HandleCameraOcclusion()
        {
            //the ray starts at the anchor of the camera inside the player, and goes out the back.
            Ray ray = new Ray(anchor.position, -anchor.forward);
            float distance = cameraDistance;
            if (Physics.SphereCast(ray, clearanceRadius, out RaycastHit hit, cameraDistance, cameraCollisionMask))
            {
                distance = hit.distance;
            }
            //zoom back out slowly
            if (distance > currentCameraDistance)
            {
                currentCameraDistance = currentCameraDistance + distance * deltaTime; //ew
            }
            else //but clip in instantly
            {
                currentCameraDistance = distance;
            }
            myTransform.localPosition = new Vector3(0, 0, -currentCameraDistance);
        }

        /// <summary>
        /// rotates a Vector on the global Y axis to align it with the XZ space of the camera.
        /// </summary>
        internal Vector3 WorldToCameraXZ(Vector3 initial)
        {
            return Quaternion.Euler(0, yaw, 0) * initial;
        }

    }
}