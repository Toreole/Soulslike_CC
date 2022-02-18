using System;
using UnityEngine;

namespace Soulslike
{
    /// <summary>
    /// A bool used specifically for player input, that buffers the input for a given duration (timeFrame) which is serialized.
    /// </summary>
    [Serializable]
    public class BufferedInputBool
    {
        [SerializeField, Range(0.01f, 0.3f)]
        private float timeFrame = 0.1f;

        private float inputTime = -100;
        private bool inputActive = false;

        /// <summary>
        /// returns true only when the input is Set() and the time since the last Set is less than the allowed timeFrame.
        /// </summary>
        public bool IsActiveAndValid
        {
            get 
            {
                return inputActive && Time.time - inputTime <= timeFrame;
            }
        }

        public void Set()
        {
            inputActive = true;
            inputTime = Time.time;
        }

        public void Unset() => inputActive = false;
    }
}
