using System;
using UnityEngine;

namespace Soulslike
{
    [Serializable]
    public class BufferedInputBool
    {
        [SerializeField, Range(0.01f, 0.3f)]
        private float timeFrame = 0.1f;

        private float inputTime;
        private bool inputActive = true;

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
