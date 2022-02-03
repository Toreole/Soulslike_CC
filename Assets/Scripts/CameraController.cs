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

        //INPUT BUFFER
        private Vector2 cameraRotationInput;

        //RUNTIME VARIABLES
        private float yaw; //Y-axis
        private float pitch; //x-axis.

        //INPUT MESSAGES
        /// <summary>
        /// Receives the new input value for camera rotation, when input changes.
        /// </summary>
        public void OnCameraRotate(InputValue input)
        {
            cameraRotationInput = input.Get<Vector2>();
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