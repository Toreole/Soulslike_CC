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

        //INPUT MESSAGES
        /// <summary>
        /// Receives the new input value for camera rotation, when input changes.
        /// </summary>
        public void OnCameraRotate(InputValue input)
        {
            cameraRotationInput = input.Get<Vector2>();
        }

    }
}