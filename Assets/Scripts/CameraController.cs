using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulslike
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform anchor;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("myTransform")]
        private Transform cameraTransform;
        [SerializeField]
        private Transform playerFollowAnchor;

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
        [SerializeField]
        private float maxEnemyDistance = 18f;

        //INPUT BUFFER
        private Vector2 cameraRotationInput;

        //RUNTIME VARIABLES
        private float yaw; //Y-axis
        private float pitch; //x-axis.
        private float deltaTime;
        private float currentCameraDistance;

        private EnemyBase targetEnemy;
        private Transform lookTarget;
        internal Transform LookTarget //Note: when the enemy dies, this should update.
        { 
            get => lookTarget; 
            private set 
            { 
                if (value != lookTarget)
                    OnTargetChanged?.Invoke(value); 
                lookTarget = value; 
            } 
        }
        private List<EnemyBase> possibleTargets = new List<EnemyBase>(15);

        internal event System.Action<Transform> OnTargetChanged;

        //INPUT MESSAGES
        /// <summary>
        /// Receives the new input value for camera rotation, when input changes.
        /// </summary>
        public void OnCameraRotate(InputValue input)
        {
            cameraRotationInput = input.Get<Vector2>();
        }

        //LockTarget input.
        public void OnLockTarget()
        {
            Debug.Log("OnLockTarget");
            if(LookTarget) //if we have a target already, unassign it.
            {
                LookTarget = null;
            }
            else //go search for a target.
            {
                if (enemiesInView.Count == 0) //dont even try if its 0.
                    return;
                Debug.Log("Some Enemies In View.");
                Vector3 camPos = cameraTransform.position;
                //sort the enemies array by total distance
                int validTargets = this.SortEnemiesByDistanceWithinMaximum(camPos);
                //sort from left to right (relative to camera)
                this.SortEnemiesByHorizontalDistance(validTargets, camPos);
                this.GetEnemiesInLineOfSightNonAlloc(possibleTargets, validTargets, camPos);

                if (possibleTargets.Count == 0) //no valid target found, return
                    return;
                Debug.Log("Valid Targets Found.");
                //assign the targetEnemy. should later be done with a property and onchange event to correctly check the OnEnemyDeath event to deselect the enemy as target.
                targetEnemy = possibleTargets[0]; //0th element because SortEnemiesByHorizontalDistance now sorts by least absolute distance.
                LookTarget = targetEnemy.transform;
            }
        }

        //Unity Messages
        private void Awake()
        {
            enemiesInView = new List<EnemyBase>(15);
        }

        //Update the camera based on the input.
        private void LateUpdate()
        {
            deltaTime = Time.deltaTime;
            FollowPlayer();
            HandleRotation();
            HandleCameraOcclusion();
        }

        /// <summary>
        /// Handles XYZ movement on the anchor to follow the playerfollowanchor through the world.
        /// </summary>
        private void FollowPlayer()
        {
            anchor.position = playerFollowAnchor.position;
        }

        /// <summary>
        /// Uses the cameraRotationInput to rotate the character and the camera.
        /// </summary>
        private void HandleRotation()
        {
            if (LookTarget == null) //default rotation behaviour based on user input.
            {
                //cache the rotation delta for the current timestep.
                float rotationDelta = deltaTime * cameraSpeed;
                //clamp pitch
                float pitchDelta = (invertY ? -1f : 1f) * rotationDelta * cameraRotationInput.y;
                pitch = Mathf.Clamp(pitch + pitchDelta, minimumYRotation, maximumYRotation);
                //change yaw
                yaw += rotationDelta * cameraRotationInput.x;
                //update the rotation based on pitch and yaw.
                //ew euler, but its simple and works.
                Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
                //assign it.
                anchor.rotation = rotation;
            }
            else //rotation behaviour when locked onto a target.
            {
                Vector3 direction = LookTarget.position - anchor.position;
                direction.Normalize();
                //Vector3 angles = Quaternion.LookRotation(direction, Vector3.up).eulerAngles;
                //Vector3 forwardAxis = Vector3.forward;
                //yaw = Vector3.SignedAngle(forwardAxis, direction, Vector3.up);
                //pitch = Vector3.SignedAngle(forwardAxis, direction, playerFollowAnchor.right);
                //pitch = Mathf.Clamp(angles.x, minimumYRotation, maximumYRotation);
                //yaw = angles.x;
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                anchor.rotation = rotation;
                yaw = rotation.eulerAngles.y;
                
            }
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
            cameraTransform.localPosition = new Vector3(0, 0, -currentCameraDistance);
        }

        /// <summary>
        /// rotates a Vector on the global Y axis to align it with the XZ space of the camera.
        /// </summary>
        internal Vector3 WorldToCameraXZ(Vector3 initial)
        {
            return Quaternion.Euler(0, yaw, 0) * initial;
        }

        //ENEMY DETECTION
        static List<EnemyBase> enemiesInView = new List<EnemyBase>(15);

        /// <summary>
        /// unregisters an enemy as visible to the camera.
        /// </summary>
        /// <param name="enemy"></param>
        public static void RegisterEnemyInView(EnemyBase enemy)
        {
            if (enemiesInView.Contains(enemy) is false)
                enemiesInView.Add(enemy);
        }

        /// <summary>
        /// unregisters an enemy as visible to the camera.
        /// </summary>
        /// <param name="enemy"></param>
        public static void UnregisterEnemyInView(EnemyBase enemy)
        {
            if (enemiesInView.Contains(enemy))
                enemiesInView.Remove(enemy);
        }

        /// <summary>
        /// Sorts the enemiesInView based on their distance from the camera.
        /// </summary>
        /// <param name="pos">The world-space position of the camera.</param>
        /// <returns>The amount of enemies that are valid for selection as target.</returns>
        private int SortEnemiesByDistanceWithinMaximum(Vector3 pos)
        {
            if (enemiesInView.Count <= 1) //if there are one or zero enemies visible, stop.
                return enemiesInView.Count;

            enemiesInView.Sort(
                (a, b) => 
                Vector3.SqrMagnitude(a.transform.position - pos) < 
                Vector3.SqrMagnitude(b.transform.position - pos) ? -1 : 1
                ); //the first element should be the one with the least distance from the camera.
            float maxDistSqr = maxEnemyDistance * maxEnemyDistance;
            //count how many enemies are within the maximum allowed distance (sqr dist because its faster)
            for(int i = 0; i < enemiesInView.Count; i++)
            {
                if (Vector3.SqrMagnitude(enemiesInView[i].transform.position - pos) > maxDistSqr)
                    return i;
            }
            return enemiesInView.Count;
        }

        /// <summary>
        /// Sorts enemies based on their horizontal distance from the camera.
        /// </summary>
        /// <param name="pos">The world-space position of the camera.</param>
        /// <param name="countToSort">the amount of items to sort, given by SortEnemiesByDistanceWithinMaximum</param>
        private void SortEnemiesByHorizontalDistance(int countToSort, Vector3 pos)
        {
            if (countToSort <= 1) //sorting one element is useless, sorting zero impossible.
                return;
            Vector3 right = anchor.right;
            enemiesInView.Sort(0, countToSort, new EnemyDotComparer(pos, right));
        }

        /// <summary>
        /// Gets all enemies from enemiesInView that arent blocked by line of sight.
        /// </summary>
        /// <param name="output">the list to populate</param>
        /// <param name="countToCheck">how many enemies to check; see SortEnemiesByDistanceWithinMaximum</param>
        /// <param name="pos">camera position</param>
        /// <returns>the amount of objects in the output array.</returns>
        private int GetEnemiesInLineOfSightNonAlloc(List<EnemyBase> output, int countToCheck, Vector3 pos)
        {
            int count = 0;
            output.Clear();
            for(int i = 0; i < countToCheck; i++)
            {
                //skip all the ones where something is between the enemy and the camera.
                if (Physics.Linecast(pos, enemiesInView[i].transform.position, cameraCollisionMask, QueryTriggerInteraction.Ignore))
                    continue;
                output.Add(enemiesInView[i]);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Compares two EnemyBase objects based on their relative position to the camera using the Dot Product of the offset and the cmameras right vector.
        /// </summary>
        private struct EnemyDotComparer : IComparer<EnemyBase>
        {
            internal Vector3 position;
            internal Vector3 right;

            internal EnemyDotComparer(Vector3 pos, Vector3 r)
            {
                position = pos;
                right = r;
            }

            public int Compare(EnemyBase a, EnemyBase b)
            {
                //Old: sorts by left to right.
                //return Vector3.Dot(a.transform.position - position, right) > Vector3.Dot(b.transform.position - position, right) ? 1 : -1;
                //New: sort by absolute left ro right (center to outside) -> element 0 will be closest to center of screen.
                return (Mathf.Abs(Vector3.Dot(a.transform.position - position, right)) > Mathf.Abs(Vector3.Dot(b.transform.position - position, right)) ? 1 : -1);
            }
        }
    }
}