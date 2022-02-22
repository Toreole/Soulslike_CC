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
        [SerializeField]
        Vector3 lockedAnchorOffset = new Vector3(0, 0.5f, 0);
        [SerializeField]
        private float maxVelocity = 8;
        [SerializeField]
        private float moveSmoothTime = 0.6f;

        //INPUT BUFFER
        private Vector2 cameraRotationInput;

        //RUNTIME VARIABLES
        private float yaw; //Y-axis
        private float pitch; //x-axis.
        private float deltaTime;
        private float currentCameraDistance;
        private Vector3 currentVelocity;
        private float currentRotationSpeed;

        /// <summary> The currently selected target enemy.</summary>
        private EnemyTarget targetEnemy;
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
        private List<EnemyTarget> possibleTargets = new List<EnemyTarget>(15);
        //new:
        //targetStack acts as a sort of "pool" for the EnemyTargets.
        private Stack<EnemyTarget> targetStack = new Stack<EnemyTarget>(15);
        private List<EnemyTarget> targetsInView = new List<EnemyTarget>(15);
        //for switching targets.
        private bool canSwitchTarget = false;

        internal event System.Action<Transform> OnTargetChanged;

        //static instance.
        private static CameraController instance;

        //INPUT MESSAGES
        /// <summary>
        /// Receives the new input value for camera rotation, when input changes.
        /// </summary>
        public void OnCameraRotate(InputValue input)
        {
            cameraRotationInput = input.Get<Vector2>();
            //when locked onto a target, enable switching once the input has returned on (0, 0)
            if(LookTarget != null)
            {
                if (cameraRotationInput == Vector2.zero) //NOTE: this is EXTREMELY sensitive on mouse controls right now. only works nicely on controller.
                    canSwitchTarget = true;
                else if(canSwitchTarget)
                {
                    canSwitchTarget = false;
                    SwitchTarget(cameraRotationInput);
                    Debug.Log("switch!");
                }
            }
            
        }

        //LockTarget input.
        public void OnLockTarget()
        {
            Debug.Log("OnLockTarget");
            if(LookTarget) //if we have a target already, unassign it.
            {
                LookTarget = null;
                currentRotationSpeed = 0;
            }
            else //go search for a target.
            {
                if (targetsInView.Count == 0) //dont even try if its 0.
                    return;
                //Debug.Log($"{targetsInView.Count} Enemies In View.");
                Vector3 camPos = anchor.position; //anchor position instead of camera position.
                Vector3 camRight = anchor.right;
                //recalculate the sqrDistance, and position for all the targetsInView.
                for (int i = 0; i < targetsInView.Count; i++)
                    targetsInView[i].RecalculateParams(camPos, camRight);

                //sort the enemies array by total distance
                int validTargets = this.SortEnemiesByDistanceWithinMaximum();
                //sort from left to right (relative to camera)
                this.SortEnemiesByWeight(validTargets);
                this.GetEnemiesInLineOfSightNonAlloc(possibleTargets, validTargets, camPos);

                if (possibleTargets.Count == 0) //no valid target found, return
                    return;
                //Debug.Log($"{possibleTargets.Count} Valid Targets Found.");
                //assign the targetEnemy. should later be done with a property and onchange event to correctly check the OnEnemyDeath event to deselect the enemy as target.
                targetEnemy = possibleTargets[0]; //0th element because SortEnemiesByHorizontalDistance now sorts by least absolute distance.
                LookTarget = targetEnemy.transform;
            }
        }

        //Unity Messages
        private void Awake()
        {
            instance = this; //no extra checks, just set the instance lol

            targetsInView = new List<EnemyTarget>(15);
            targetStack = new Stack<EnemyTarget>(15);
            //setup.
            for(int i = 0; i < 15; i++)
            {
                targetStack.Push(new EnemyTarget());
            }
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
            //smoothdamp position because im lazy.
            Vector3 position = Vector3.SmoothDamp(anchor.position, playerFollowAnchor.position, ref currentVelocity, moveSmoothTime, maxVelocity);
            anchor.position = position;
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
                Vector3 direction = LookTarget.position - (anchor.position + lockedAnchorOffset);
                direction.Normalize();
                //Vector3 angles = Quaternion.LookRotation(direction, Vector3.up).eulerAngles;
                //Vector3 forwardAxis = Vector3.forward;
                //yaw = Vector3.SignedAngle(forwardAxis, direction, Vector3.up);
                //pitch = Vector3.SignedAngle(forwardAxis, direction, playerFollowAnchor.right);
                //pitch = Mathf.Clamp(angles.x, minimumYRotation, maximumYRotation);
                //yaw = angles.x;
                Vector3 desiredRotation = Quaternion.LookRotation(direction, Vector3.up).eulerAngles;
                //anchor.rotation = rotation;
                yaw = Mathf.SmoothDampAngle(yaw, desiredRotation.y, ref currentRotationSpeed, .2f);
                anchor.rotation = Quaternion.Euler(desiredRotation.x, yaw, 0);
                //yaw = rotation.eulerAngles.y;
                
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
        //old:
        //static List<EnemyBase> enemiesInView = new List<EnemyBase>(15);

        /// <summary>
        /// unregisters an enemy as visible to the camera.
        /// </summary>
        /// <param name="enemy"></param>
        public static void RegisterEnemyInView(EnemyBase enemy)
        {
            instance.RegisterEnemy(enemy);
        }

        /// <summary>
        /// unregisters an enemy as visible to the camera.
        /// </summary>
        /// <param name="enemy"></param>
        public static void UnregisterEnemyInView(EnemyBase enemy)
        {
            instance.UnregisterEnemy(enemy);
        }

        //register the enemy.
        private void RegisterEnemy(EnemyBase enemy)
        {
            //check if it already exists
            if (targetsInView.Exists(x => x.enemy == enemy) is false)
                targetsInView.Add(GetTargetFromStack().As(enemy));
        }

        //Unregister the enemy.
        private void UnregisterEnemy(EnemyBase enemy)
        {
            //try to find it.
            var enemyTarget = targetsInView.Find(x => x.enemy = enemy);
            if (enemyTarget != null)
            {
                //remove the enemyTarget from active tracking, and push it back onto the stack.
                targetsInView.Remove(enemyTarget);
                targetStack.Push(enemyTarget);
            }
        }

        /// <summary>
        /// Gets an unused EnemyTarget from the targetStack, or creates a new instance if the stack is empty.
        /// </summary>
        private EnemyTarget GetTargetFromStack()
        {
            return targetStack.Count == 0 ? new EnemyTarget() : targetStack.Pop();
        }

        /// <summary>
        /// Switch to a different enemy based on the look input.
        /// </summary>
        private void SwitchTarget(Vector2 input)
        {
            //sort the enemies.
            int validTargets = this.SortEnemiesByDistanceWithinMaximum();
            //sort from left to right (relative to camera)
            this.SortEnemiesByAlignment(validTargets);
            this.GetEnemiesInLineOfSightNonAlloc(possibleTargets, validTargets, anchor.position);

            //the indices.
            int indexDelta = (int)Mathf.Sign(input.x);
            int currentIndex = possibleTargets.IndexOf(targetEnemy);
            currentIndex = Mathf.Clamp(indexDelta + currentIndex, 0, possibleTargets.Count - 1);
            this.targetEnemy = possibleTargets[currentIndex];
            this.LookTarget = targetEnemy.transform;
        }

        /// <summary>
        /// Sorts the enemiesInView based on their distance from the camera.
        /// </summary>
        /// <param name="pos">The world-space position of the camera.</param>
        /// <returns>The amount of enemies that are valid for selection as target.</returns>
        private int SortEnemiesByDistanceWithinMaximum()
        {
            if (targetsInView.Count <= 1) //if there are one or zero enemies visible, stop.
                return targetsInView.Count;

            //the first element should be the one with the least distance from the camera.
            targetsInView.Sort((a, b) =>  a.sqrDistance < b.sqrDistance ? -1 : 1); 

            float maxDistSqr = maxEnemyDistance * maxEnemyDistance;
            //count how many enemies are within the maximum allowed distance (sqr dist because its faster)
            for(int i = 0; i < targetsInView.Count; i++)
            {
                if (targetsInView[i].sqrDistance > maxDistSqr)
                    return i;
            }
            return targetsInView.Count;
        }

        /// <summary>
        /// Sorts enemies based on their calculated weight
        /// </summary>
        /// <param name="countToSort">the amount of items to sort, given by SortEnemiesByDistanceWithinMaximum</param>
        private void SortEnemiesByWeight(int countToSort)
        {
            if (countToSort <= 1) //sorting one element is useless, sorting zero impossible.
                return;
            //Vector3 right = anchor.right;
            targetsInView.Sort(0, countToSort, new EnemyTargetComparer(EnemyTargetComparer.CompareMode.Weight));
        }

        /// <summary>
        /// Sorts enemies based on their horizontal alignment with the camera/player
        /// </summary>
        /// <param name="countToSort">amount of items to sort</param>
        private void SortEnemiesByAlignment(int countToSort)
        {
            if (countToSort <= 1)
                return;
            targetsInView.Sort(0, countToSort, new EnemyTargetComparer(EnemyTargetComparer.CompareMode.Alignment));
        }

        /// <summary>
        /// Gets all enemies from enemiesInView that arent blocked by line of sight.
        /// </summary>
        /// <param name="output">the list to populate</param>
        /// <param name="countToCheck">how many enemies to check; see SortEnemiesByDistanceWithinMaximum</param>
        /// <param name="pos">camera position</param>
        /// <returns>the amount of objects in the output array.</returns>
        private int GetEnemiesInLineOfSightNonAlloc(List<EnemyTarget> output, int countToCheck, Vector3 pos)
        {
            int count = 0;
            output.Clear();
            for(int i = 0; i < countToCheck; i++)
            {
                //skip all the ones where something is between the enemy and the camera.
                if (Physics.Linecast(pos, targetsInView[i].position, cameraCollisionMask, QueryTriggerInteraction.Ignore))
                    continue;
                output.Add(targetsInView[i]);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Compares two EnemyBase objects based on their relative position to the camera using the Dot Product of the offset and the cmameras right vector.
        /// </summary>
        private struct EnemyTargetComparer : IComparer<EnemyTarget> //struct because this gets created for a frame and then discarded again.
        {
            private CompareMode mode;
            public EnemyTargetComparer(CompareMode compareMode) => mode = compareMode;

            public int Compare(EnemyTarget a, EnemyTarget b)
            {
                //Old1: sorts by left to right.
                //Old2: sort by absolute left ro right (center to outside) -> element 0 will be closest to center of screen.
                //New: use the weight to sort them, the closest to player/center will be element 0
                if (mode is CompareMode.Weight)
                    return (a.weight > b.weight) ? 1 : -1;
                else //sort based on horizontal alignment [..., -1, 0, 1, ...] in "screenspace"
                    return (a.horizontalAlignment > b.horizontalAlignment) ? 1 : -1;
            }

            public enum CompareMode { Weight, Alignment };
        }

        //Could be a struct instead, but i felt like a reference type might actually be more useful here.
        private class EnemyTarget
        {
            public EnemyBase enemy;
            public Transform transform;

            //These will need to be recalculated once before sorting.
            public Vector3 position;
            public float sqrDistance;
            public float horizontalAlignment;
            public float weight;

            /// <summary>
            /// Recalculate position, sqrDistance, and horizontalAlignment based on the cameras position and right vector.
            /// </summary>
            /// <param name="camPos">the worldspace position of the camera</param>
            /// <param name="camRight">the cameras local right vector in worldspace</param>
            public void RecalculateParams(Vector3 camPos, Vector3 camRight)
            {
                position = transform.position;
                Vector3 offset = position - camPos;
                sqrDistance = Vector3.SqrMagnitude(offset);
                horizontalAlignment = Vector3.Dot(camRight, offset);

                //"weight" determines the order in which enemytargets are sorted later on.
                //1/4sqrDistance * max(0.75, alignment)
                weight = sqrDistance * 0.25f * Mathf.Max(.75f, Mathf.Abs(horizontalAlignment));
            }

            /// <summary>
            /// Initialize this EnemyTarget as one that contains the specified Enemy.
            /// </summary>
            /// <returns>itself</returns>
            public EnemyTarget As(EnemyBase enemy)
            {
                this.enemy = enemy;
                this.transform = enemy.transform;
                return this;
            }
        }
    }
}