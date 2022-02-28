using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Soulslike
{
    /// <summary>
    /// Defines an attack.
    /// </summary>
    [CreateAssetMenu(menuName = "Custom Data/Attack Definition")]
    public class AttackDefinition : ScriptableObject
    {
        public float damageMultiplier;
        public HitVolume[] hitVolumes;
        public float staminaCost;
#if UNITY_EDITOR
        //this is pretty much editor only
        public AnimationClip associatedAnimation;
#endif
    }

    /// <summary>
    /// Defines a 3D volume in which collision is detected with Physics.Overlap_ calls.
    /// </summary>
    [Serializable]
    public class HitVolume
    {
        [SerializeField, Tooltip("Should only be Sphere, Capsule, or Box.")]
        private PrimitiveType shape = 0;
        [SerializeField]
        private Vector3 relativePosition;
        [SerializeField]
        private Quaternion relativeRotation;
        [SerializeField]
        private Vector3 sizes;

        /// <summary>
        /// Gets all colliders that overlap with this hitvolume.
        /// Does not allocate.
        /// </summary>
        /// <param name="hitColliders">The array to populate with data.</param>
        /// <param name="relativeTo">The transform that this happens in relation to.</param>
        /// <param name="mask">The layermask to use for collision detection.</param>
        /// <returns>The amount of colliders found.</returns>
        public int Overlap(Collider[] hitColliders, Transform relativeTo, LayerMask mask)
        {
            //calculate things that are (almost) always needed.
            Vector3 position = relativeTo.TransformPoint(relativePosition);
            Quaternion rotation = relativeTo.rotation * relativeRotation;
            //simple switch because it's neat.
            switch (shape)
            {
                case PrimitiveType.Capsule: //Capsules use size.y as the "height" and size.x as radius.
                    float sphereDistance = sizes.y - sizes.x;
                    Vector3 halfUp = (rotation * Vector3.up) * (0.5f * sphereDistance);
                    Vector3 startPos = position + halfUp;
                    Vector3 endPos = position - halfUp;
                    return Physics.OverlapCapsuleNonAlloc(startPos, endPos, sizes.x, hitColliders, mask);
                case PrimitiveType.Sphere: //the simplest of them all.
                    return Physics.OverlapSphereNonAlloc(position, sizes.x, hitColliders, mask);
                case PrimitiveType.Cube:
                    return Physics.OverlapBoxNonAlloc(position, sizes, hitColliders, rotation, mask);
                //invalid shape selected.
                default: return 0;
            }
        }

        public void DrawGizmos(Transform relativeTo)
        {
            //calculate things that are (almost) always needed.
            Vector3 position = relativeTo.TransformPoint(relativePosition);
            Quaternion rotation = relativeTo.rotation * relativeRotation;
            //simple switch because it's neat.
            switch (shape)
            {
                case PrimitiveType.Capsule: //Capsules use size.y as the "height" and size.x as radius.
                    float sphereDistance = sizes.y - sizes.x;
                    Vector3 halfUp = (rotation * Vector3.up) * (0.5f * sphereDistance);
                    Vector3 startPos = position + halfUp;
                    Vector3 endPos = position - halfUp;
                    Gizmos.DrawWireSphere(startPos, sizes.x);
                    Gizmos.DrawWireSphere(endPos,   sizes.x);
                    Gizmos.DrawLine(endPos, startPos);
                    break;
                case PrimitiveType.Sphere: //the simplest of them all.
                    Gizmos.DrawWireSphere(position, sizes.x);
                    break;
                case PrimitiveType.Cube:
                    //need to override the gizmos matrix to account for rotation.
                    Matrix4x4 gizmoMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation);
                    Gizmos.DrawWireCube(Vector3.zero, sizes);
                    Gizmos.matrix = gizmoMatrix;
                    break;
                //invalid shape selected.
                default: break;
            }
        }
    }
}